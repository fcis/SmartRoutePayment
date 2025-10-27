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
    /// Service implementation for Transaction Refund operations
    /// Handles business logic and DTO-Entity mapping
    /// </summary>
    public class RefundService : IRefundService
    {
        private readonly IRefundGateway _refundGateway;

        public RefundService(IRefundGateway refundGateway)
        {
            _refundGateway = refundGateway ?? throw new ArgumentNullException(nameof(refundGateway));
        }

        /// <summary>
        /// Refunds a specific transaction (partial or full)
        /// </summary>
        public async Task<RefundResponseDto> RefundTransactionAsync(
            RefundRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // Generate unique refund transaction ID
            var refundTransactionId = GenerateTransactionId();

            // Map DTO to Domain Entity
            var refundRequest = MapToRefundRequest(request, refundTransactionId);

            // Execute refund
            var refundResponse = await _refundGateway.RefundTransactionAsync(
                refundRequest,
                cancellationToken);

            // Map Domain Entity to DTO
            return MapToRefundResponseDto(refundResponse);
        }

        /// <summary>
        /// Maps DTO to Domain Entity
        /// Gateway implementation will fill in configuration values
        /// </summary>
        private static RefundRequest MapToRefundRequest(RefundRequestDto dto, string refundTransactionId)
        {
            // Convert decimal amount to ISO format (no decimal point)
            // Example: 50.00 SAR -> "5000"
            var amountInSmallestUnit = ((int)(dto.RefundAmount * 100)).ToString();

            return new RefundRequest
            {
                MessageId = "4", // 4 = Transaction Refund
                TransactionId = refundTransactionId,
                SubPun = dto.SubPun,
                Amount = amountInSmallestUnit,
                OriginalTransactionId = dto.OriginalTransactionId
                // MerchantId, CurrencyIsoCode, and Version will be filled by the Gateway implementation from configuration
            };
        }

        /// <summary>
        /// Maps Domain Entity to DTO
        /// </summary>
        private static RefundResponseDto MapToRefundResponseDto(RefundResponse response)
        {
            return new RefundResponseDto
            {
                IsSuccess = response.IsSuccess,
                RefundTransactionId = response.TransactionId,
                OriginalTransactionId = response.OriginalTransactionId,
                StatusCode = response.StatusCode,
                StatusDescription = response.StatusDescription,
                Amount = response.Amount,
                CurrencyIsoCode = response.CurrencyIsoCode,
                SubPun = response.SubPun,
                Rrn = response.Rrn,
                ErrorMessage = response.ErrorMessage,
                ProcessedAt = response.ProcessedAt
            };
        }

        /// <summary>
        /// Generates unique transaction ID for refund
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
