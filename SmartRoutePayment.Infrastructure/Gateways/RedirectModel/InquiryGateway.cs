using Microsoft.Extensions.Options;
using SmartRoutePayment.Domain.Entities.RedirectModel;
using SmartRoutePayment.Domain.Interfaces;
using SmartRoutePayment.Infrastructure.Configuration;
using SmartRoutePayment.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Infrastructure.Gateways.RedirectModel
{
    /// <summary>
    /// Gateway implementation for Transaction Inquiry operations
    /// Implements B2B API Communication Model
    /// </summary>
    public class InquiryGateway : IInquiryGateway
    {
        private readonly HttpClient _httpClient;
        private readonly SmartRouteSettings _settings;
        private readonly RedirectSecureHashGenerator _secureHashGenerator;

        public InquiryGateway(
            HttpClient httpClient,
            IOptions<SmartRouteSettings> settings,
            RedirectSecureHashGenerator secureHashGenerator)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _secureHashGenerator = secureHashGenerator ?? throw new ArgumentNullException(nameof(secureHashGenerator));
        }

        /// <summary>
        /// Inquires about the status of a specific transaction
        /// </summary>
        public async Task<InquiryResponse> InquireTransactionAsync(
            InquiryRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Build request parameters
                var parameters = BuildRequestParameters(request);

                // Generate secure hash
                var secureHash = _secureHashGenerator.Generate(parameters, _settings.AuthenticationToken);
                parameters.Add("SecureHash", secureHash);

                // Convert to form data
                var formContent = new FormUrlEncodedContent(parameters);

                // Send request to SmartRoute Inquiry endpoint
                var response = await _httpClient.PostAsync(
                    _settings.InquiryUrl,
                    formContent,
                    cancellationToken);

                // Read response content
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                // Parse and validate response
                return ParseAndValidateResponse(responseContent, request.OriginalTransactionId);
            }
            catch (HttpRequestException ex)
            {
                return CreateErrorResponse(
                    request.OriginalTransactionId,
                    "HTTP_ERROR",
                    $"Connection error: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                return CreateErrorResponse(
                    request.OriginalTransactionId,
                    "TIMEOUT",
                    "Request timeout");
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(
                    request.OriginalTransactionId,
                    "SYSTEM_ERROR",
                    $"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds request parameters for inquiry
        /// Fills in configuration values from settings
        /// </summary>
        private Dictionary<string, string> BuildRequestParameters(InquiryRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                { "MessageID", request.MessageId },
                { "MerchantID", _settings.MerchantId },
                { "OriginalTransactionID", request.OriginalTransactionId },
                { "Version", _settings.Version }
            };

            // Add optional parameters if provided
            if (!string.IsNullOrWhiteSpace(request.IncludeRefundIds))
                parameters.Add("IncludeRefundIds", request.IncludeRefundIds);

            return parameters;
        }

        /// <summary>
        /// Parses and validates inquiry response
        /// </summary>
        private InquiryResponse ParseAndValidateResponse(string responseContent, string transactionId)
        {
            // Parse form-encoded response
            var responsePairs = ParseFormEncodedResponse(responseContent);

            // Extract response parameters
            var inquiryResponse = new InquiryResponse
            {
                MessageStatus = responsePairs.GetValueOrDefault("Response.MessageStatus", string.Empty),
                StatusCode = responsePairs.GetValueOrDefault("Response.StatusCode", string.Empty),
                Amount = responsePairs.GetValueOrDefault("Response.Amount", string.Empty),
                CurrencyIsoCode = responsePairs.GetValueOrDefault("Response.CurrencyISOCode", string.Empty),
                MerchantId = responsePairs.GetValueOrDefault("Response.MerchantID", string.Empty),
                TransactionId = responsePairs.GetValueOrDefault("Response.TransactionID", transactionId),
                MessageId = responsePairs.GetValueOrDefault("Response.MessageID", string.Empty),
                ReversalStatus = responsePairs.GetValueOrDefault("Response.ReversalStatus", string.Empty),
                SecureHash = responsePairs.GetValueOrDefault("Response.SecureHash", string.Empty),
                PaymentMethod = responsePairs.GetValueOrDefault("Response.PaymentMethod", string.Empty),
                AuthorizedAmount = responsePairs.GetValueOrDefault("Response.AuthorizedAmount"),
                AuthorizedCurrencyIsoCode = responsePairs.GetValueOrDefault("Response.AuthorizedCurrencyISOCode"),
                GatewayStatusCode = responsePairs.GetValueOrDefault("Response.GatewayStatusCode"),
                GatewayStatusDescription = responsePairs.GetValueOrDefault("Response.GatewayStatusDescription"),
                GatewayName = responsePairs.GetValueOrDefault("Response.GatewayName"),
                Rrn = responsePairs.GetValueOrDefault("Response.RRN"),
                ApprovalCode = responsePairs.GetValueOrDefault("Response.ApprovalCode"),
                CardExpiryDate = responsePairs.GetValueOrDefault("Response.CardExpiryDate"),
                CardHolderName = responsePairs.GetValueOrDefault("Response.CardHolderName"),
                CardNumber = responsePairs.GetValueOrDefault("Response.CardNumber"),
                RefundStatus = responsePairs.GetValueOrDefault("Response.RefundStatus"),
                RefundIds = responsePairs.GetValueOrDefault("Response.RefundIds"),
                IssuerName = responsePairs.GetValueOrDefault("Response.IssuerName"),
                ProcessedAt = DateTime.UtcNow
            };

            // Validate secure hash
            var receivedHash = inquiryResponse.SecureHash;
            var isHashValid = _secureHashGenerator.Validate(
                responsePairs,
                receivedHash,
                _settings.AuthenticationToken);

            if (!isHashValid)
            {
                inquiryResponse.IsSuccess = false;
                inquiryResponse.ErrorMessage = "Invalid secure hash - response may be tampered";
                return inquiryResponse;
            }

            // Check if inquiry was successful
            inquiryResponse.IsSuccess = inquiryResponse.MessageStatus == "00000";
            inquiryResponse.ErrorMessage = inquiryResponse.IsSuccess
                ? string.Empty
                : inquiryResponse.MessageStatus;

            return inquiryResponse;
        }

        /// <summary>
        /// Parses form-encoded response string
        /// </summary>
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

        /// <summary>
        /// Creates error response for inquiry
        /// </summary>
        private static InquiryResponse CreateErrorResponse(
            string transactionId,
            string statusCode,
            string errorMessage)
        {
            return new InquiryResponse
            {
                TransactionId = transactionId,
                MessageStatus = statusCode,
                StatusCode = statusCode,
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }
}
