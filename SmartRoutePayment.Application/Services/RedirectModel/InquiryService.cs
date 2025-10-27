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
    /// Service implementation for Transaction Inquiry operations
    /// Handles business logic and DTO-Entity mapping
    /// </summary>
    public class InquiryService : IInquiryService
    {
        private readonly IInquiryGateway _inquiryGateway;

        public InquiryService(IInquiryGateway inquiryGateway)
        {
            _inquiryGateway = inquiryGateway ?? throw new ArgumentNullException(nameof(inquiryGateway));
        }

        /// <summary>
        /// Inquires about the status of a specific transaction
        /// </summary>
        public async Task<InquiryResponseDto> InquireTransactionAsync(
            InquiryRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // Map DTO to Domain Entity
            var inquiryRequest = MapToInquiryRequest(request);

            // Execute inquiry
            var inquiryResponse = await _inquiryGateway.InquireTransactionAsync(
                inquiryRequest,
                cancellationToken);

            // Map Domain Entity to DTO
            return MapToInquiryResponseDto(inquiryResponse);
        }

        /// <summary>
        /// Maps DTO to Domain Entity
        /// Gateway implementation will fill in configuration values
        /// </summary>
        private static InquiryRequest MapToInquiryRequest(InquiryRequestDto dto)
        {
            return new InquiryRequest
            {
                MessageId = "2", // 2 = Transaction Inquiry
                OriginalTransactionId = dto.OriginalTransactionId,
                IncludeRefundIds = dto.IncludeRefundIds
                // MerchantId and Version will be filled by the Gateway implementation from configuration
            };
        }

        /// <summary>
        /// Maps Domain Entity to DTO
        /// </summary>
        private static InquiryResponseDto MapToInquiryResponseDto(InquiryResponse response)
        {
            return new InquiryResponseDto
            {
                IsSuccess = response.IsSuccess,
                TransactionId = response.TransactionId,
                StatusCode = response.StatusCode,
                MessageStatus = response.MessageStatus,
                Amount = response.Amount,
                CurrencyIsoCode = response.CurrencyIsoCode,
                ReversalStatus = response.ReversalStatus,
                PaymentMethod = response.PaymentMethod,
                AuthorizedAmount = response.AuthorizedAmount,
                AuthorizedCurrencyIsoCode = response.AuthorizedCurrencyIsoCode,
                GatewayStatusCode = response.GatewayStatusCode,
                GatewayStatusDescription = response.GatewayStatusDescription,
                GatewayName = response.GatewayName,
                Rrn = response.Rrn,
                ApprovalCode = response.ApprovalCode,
                CardExpiryDate = response.CardExpiryDate,
                CardHolderName = response.CardHolderName,
                CardNumber = response.CardNumber,
                RefundStatus = response.RefundStatus,
                RefundIds = response.RefundIds,
                IssuerName = response.IssuerName,
                ErrorMessage = response.ErrorMessage,
                ProcessedAt = response.ProcessedAt
            };
        }
    }
}
