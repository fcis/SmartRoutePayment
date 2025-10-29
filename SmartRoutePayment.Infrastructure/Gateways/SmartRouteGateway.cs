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
                _logger.LogDebug("=== REQUEST PARAMETERS (Before Hash) ===");
                foreach (var param in parameters.OrderBy(p => p.Key, StringComparer.Ordinal))
                {
                    // Don't log sensitive card data
                    var value = param.Key.Contains("Card") || param.Key.Contains("Security")
                        ? "****"
                        : param.Value;
                    _logger.LogDebug("{Key} = {Value}", param.Key, value);
                }

                // Generate secure hash (card details are excluded automatically by SecureHashGenerator)
                var secureHash = _secureHashGenerator.Generate(parameters, _settings.AuthenticationToken);

                // === DEBUG: Log generated hash ===
                _logger.LogDebug("Generated Request SecureHash: {Hash}", secureHash);

                parameters.Add("SecureHash", secureHash);

                // Convert to form data
                var formContent = new FormUrlEncodedContent(parameters);

                // === DEBUG: Log request ===
                _logger.LogInformation("Sending request to SmartRoute: {Url}", _settings.ApiUrl);

                // Send request to SmartRoute API
                var response = await _httpClient.PostAsync(
                    _settings.ApiUrl,
                    formContent,
                    cancellationToken);

                // Read response content
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                // === DEBUG: Log raw response ===
                _logger.LogDebug("=== RAW RESPONSE FROM SMARTROUTE ===");
                _logger.LogDebug("Status Code: {StatusCode}", response.StatusCode);
                _logger.LogDebug("{Response}", responseContent);

                // Check for HTTP errors
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("SmartRoute returned HTTP {StatusCode}", response.StatusCode);
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
            // CRITICAL: Amount must be in smallest currency unit (fils/cents) without decimal point
            // For SAR: 1.50 SAR = 150 halalas
            // For USD: 1.50 USD = 150 cents
            var amountInSmallestUnit = (request.Amount * 100).ToString("F0");

            var parameters = new Dictionary<string, string>
            {
                // Core Transaction Parameters
                { "TransactionID", request.TransactionId },
                { "MerchantID", _settings.MerchantId },
                { "Amount", amountInSmallestUnit }, // Amount in fils/cents (smallest currency unit)
                { "CurrencyISOCode", _settings.CurrencyIsoCode },
                { "MessageID", request.MessageId.ToString() }, // 1=Payment, 2=PreAuth, 3=Verify
                { "Quantity", request.Quantity > 0 ? request.Quantity.ToString() : _settings.Quantity.ToString() },
                { "Channel", request.Channel >= 0 ? request.Channel.ToString() : _settings.Channel.ToString() }, // 0=Web, 1=Mobile, 2=CallCenter
                { "PaymentMethod", request.PaymentMethod.ToString() }, // 1=Card, 2=Sadad, etc.
                
                // UI Configuration
                { "Language", _settings.Language }, // en or ar
                { "ThemeID", _settings.ThemeId },
                { "Version", _settings.Version },

                // Card Details (excluded from hash by SecureHashGenerator)
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

            // Add optional PaymentDescription if provided (MUST be UTF-8 encoded)
            if (!string.IsNullOrWhiteSpace(request.PaymentDescription))
            {
                parameters.Add("PaymentDescription", request.PaymentDescription);
            }

            // Add optional ItemId if provided
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
                CardNumber = GetResponseValue(responsePairs, "Response.CardNumber"), // Masked by SmartRoute
                MerchantId = GetResponseValue(responsePairs, "Response.MerchantID"),
                Rrn = GetResponseValue(responsePairs, "Response.RRN"),
                SecureHash = GetResponseValue(responsePairs, "Response.SecureHash"),
                Token = GetResponseValue(responsePairs, "Response.Token"),
                IssuerName = GetResponseValue(responsePairs, "Response.IssuerName"),
                PaymentMethod = GetResponseValue(responsePairs, "Response.PaymentMethod"),
                ProcessedAt = DateTime.UtcNow
            };

            // === DEBUG: Check if StatusCode is 00018 (hash mismatch from SmartRoute) ===
            if (paymentResponse.StatusCode == "00018")
            {
                _logger.LogError("SmartRoute returned 00018 - REQUEST hash was invalid!");
                _logger.LogError("This means the hash WE SENT was wrong, not the response hash.");
                paymentResponse.IsSuccess = false;
                paymentResponse.ErrorMessage = "Secure hash validation failed on SmartRoute side (Error 00018)";
                return paymentResponse;
            }

            // CRITICAL: Validate secure hash
            // According to SmartRoute .NET sample code, StatusDescription and GatewayStatusDescription
            // MUST be URL-encoded when building the hash validation string
            var receivedHash = paymentResponse.SecureHash;

            // === DEBUG: Log received hash ===
            _logger.LogDebug("Received Response SecureHash: {Hash}", receivedHash);

            // Build parameters for hash validation (exclude Response.SecureHash)
            // CRITICAL: URL-encode StatusDescription and GatewayStatusDescription as per SmartRoute docs
            var parametersForValidation = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var pair in responsePairs.Where(p => p.Key.StartsWith("Response.", StringComparison.OrdinalIgnoreCase)
                                                       && !p.Key.Equals("Response.SecureHash", StringComparison.OrdinalIgnoreCase)))
            {
                // CRITICAL: URL-encode these specific fields for hash validation
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

            // === DEBUG: Log parameters used for validation ===
            _logger.LogDebug("=== PARAMETERS FOR HASH VALIDATION (with URL encoding) ===");
            foreach (var param in parametersForValidation.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                _logger.LogDebug("{Key} = {Value}", param.Key, param.Value);
            }

            var computedHash = _secureHashGenerator.Generate(parametersForValidation, _settings.AuthenticationToken);

            // === DEBUG: Compare hashes ===
            _logger.LogDebug("Computed Hash from Response: {ComputedHash}", computedHash);
            _logger.LogDebug("Received Hash from SmartRoute: {ReceivedHash}", receivedHash);
            _logger.LogDebug("Hashes Match: {Match}",
                string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase));

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
