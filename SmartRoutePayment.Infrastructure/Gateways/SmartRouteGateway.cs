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
    /// <summary>
    /// SmartRoute Gateway for Direct Post Payment Model
    /// Handles server-to-server communication with SmartRoute Payment Gateway
    /// </summary>
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

        /// <summary>
        /// Process Direct Post Payment through SmartRoute Gateway
        /// </summary>
        public async Task<PaymentResponse> ProcessPaymentAsync(
            PaymentRequest paymentRequest,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate request
                ValidatePaymentRequest(paymentRequest);

                // Build request parameters
                var parameters = BuildRequestParameters(paymentRequest);

                // Generate secure hash (card details are excluded automatically by SecureHashGenerator)
                var secureHash = _secureHashGenerator.Generate(parameters, _settings.AuthenticationToken);
                parameters.Add("SecureHash", secureHash);

                // Convert to form data
                var formContent = new FormUrlEncodedContent(parameters);

                // Send request to SmartRoute API
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
            catch (ArgumentException ex)
            {
                return CreateErrorResponse(
                    paymentRequest.TransactionId,
                    "VALIDATION_ERROR",
                    ex.Message);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(
                    paymentRequest.TransactionId,
                    "SYSTEM_ERROR",
                    $"Unexpected error: {ex.Message}");
            }
        }

        private void ValidatePaymentRequest(PaymentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TransactionId))
                throw new ArgumentException("TransactionId is required");

            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            if (request.MessageId <= 0)
                throw new ArgumentException("MessageId is required (1=Payment, 2=PreAuth, 3=Verify)");

            // Validate card details for Direct Post Payment
            if (string.IsNullOrWhiteSpace(request.CardNumber))
                throw new ArgumentException("CardNumber is required");

            if (string.IsNullOrWhiteSpace(request.ExpiryDateYear))
                throw new ArgumentException("ExpiryDateYear is required");

            if (string.IsNullOrWhiteSpace(request.ExpiryDateMonth))
                throw new ArgumentException("ExpiryDateMonth is required");

            if (string.IsNullOrWhiteSpace(request.SecurityCode))
                throw new ArgumentException("SecurityCode is required");

            if (string.IsNullOrWhiteSpace(request.CardHolderName))
                throw new ArgumentException("CardHolderName is required");
        }

        private Dictionary<string, string> BuildRequestParameters(PaymentRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                // Core Transaction Parameters
                { "TransactionID", request.TransactionId },
                { "MerchantID", _settings.MerchantId },
                { "Amount", ((int)request.Amount).ToString() }, // Amount in fils/cents (smallest currency unit)
                { "CurrencyISOCode", _settings.CurrencyIsoCode },
                { "MessageID", request.MessageId.ToString() }, // 1=Payment, 2=PreAuth, 3=Verify
                { "Quantity", _settings.Quantity.ToString() },
                { "Channel", _settings.Channel.ToString() }, // 0=Web, 1=Mobile, 2=CallCenter
                { "PaymentMethod", request.PaymentMethod.ToString() }, // 1=Card, 2=Sadad, etc.
                
                // UI Configuration
                { "Language", _settings.Language }, // en or ar
                { "ThemeID", _settings.ThemeId },
                { "Version", _settings.Version },

                // Card Details (IMPORTANT: Use correct field names with Card. prefix)
                { "CardNumber", request.CardNumber },
                { "ExpiryDateYear", request.ExpiryDateYear },
                { "ExpiryDateMonth", request.ExpiryDateMonth },
                { "SecurityCode", request.SecurityCode },
                { "CardHolderName", request.CardHolderName }
            };

            // Add optional ResponseBackURL if provided
            if (!string.IsNullOrWhiteSpace(request.ResponseBackURL))
            {
                parameters.Add("ResponseBackURL", request.ResponseBackURL);
            }

            // Add optional PaymentDescription if provided
            if (!string.IsNullOrWhiteSpace(request.PaymentDescription))
            {
                parameters.Add("PaymentDescription", request.PaymentDescription);
            }

            // Add optional ItemId if provided
            if (!string.IsNullOrWhiteSpace(request.ItemId))
            {
                parameters.Add("ItemId", request.ItemId);
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
                MessageId = GetResponseValue(responsePairs, "Response.MessageID"),
                TransactionId = GetResponseValue(responsePairs, "Response.TransactionID", transactionId),
                StatusCode = GetResponseValue(responsePairs, "Response.StatusCode", "ERROR"),
                StatusDescription = GetResponseValue(responsePairs, "Response.StatusDescription", "Unknown error"),
                GatewayStatusCode = GetResponseValue(responsePairs, "Response.GatewayStatusCode"),
                GatewayName = GetResponseValue(responsePairs, "Response.GatewayName"),
                GatewayStatusDescription = GetResponseValue(responsePairs, "Response.GatewayStatusDescription"),
                Amount = GetResponseValue(responsePairs, "Response.Amount"),
                ApprovalCode = GetResponseValue(responsePairs, "Response.ApprovalCode"),
                CardExpiryDate = GetResponseValue(responsePairs, "Response.CardExpiryDate"),
                CardHolderName = GetResponseValue(responsePairs, "Response.CardHolderName"),
                CurrencyIsoCode = GetResponseValue(responsePairs, "Response.CurrencyISOCode"),
                CardNumber = GetResponseValue(responsePairs, "Response.CardNumber"), // Masked by SmartRoute
                MerchantId = GetResponseValue(responsePairs, "Response.MerchantID"),
                Rrn = GetResponseValue(responsePairs, "Response.RRN"),
                SecureHash = GetResponseValue(responsePairs, "Response.SecureHash"),
                Token = GetResponseValue(responsePairs, "Response.Token"),
                IssuerName = GetResponseValue(responsePairs, "Response.IssuerName"),
                PaymentMethod = GetResponseValue(responsePairs, "Response.PaymentMethod"),
                ProcessedAt = DateTime.UtcNow
            };

            // Validate secure hash
            var receivedHash = paymentResponse.SecureHash;

            // Build parameters for hash validation (exclude Response.SecureHash)
            var parametersForValidation = responsePairs
                .Where(p => p.Key.StartsWith("Response.", StringComparison.OrdinalIgnoreCase)
                         && !p.Key.Equals("Response.SecureHash", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);

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
            if (string.IsNullOrWhiteSpace(responseContent))
                return new Dictionary<string, string>();

            return responseContent
                .Split('&')
                .Select(pair => pair.Split('='))
                .Where(parts => parts.Length == 2)
                .ToDictionary(
                    parts => Uri.UnescapeDataString(parts[0]),
                    parts => Uri.UnescapeDataString(parts[1]),
                    StringComparer.Ordinal);
        }

        private static string GetResponseValue(
            Dictionary<string, string> responsePairs,
            string key,
            string defaultValue = "")
        {
            return responsePairs.TryGetValue(key, out var value) ? value : defaultValue;
        }

        private static PaymentResponse CreateErrorResponse(
            string transactionId,
            string statusCode,
            string errorMessage)
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
