using SmartRoutePayment.Application.DTOs.Requests;
using SmartRoutePayment.Application.DTOs.Responses;
using SmartRoutePayment.Application.Interfaces;
using SmartRoutePayment.Domain.Entities;
using SmartRoutePayment.Domain.Interfaces;

namespace SmartRoutePayment.Application.Services
{
    /// <summary>
    /// Payment service for processing Direct Post Payment through SmartRoute/Payone
    /// Application layer - depends only on Domain interfaces
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly ISmartRouteGateway _smartRouteGateway;
        private readonly ISecureHashGenerator _secureHashGenerator;
        private readonly IPaymentConfigurationProvider _configurationProvider;

        public PaymentService(
            ISmartRouteGateway smartRouteGateway,
            ISecureHashGenerator secureHashGenerator,
            IPaymentConfigurationProvider configurationProvider)
        {
            _smartRouteGateway = smartRouteGateway ?? throw new ArgumentNullException(nameof(smartRouteGateway));
            _secureHashGenerator = secureHashGenerator ?? throw new ArgumentNullException(nameof(secureHashGenerator));
            _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
        }


        /// <summary>
        /// Handles payment callback from Payone
        /// Validates SecureHash and processes payment result
        /// </summary>
        public async Task<PaymentCallbackDto> HandlePaymentCallbackAsync(
            Dictionary<string, string> callbackData,
            CancellationToken cancellationToken = default)
        {
            if (callbackData == null || callbackData.Count == 0)
                throw new ArgumentException("Callback data cannot be null or empty", nameof(callbackData));

            // Extract SecureHash from callback data
            if (!callbackData.TryGetValue("SecureHash", out var receivedHash))
                throw new ArgumentException("SecureHash not found in callback data");

            // Validate SecureHash
            var isValidHash = _secureHashGenerator.Validate(
                callbackData,
                receivedHash,
                _configurationProvider.AuthenticationToken);

            if (!isValidHash)
            {
                throw new InvalidOperationException("Invalid SecureHash - callback data may have been tampered with");
            }

            // Map callback data to DTO
            var callback = new PaymentCallbackDto
            {
                TransactionId = GetValue(callbackData, "TransactionId"),
                MerchantId = GetValue(callbackData, "MerchantId"),
                MessageId = GetValue(callbackData, "MessageId"),
                Amount = GetValue(callbackData, "Amount"),
                CurrencyIsoCode = GetValue(callbackData, "CurrencyIsoCode"),
                StatusCode = GetValue(callbackData, "StatusCode"),
                StatusDescription = GetValue(callbackData, "StatusDescription"),
                ApprovalCode = GetValueOrNull(callbackData, "ApprovalCode"),
                GatewayName = GetValueOrNull(callbackData, "GatewayName"),
                GatewayStatusCode = GetValueOrNull(callbackData, "GatewayStatusCode"),
                GatewayStatusDescription = GetValueOrNull(callbackData, "GatewayStatusDescription"),
                CardNumber = GetValueOrNull(callbackData, "CardNumber"),
                CardExpiryDate = GetValueOrNull(callbackData, "CardExpiryDate"),
                CardHolderName = GetValueOrNull(callbackData, "CardHolderName"),
                Rrn = GetValueOrNull(callbackData, "Rrn"),
                Token = GetValueOrNull(callbackData, "Token"),
                IssuerName = GetValueOrNull(callbackData, "IssuerName"),
                SecureHash = receivedHash,
                PaymentDescription = GetValueOrNull(callbackData, "PaymentDescription"),
                ItemId = GetValueOrNull(callbackData, "ItemId")
            };

            // TODO: Save payment result to database here
            // Example:
            // await _paymentRepository.SavePaymentResultAsync(callback, cancellationToken);

            return await Task.FromResult(callback);
        }

        /// <summary>
        /// Gets value from dictionary, throws if not found
        /// </summary>
        private static string GetValue(Dictionary<string, string> data, string key)
        {
            if (data.TryGetValue(key, out var value))
                return value;

            throw new KeyNotFoundException($"Required key '{key}' not found in callback data");
        }

        /// <summary>
        /// Gets value from dictionary, returns null if not found
        /// </summary>
        private static string? GetValueOrNull(Dictionary<string, string> data, string key)
        {
            return data.TryGetValue(key, out var value) ? value : null;
        }
        /// <summary>
        /// Prepares payment parameters and generates secure hash for Direct Post Payment
        /// Frontend will add card data and post directly to Payone
        /// </summary>
        public async Task<PreparePaymentResponseDto> PreparePaymentAsync(
            PreparePaymentRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // Validate input
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(request.Amount));

            // Generate unique transaction ID (timestamp-based)
            var transactionId = GenerateTransactionId();

            // Convert amount from SAR to fils (smallest currency unit)
            // 1 SAR = 100 fils, so 50.00 SAR = 5000 fils
            var amountInFils = ConvertToFils(request.Amount);

            // Prepare all parameters for Payone Direct Post
            var parameters = new Dictionary<string, string>
            {
                { "MerchantId", _configurationProvider.MerchantId },
                { "TransactionId", transactionId },
                { "Amount", amountInFils },
                { "CurrencyIsoCode", _configurationProvider.CurrencyIsoCode },
                { "MessageId", request.MessageId.ToString() },
                { "Quantity", _configurationProvider.Quantity.ToString() },
                { "Channel", _configurationProvider.Channel.ToString() },
                { "PaymentMethod", request.PaymentMethod.ToString() },
                { "Language", _configurationProvider.Language },
                { "ThemeId", _configurationProvider.ThemeId },
                { "Version", _configurationProvider.Version }
            };

            // Add optional parameters if provided
            if (!string.IsNullOrWhiteSpace(request.PaymentDescription))
            {
                parameters.Add("PaymentDescription", request.PaymentDescription);
            }

            if (!string.IsNullOrWhiteSpace(request.ItemId))
            {
                parameters.Add("ItemId", request.ItemId);
            }

            // Generate SecureHash (card fields will be excluded automatically by SecureHashGenerator)
            var secureHash = _secureHashGenerator.Generate(parameters, _configurationProvider.AuthenticationToken);

            // Return response with all data needed by Angular to post to Payone
            return await Task.FromResult(new PreparePaymentResponseDto
            {
                TransactionId = transactionId,
                MerchantId = _configurationProvider.MerchantId,
                Amount = amountInFils,
                CurrencyIsoCode = _configurationProvider.CurrencyIsoCode,
                MessageId = request.MessageId.ToString(),
                Quantity = _configurationProvider.Quantity.ToString(),
                Channel = _configurationProvider.Channel.ToString(),
                PaymentMethod = request.PaymentMethod.ToString(),
                Language = _configurationProvider.Language,
                ThemeId = _configurationProvider.ThemeId,
                Version = _configurationProvider.Version,
                SecureHash = secureHash,
                PaymentDescription = request.PaymentDescription,
                ItemId = request.ItemId,
                PayoneUrl = _configurationProvider.ApiUrl // Payone Direct Post URL where Angular should POST
            });
        }

        /// <summary>
        /// Process payment request through SmartRoute Direct Post Payment
        /// </summary>
        public async Task<PaymentResponseDto> ProcessPaymentAsync(
            PaymentRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // Generate unique transaction ID (timestamp-based)
            var transactionId = GenerateTransactionId();

            // Map DTO to Domain Entity
            var paymentRequest = MapToPaymentRequest(request, transactionId);

            // Process payment through gateway
            var paymentResponse = await _smartRouteGateway.ProcessPaymentAsync(
                paymentRequest,
                cancellationToken);

            // Map Domain Entity to DTO
            return MapToPaymentResponseDto(paymentResponse);
        }

        /// <summary>
        /// Converts amount from major currency unit (SAR) to fils (smallest unit)
        /// Example: 50.00 SAR -> 5000 fils
        /// </summary>
        private static string ConvertToFils(decimal amount)
        {
            // Multiply by 100 to convert SAR to fils
            var fils = (int)(amount * 100);
            return fils.ToString();
        }

        /// <summary>
        /// Generates unique 20-character transaction ID
        /// Format: Unix timestamp (13 digits) + Random (7 digits)
        /// Example: 17298765432101234567
        /// </summary>
        private static string GenerateTransactionId()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var random = Random.Shared.Next(1000000, 9999999).ToString();
            return $"{timestamp}{random}";
        }

        /// <summary>
        /// Maps PaymentRequestDto to Domain PaymentRequest entity
        /// </summary>
        private static PaymentRequest MapToPaymentRequest(PaymentRequestDto dto, string transactionId)
        {
            return new PaymentRequest
            {
                TransactionId = transactionId,
                Amount = dto.Amount,
                MessageId = 1, // REQUIRED: 1=Payment, 2=PreAuth, 3=Verify
                PaymentMethod = 1, // 1 = Card Payment (Mada is treated as card payment)
                CardNumber = dto.CardNumber,
                ExpiryDateMonth = dto.ExpiryDateMonth,
                ExpiryDateYear = dto.ExpiryDateYear,
                SecurityCode = dto.SecurityCode,
                CardHolderName = dto.CardHolderName,
                PaymentDescription = dto.PaymentDescription ?? string.Empty,
                ItemId = dto.ItemId ?? string.Empty
            };
        }

        /// <summary>
        /// Maps Domain PaymentResponse entity to PaymentResponseDto
        /// </summary>
        private static PaymentResponseDto MapToPaymentResponseDto(PaymentResponse response)
        {
            return new PaymentResponseDto
            {
                IsSuccess = response.IsSuccess,
                TransactionId = response.TransactionId,
                MessageId = response.MessageId,
                StatusCode = response.StatusCode,
                StatusDescription = response.StatusDescription,
                ApprovalCode = response.ApprovalCode,
                GatewayName = response.GatewayName,
                GatewayStatusCode = response.GatewayStatusCode,
                GatewayStatusDescription = response.GatewayStatusDescription,
                MaskedCardNumber = response.CardNumber, // Already masked by SmartRoute
                CardExpiryDate = response.CardExpiryDate,
                CardHolderName = response.CardHolderName,
                Amount = response.Amount,
                CurrencyIsoCode = response.CurrencyIsoCode,
                Rrn = response.Rrn,
                Token = response.Token,
                IssuerName = response.IssuerName,
                ErrorMessage = response.ErrorMessage,
                ProcessedAt = response.ProcessedAt
            };
        }
    }
}