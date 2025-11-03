using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartRoutePayment.Domain.Entities;
using SmartRoutePayment.Domain.Interfaces;
using SmartRoutePayment.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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
        private readonly ILogger<SmartRouteGateway> _logger;

        public SmartRouteGateway(
            HttpClient httpClient,
            IOptions<SmartRouteSettings> settings,
            ISecureHashGenerator secureHashGenerator,
            ILogger<SmartRouteGateway> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _secureHashGenerator = secureHashGenerator ?? throw new ArgumentNullException(nameof(secureHashGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                // === DEBUG: Log parameters BEFORE hash generation ===
                _logger.LogInformation("=== REQUEST PARAMETERS (Before Hash) ===");
                foreach (var param in parameters.OrderBy(p => p.Key, StringComparer.Ordinal))
                {
                    // Mask sensitive card data in logs
                    var value = param.Key.Contains("Card") || param.Key.Contains("Security")
                        ? "****"
                        : param.Value;
                    _logger.LogInformation("  {Key} = {Value}", param.Key, value);
                }

                // Generate secure hash (card details are excluded automatically by SecureHashGenerator)
                var secureHash = _secureHashGenerator.Generate(parameters, _settings.AuthenticationToken);

                // === DEBUG: Log generated hash ===
                _logger.LogInformation("Generated Request SecureHash: {Hash}", secureHash);

                parameters.Add("SecureHash", secureHash);

                // Convert to form data
                var formContent = new FormUrlEncodedContent(parameters);

                // === CRITICAL: Log the actual encoded request body ===
                var requestBody = await formContent.ReadAsStringAsync();
                _logger.LogInformation("=== FULL ENCODED REQUEST BODY (for SmartRoute Support) ===");
                _logger.LogInformation("URL: {Url}", _settings.ApiUrl);
                _logger.LogInformation("Method: POST");
                _logger.LogInformation("Content-Type: application/x-www-form-urlencoded");
                _logger.LogInformation("Body Length: {Length} bytes", requestBody.Length);
                _logger.LogInformation("Body: {Body}", requestBody);
                _logger.LogInformation("========================================");

                // IMPORTANT: After reading the content, we need to recreate it
                // because the stream has been consumed
                formContent = new FormUrlEncodedContent(parameters);

                // Send request to SmartRoute API
                _logger.LogInformation("Sending request to SmartRoute...");
                var response = await _httpClient.PostAsync(
                    _settings.ApiUrl,
                    formContent,
                    cancellationToken);

                // Read response content
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                // === DEBUG: Log full response ===
                _logger.LogInformation("=== RAW RESPONSE FROM SMARTROUTE ===");
                _logger.LogInformation("HTTP Status: {StatusCode} ({StatusCodeInt})",
                    response.StatusCode, (int)response.StatusCode);
                _logger.LogInformation("Response Headers:");
                foreach (var header in response.Headers)
                {
                    _logger.LogInformation("  {Key}: {Value}", header.Key, string.Join(", ", header.Value));
                }
                _logger.LogInformation("Content Headers:");
                foreach (var header in response.Content.Headers)
                {
                    _logger.LogInformation("  {Key}: {Value}", header.Key, string.Join(", ", header.Value));
                }
                _logger.LogInformation("Response Body: {Response}", responseContent);
                _logger.LogInformation("========================================");

                // Check HTTP status
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("SmartRoute returned HTTP error: {StatusCode}", response.StatusCode);

                    // Special handling for 403 Forbidden
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        _logger.LogError("403 Forbidden - Possible causes:");
                        _logger.LogError("1. Incorrect API URL (check _settings.ApiUrl)");
                        _logger.LogError("2. IP address not whitelisted with SmartRoute");
                        _logger.LogError("3. Incorrect MerchantID or Authentication Token");
                        _logger.LogError("4. Merchant account not configured for Direct Post Payment");
                        _logger.LogError("5. Request not coming from expected source");
                        _logger.LogError("Current API URL: {Url}", _settings.ApiUrl);
                        _logger.LogError("Current MerchantID: {MerchantId}", _settings.MerchantId);
                    }

                    return CreateErrorResponse(
                        paymentRequest.TransactionId,
                        $"HTTP_{(int)response.StatusCode}",
                        $"SmartRoute returned HTTP {response.StatusCode}: {responseContent}");
                }

                // Parse and validate response
                return ParseAndValidateResponse(responseContent, paymentRequest.TransactionId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP connection error");
                return CreateErrorResponse(
                    paymentRequest.TransactionId,
                    "HTTP_ERROR",
                    $"Connection error: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout");
                return CreateErrorResponse(
                    paymentRequest.TransactionId,
                    "TIMEOUT",
                    "Request timeout");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error");
                return CreateErrorResponse(
                    paymentRequest.TransactionId,
                    "VALIDATION_ERROR",
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
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
            // Convert amount to smallest currency unit (fils/halalas for SAR)
            var amountInSmallestUnit = ((int)(request.Amount * 100)).ToString();

            var parameters = new Dictionary<string, string>
            {
                // Core Transaction Parameters
                { "TransactionID", request.TransactionId },
                { "MerchantID", _settings.MerchantId },
                { "Amount", amountInSmallestUnit },
                { "CurrencyISOCode", _settings.CurrencyIsoCode },
                { "MessageID", request.MessageId.ToString() },
                { "Quantity", request.Quantity > 0 ? request.Quantity.ToString() : _settings.Quantity.ToString() },
                { "Channel", request.Channel >= 0 ? request.Channel.ToString() : _settings.Channel.ToString() },
                { "PaymentMethod", request.PaymentMethod.ToString() },
                
                // UI Configuration
                { "Language", _settings.Language },
                { "ThemeID", _settings.ThemeId },
                { "Version", _settings.Version },

                // Card Details (excluded from hash automatically)
                { "CardNumber", request.CardNumber },
                { "ExpiryDateYear", request.ExpiryDateYear },
                { "ExpiryDateMonth", request.ExpiryDateMonth },
                { "SecurityCode", request.SecurityCode },
                { "CardHolderName", request.CardHolderName }
            };

            // Add optional parameters if provided
            if (!string.IsNullOrWhiteSpace(request.ResponseBackURL))
            {
                parameters.Add("ResponseBackURL", request.ResponseBackURL);
            }

            if (!string.IsNullOrWhiteSpace(request.PaymentDescription))
            {
                parameters.Add("PaymentDescription", request.PaymentDescription);
            }

            if (!string.IsNullOrWhiteSpace(request.ItemId))
            {
                parameters.Add("ItemID", request.ItemId);
            }

            return parameters;
        }

        private PaymentResponse ParseAndValidateResponse(string responseContent, string transactionId)
        {
            // Parse form-encoded response
            var responsePairs = ParseFormEncodedResponse(responseContent);

            // === DEBUG: Log parsed response ===
            _logger.LogDebug("=== PARSED RESPONSE PARAMETERS ===");
            foreach (var param in responsePairs.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                _logger.LogDebug("{Key} = {Value}", param.Key, param.Value);
            }

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
                CardNumber = GetResponseValue(responsePairs, "Response.CardNumber"),
                MerchantId = GetResponseValue(responsePairs, "Response.MerchantID"),
                Rrn = GetResponseValue(responsePairs, "Response.RRN"),
                SecureHash = GetResponseValue(responsePairs, "Response.SecureHash"),
                Token = GetResponseValue(responsePairs, "Response.Token"),
                IssuerName = GetResponseValue(responsePairs, "Response.IssuerName"),
                PaymentMethod = GetResponseValue(responsePairs, "Response.PaymentMethod"),
                ProcessedAt = DateTime.UtcNow
            };

            // Check if SmartRoute rejected our request hash (Error 00018)
            if (paymentResponse.StatusCode == "00018")
            {
                _logger.LogError("SmartRoute returned 00018 - Our REQUEST hash was invalid!");
                paymentResponse.IsSuccess = false;
                paymentResponse.ErrorMessage = "Secure hash validation failed on SmartRoute side (Error 00018)";
                return paymentResponse;
            }

            // === URL-encode specific fields for response validation ===
            var receivedHash = paymentResponse.SecureHash;
            _logger.LogDebug("Received Response SecureHash: {Hash}", receivedHash);

            // Build parameters for hash validation with URL encoding
            var parametersForValidation = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var pair in responsePairs.Where(p => p.Key.StartsWith("Response.", StringComparison.OrdinalIgnoreCase)
                                                       && !p.Key.Equals("Response.SecureHash", StringComparison.OrdinalIgnoreCase)))
            {
                // URL-encode StatusDescription and GatewayStatusDescription
                if (pair.Key.Equals("Response.StatusDescription", StringComparison.OrdinalIgnoreCase) ||
                    pair.Key.Equals("Response.GatewayStatusDescription", StringComparison.OrdinalIgnoreCase))
                {
                    parametersForValidation[pair.Key] = HttpUtility.UrlEncode(pair.Value, System.Text.Encoding.UTF8);
                }
                else
                {
                    parametersForValidation[pair.Key] = pair.Value;
                }
            }

            var computedHash = _secureHashGenerator.Generate(parametersForValidation, _settings.AuthenticationToken);

            _logger.LogDebug("Computed Hash: {ComputedHash}", computedHash);
            _logger.LogDebug("Received Hash: {ReceivedHash}", receivedHash);

            var isHashValid = _secureHashGenerator.Validate(
                parametersForValidation,
                receivedHash,
                _settings.AuthenticationToken);

            if (!isHashValid)
            {
                _logger.LogError("Response hash validation FAILED!");
                _logger.LogError("Expected: {Expected}, Received: {Received}", computedHash, receivedHash);

                paymentResponse.IsSuccess = false;
                paymentResponse.ErrorMessage = "Invalid secure hash - response may be tampered";
                return paymentResponse;
            }

            _logger.LogInformation("Response hash validation PASSED!");

            // Check payment success (00000 = Success)
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
