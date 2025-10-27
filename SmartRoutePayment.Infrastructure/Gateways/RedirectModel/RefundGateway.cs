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
    /// Gateway implementation for Transaction Refund operations
    /// Implements B2B API Communication Model
    /// </summary>
    public class RefundGateway : IRefundGateway
    {
        private readonly HttpClient _httpClient;
        private readonly SmartRouteSettings _settings;
        private readonly RedirectSecureHashGenerator _secureHashGenerator;

        public RefundGateway(
            HttpClient httpClient,
            IOptions<SmartRouteSettings> settings,
            RedirectSecureHashGenerator secureHashGenerator)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _secureHashGenerator = secureHashGenerator ?? throw new ArgumentNullException(nameof(secureHashGenerator));
        }

        /// <summary>
        /// Refunds a specific transaction partially or fully
        /// </summary>
        public async Task<RefundResponse> RefundTransactionAsync(
            RefundRequest request,
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

                // Send request to SmartRoute Refund endpoint
                var response = await _httpClient.PostAsync(
                    _settings.RefundUrl,
                    formContent,
                    cancellationToken);

                // Read response content
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                // Parse and validate response
                return ParseAndValidateResponse(responseContent, request.TransactionId, request.OriginalTransactionId);
            }
            catch (HttpRequestException ex)
            {
                return CreateErrorResponse(
                    request.TransactionId,
                    request.OriginalTransactionId,
                    "HTTP_ERROR",
                    $"Connection error: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                return CreateErrorResponse(
                    request.TransactionId,
                    request.OriginalTransactionId,
                    "TIMEOUT",
                    "Request timeout");
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(
                    request.TransactionId,
                    request.OriginalTransactionId,
                    "SYSTEM_ERROR",
                    $"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds request parameters for refund
        /// Fills in configuration values from settings
        /// </summary>
        private Dictionary<string, string> BuildRequestParameters(RefundRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                { "MessageID", request.MessageId },
                { "TransactionID", request.TransactionId },
                { "MerchantID", _settings.MerchantId },
                { "CurrencyISOCode", _settings.CurrencyIsoCode },
                { "Amount", request.Amount },
                { "Version", _settings.Version },
                { "OriginalTransactionID", request.OriginalTransactionId }
            };

            // Add optional parameters if provided
            if (!string.IsNullOrWhiteSpace(request.SubPun))
                parameters.Add("SubPUN", request.SubPun);

            return parameters;
        }

        /// <summary>
        /// Parses and validates refund response
        /// </summary>
        private RefundResponse ParseAndValidateResponse(
            string responseContent,
            string transactionId,
            string originalTransactionId)
        {
            // Parse form-encoded response
            var responsePairs = ParseFormEncodedResponse(responseContent);

            // Extract response parameters
            var refundResponse = new RefundResponse
            {
                StatusCode = responsePairs.GetValueOrDefault("Response.StatusCode", string.Empty),
                StatusDescription = responsePairs.GetValueOrDefault("Response.StatusDescription", string.Empty),
                Amount = responsePairs.GetValueOrDefault("Response.Amount", string.Empty),
                CurrencyIsoCode = responsePairs.GetValueOrDefault("Response.CurrencyISOCode", string.Empty),
                MerchantId = responsePairs.GetValueOrDefault("Response.MerchantID", string.Empty),
                TransactionId = responsePairs.GetValueOrDefault("Response.TransactionID", transactionId),
                SubPun = responsePairs.GetValueOrDefault("Response.SubPUN"),
                MessageId = responsePairs.GetValueOrDefault("Response.MessageID", string.Empty),
                SecureHash = responsePairs.GetValueOrDefault("Response.SecureHash", string.Empty),
                OriginalTransactionId = responsePairs.GetValueOrDefault("Response.OriginalTransactionID", originalTransactionId),
                Rrn = responsePairs.GetValueOrDefault("Response.RRN"),
                ProcessedAt = DateTime.UtcNow
            };

            // Validate secure hash
            var receivedHash = refundResponse.SecureHash;
            var isHashValid = _secureHashGenerator.Validate(
                responsePairs,
                receivedHash,
                _settings.AuthenticationToken);

            if (!isHashValid)
            {
                refundResponse.IsSuccess = false;
                refundResponse.ErrorMessage = "Invalid secure hash - response may be tampered";
                return refundResponse;
            }

            // Check if refund was successful (00000 = Success)
            refundResponse.IsSuccess = refundResponse.StatusCode == "00000";
            refundResponse.ErrorMessage = refundResponse.IsSuccess
                ? string.Empty
                : refundResponse.StatusDescription;

            return refundResponse;
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
        /// Creates error response for refund
        /// </summary>
        private static RefundResponse CreateErrorResponse(
            string transactionId,
            string originalTransactionId,
            string statusCode,
            string errorMessage)
        {
            return new RefundResponse
            {
                TransactionId = transactionId,
                OriginalTransactionId = originalTransactionId,
                StatusCode = statusCode,
                StatusDescription = errorMessage,
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }
}
