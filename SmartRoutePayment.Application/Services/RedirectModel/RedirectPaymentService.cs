using Microsoft.Extensions.Options;
using SmartRoutePayment.Application.DTOs.Requests.RedirectModel;
using SmartRoutePayment.Application.DTOs.Responses.RedirectModel;
using SmartRoutePayment.Application.Interfaces;
using SmartRoutePayment.Domain.Entities.RedirectModel;
using SmartRoutePayment.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.Services.RedirectModel
{
    /// <summary>
    /// Service implementation for Redirect Payment operations
    /// Handles business logic and DTO-Entity mapping
    /// </summary>
    public class RedirectPaymentService : IRedirectPaymentService
    {
        private readonly IRedirectPaymentGateway _redirectPaymentGateway;

        public RedirectPaymentService(IRedirectPaymentGateway redirectPaymentGateway)
        {
            _redirectPaymentGateway = redirectPaymentGateway ?? throw new ArgumentNullException(nameof(redirectPaymentGateway));
        }

        /// <summary>
        /// Initiates a redirect payment
        /// </summary>
        public async Task<RedirectPaymentInitiationResponseDto> InitiatePaymentAsync(
            RedirectPaymentRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // Generate unique transaction ID
            var transactionId = GenerateTransactionId();

            // Map DTO to Domain Entity
            var paymentRequest = MapToPaymentRequest(request, transactionId);

            // Prepare redirect payment (get form parameters with secure hash)
            var formParameters = await _redirectPaymentGateway.PrepareRedirectPaymentAsync(
                paymentRequest,
                cancellationToken);

            // Get payment URL
            var paymentUrl = _redirectPaymentGateway.GetPaymentUrl();

            // Return initiation response
            return new RedirectPaymentInitiationResponseDto
            {
                PaymentUrl = paymentUrl,
                FormParameters = formParameters,
                TransactionId = transactionId
            };
        }

        /// <summary>
        /// Processes callback response from SmartRoute
        /// </summary>
        public async Task<RedirectPaymentCallbackResponseDto> ProcessCallbackAsync(
            Dictionary<string, string> callbackParameters,
            CancellationToken cancellationToken = default)
        {
            // Validate and parse response
            var paymentResponse = await _redirectPaymentGateway.ValidateRedirectPaymentResponseAsync(
                callbackParameters,
                cancellationToken);

            // Map Domain Entity to DTO
            return MapToCallbackResponseDto(paymentResponse);
        }

        /// <summary>
        /// Maps DTO to Domain Entity
        /// Gateway implementation will fill in configuration values
        /// </summary>
        private static RedirectPaymentRequest MapToPaymentRequest(RedirectPaymentRequestDto dto, string transactionId)
        {
            // Convert decimal amount to ISO format (no decimal point)
            // Example: 100.50 SAR -> "10050"
            var amountInSmallestUnit = ((int)(dto.Amount * 100)).ToString();

            return new RedirectPaymentRequest
            {
                MessageId = "1", // 1 = Redirect Payment
                TransactionId = transactionId,
                Amount = amountInSmallestUnit,
                Language = dto.Language,
                PaymentDescription = dto.PaymentDescription,
                ItemId = dto.ItemId,
                ResponseBackUrl = dto.CustomResponseBackUrl,
                GenerateToken = dto.GenerateToken,
                Token = dto.Token,
                AgreementId = dto.AgreementId,
                AgreementType = dto.AgreementType,
                PreferredPaymentMethod = dto.PreferredPaymentMethod
                // MerchantId, CurrencyIsoCode, Version, Channel, Quantity, ThemeId, FailedPaymentReplyUrl
                // will be filled by the Gateway implementation from configuration
            };
        }

        /// <summary>
        /// Maps Domain Entity to DTO
        /// </summary>
        private static RedirectPaymentCallbackResponseDto MapToCallbackResponseDto(RedirectPaymentResponse response)
        {
            return new RedirectPaymentCallbackResponseDto
            {
                IsSuccess = response.IsSuccess,
                TransactionId = response.TransactionId,
                StatusCode = response.StatusCode,
                StatusDescription = response.StatusDescription,
                Amount = response.Amount,
                CurrencyIsoCode = response.CurrencyIsoCode,
                MaskedCardNumber = response.CardNumber,
                CardExpiryDate = response.CardExpiryDate,
                CardHolderName = response.CardHolderName,
                ApprovalCode = response.ApprovalCode,
                Rrn = response.Rrn,
                GatewayName = response.GatewayName,
                GatewayStatusCode = response.GatewayStatusCode,
                GatewayStatusDescription = response.GatewayStatusDescription,
                Token = response.Token,
                PaymentMethod = response.PaymentMethod,
                IssuerName = response.IssuerName,
                ErrorMessage = response.ErrorMessage,
                ProcessedAt = response.ProcessedAt
            };
        }

        /// <summary>
        /// Generates unique transaction ID
        /// Format: Timestamp (13 digits) + Random (7 digits) = 20 characters
        /// </summary>
        private static string GenerateTransactionId()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var random = Random.Shared.Next(1000000, 9999999).ToString();
            return $"{timestamp}{random}";
        }
    }
}
