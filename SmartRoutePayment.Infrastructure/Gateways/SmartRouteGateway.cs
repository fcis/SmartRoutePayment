using Microsoft.Extensions.Options;
using SmartRoutePayment.Domain.Entities;
using SmartRoutePayment.Domain.Interfaces;
using SmartRoutePayment.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Infrastructure.Gateways
{
    public class SmartRouteGateway : ISmartRouteGateway
    {
        private readonly HttpClient _httpClient;
        private readonly SmartRouteSettings _settings;
        private readonly ISecureHashGenerator _secureHashGenerator;

        public SmartRouteGateway(
            HttpClient httpClient,
            IOptions<SmartRouteSettings> settings,
            ISecureHashGenerator secureHashGenerator)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _secureHashGenerator = secureHashGenerator ?? throw new ArgumentNullException(nameof(secureHashGenerator));
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(
            PaymentRequest paymentRequest,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Build request parameters (sorted dictionary for consistent ordering)
                var parameters = BuildRequestParameters(paymentRequest);

                // Generate secure hash (excluding card details)
                var secureHash = _secureHashGenerator.Generate(parameters, _settings.AuthenticationToken);
                parameters.Add("SecureHash", secureHash);

                // Convert to form data
                var formContent = new FormUrlEncodedContent(parameters);

                // Send request to SmartRoute
                var response = await _httpClient.PostAsync(
                    _settings.ApiUrl,
                    formContent,
                    cancellationToken);

                // Read response content
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                // Parse and validate response
                return ParseAndValidateResponse(responseContent, paymentRequest.TransactionId);
            }
            catch (HttpRequestException ex)
            {
                return CreateErrorResponse(
                    paymentRequest.TransactionId,
                    "HTTP_ERROR",
                    $"Connection error: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                return CreateErrorResponse(
                    paymentRequest.TransactionId,
                    "TIMEOUT",
                    "Request timeout");
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(
                    paymentRequest.TransactionId,
                    "SYSTEM_ERROR",
                    $"Unexpected error: {ex.Message}");
            }
        }

        private Dictionary<string, string> BuildRequestParameters(PaymentRequest request)
        {
            var parameters = new Dictionary<string, string>
        {
            { "TransactionID", request.TransactionId },
            { "MerchantID", _settings.MerchantId },
            { "Amount", ((int)(request.Amount * 100)).ToString() }, // Convert to smallest currency unit (fils/cents)
            { "CurrencyISOCode", _settings.CurrencyIsoCode },
            { "MessageID", "1" }, // 1 = Direct Post Payment
            { "Quantity", _settings.Quantity.ToString() },
            { "Channel", _settings.Channel.ToString() },
            { "PaymentMethod", request.PaymentMethod.ToString() },
            { "Language", _settings.Language },
            { "ThemeID", _settings.ThemeId },
            { "Version", _settings.Version },
            // Card details (NOT included in secure hash)
            { "CardNumber", request.CardNumber },
            { "ExpiryDateYear", request.ExpiryDateYear },
            { "ExpiryDateMonth", request.ExpiryDateMonth },
            { "SecurityCode", request.SecurityCode },
            { "CardHolderName", request.CardHolderName }
        };

            // Add optional ResponseBackURL if configured
            if (!string.IsNullOrWhiteSpace(request.ResponseBackURL))
            {
                parameters.Add("ResponseBackURL", request.ResponseBackURL);
            }

            return parameters;
        }

        private PaymentResponse ParseAndValidateResponse(string responseContent, string transactionId)
        {
            // Parse form-encoded response
            var responsePairs = ParseFormEncodedResponse(responseContent);

            // Extract response parameters
            var paymentResponse = new PaymentResponse
            {
                MessageId = responsePairs.GetValueOrDefault("Response.MessageID", string.Empty),
                TransactionId = responsePairs.GetValueOrDefault("Response.TransactionID", transactionId),
                StatusCode = responsePairs.GetValueOrDefault("Response.StatusCode", "ERROR"),
                StatusDescription = responsePairs.GetValueOrDefault("Response.StatusDescription", "Unknown error"),
                GatewayStatusCode = responsePairs.GetValueOrDefault("Response.GatewayStatusCode", string.Empty),
                GatewayName = responsePairs.GetValueOrDefault("Response.GatewayName", string.Empty),
                GatewayStatusDescription = responsePairs.GetValueOrDefault("Response.GatewayStatusDescription", string.Empty),
                Amount = responsePairs.GetValueOrDefault("Response.Amount", string.Empty),
                ApprovalCode = responsePairs.GetValueOrDefault("Response.ApprovalCode", string.Empty),
                CardExpiryDate = responsePairs.GetValueOrDefault("Response.CardExpiryDate", string.Empty),
                CardHolderName = responsePairs.GetValueOrDefault("Response.CardHolderName", string.Empty),
                CurrencyIsoCode = responsePairs.GetValueOrDefault("Response.CurrencyISOCode", string.Empty),
                CardNumber = responsePairs.GetValueOrDefault("Response.CardNumber", string.Empty),
                MerchantId = responsePairs.GetValueOrDefault("Response.MerchantID", string.Empty),
                Rrn = responsePairs.GetValueOrDefault("Response.RRN", string.Empty),
                SecureHash = responsePairs.GetValueOrDefault("Response.SecureHash", string.Empty),
                ProcessedAt = DateTime.UtcNow
            };

            // Validate secure hash
            var receivedHash = paymentResponse.SecureHash;
            var parametersForValidation = responsePairs
                .Where(p => p.Key.StartsWith("Response.") && p.Key != "Response.SecureHash")
                .ToDictionary(p => p.Key, p => p.Value);

            var isHashValid = _secureHashGenerator.Validate(
                parametersForValidation,
                receivedHash,
                _settings.AuthenticationToken);

            if (!isHashValid)
            {
                paymentResponse.IsSuccess = false;
                paymentResponse.ErrorMessage = "Invalid secure hash - response may be tampered";
                return paymentResponse;
            }

            // Check if payment was successful (00000 = Success)
            paymentResponse.IsSuccess = paymentResponse.StatusCode == "00000";
            paymentResponse.ErrorMessage = paymentResponse.IsSuccess
                ? string.Empty
                : paymentResponse.StatusDescription;

            return paymentResponse;
        }

        private static Dictionary<string, string> ParseFormEncodedResponse(string responseContent)
        {
            return responseContent
                .Split('&')
                .Select(pair => pair.Split('='))
                .Where(parts => parts.Length == 2)
                .ToDictionary(
                    parts => parts[0],
                    parts => Uri.UnescapeDataString(parts[1]),
                    StringComparer.Ordinal);
        }

        private static PaymentResponse CreateErrorResponse(string transactionId, string statusCode, string errorMessage)
        {
            return new PaymentResponse
            {
                TransactionId = transactionId,
                StatusCode = statusCode,
                StatusDescription = errorMessage,
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }
}
