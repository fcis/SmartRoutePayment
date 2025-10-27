using SmartRoutePayment.Application.DTOs.Requests;
using SmartRoutePayment.Application.DTOs.Responses;
using SmartRoutePayment.Application.Interfaces;
using SmartRoutePayment.Domain.Entities;
using SmartRoutePayment.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ISmartRouteGateway _smartRouteGateway;

        public PaymentService(ISmartRouteGateway smartRouteGateway)
        {
            _smartRouteGateway = smartRouteGateway ?? throw new ArgumentNullException(nameof(smartRouteGateway));
        }

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

        private static PaymentRequest MapToPaymentRequest(PaymentRequestDto dto, string transactionId)
        {
            return new PaymentRequest
            {
                TransactionId = transactionId,
                Amount = dto.Amount,
                CardNumber = dto.CardNumber,
                ExpiryDateMonth = dto.ExpiryDateMonth,
                ExpiryDateYear = dto.ExpiryDateYear,
                SecurityCode = dto.SecurityCode,
                CardHolderName = dto.CardHolderName,
                PaymentMethod = 1, // 1 = Card Payment (Mada is treated as card)
                PaymentDescription = dto.PaymentDescription ?? string.Empty,
                ItemId = dto.ItemId ?? string.Empty
            };
        }

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
                MaskedCardNumber = response.CardNumber,
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

        private static string GenerateTransactionId()
        {
            // Generate unique 20-character transaction ID
            // Format: Timestamp (13 digits) + Random (7 digits)
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var random = Random.Shared.Next(1000000, 9999999).ToString();
            return $"{timestamp}{random}";
        }
    }
}
