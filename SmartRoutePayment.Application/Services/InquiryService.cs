using Microsoft.Extensions.Logging;
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
    public class InquiryService : IInquiryService
    {
        private readonly ISmartRouteGateway _smartRouteGateway;
        private readonly ILogger<InquiryService> _logger;

        public InquiryService(
            ISmartRouteGateway smartRouteGateway,
            ILogger<InquiryService> logger)
        {
            _smartRouteGateway = smartRouteGateway ?? throw new ArgumentNullException(nameof(smartRouteGateway));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Inquire about a transaction status
        /// </summary>
        public async Task<InquiryResponseDto> InquireTransactionAsync(
            InquiryRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting transaction inquiry for OriginalTransactionID: {TransactionId}",
                    request.OriginalTransactionID);

                // Validate request
                ValidateRequest(request);

                // Map DTO to Domain Entity
                var inquiryRequest = MapToInquiryRequest(request);

                // Call gateway
                var inquiryResponse = await _smartRouteGateway.InquireTransactionAsync(
                    inquiryRequest,
                    cancellationToken);

                // Map Domain Entity to DTO
                var responseDto = MapToInquiryResponseDto(inquiryResponse);

                _logger.LogInformation(
                    "Transaction inquiry completed for OriginalTransactionID: {TransactionId}, Success: {IsSuccess}, StatusCode: {StatusCode}",
                    request.OriginalTransactionID,
                    responseDto.IsSuccess,
                    responseDto.StatusCode);

                return responseDto;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during transaction inquiry");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during transaction inquiry for OriginalTransactionID: {TransactionId}",
                    request.OriginalTransactionID);
                throw;
            }
        }

        private void ValidateRequest(InquiryRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.OriginalTransactionID))
                throw new ArgumentException("OriginalTransactionID is required", nameof(request));
        }

        private InquiryRequest MapToInquiryRequest(InquiryRequestDto dto)
        {
            return new InquiryRequest
            {
                OriginalTransactionID = dto.OriginalTransactionID,
                MessageID = dto.MessageID,
                Version = dto.Version
            };
        }

        private InquiryResponseDto MapToInquiryResponseDto(InquiryResponse entity)
        {
            return new InquiryResponseDto
            {
                MessageStatus = entity.MessageStatus,
                ReversalStatus = entity.ReversalStatus,
                GatewayStatusCode = entity.GatewayStatusCode,
                GatewayStatusDescription = entity.GatewayStatusDescription,
                GatewayName = entity.GatewayName,
                TransactionID = entity.TransactionID,
                Amount = entity.Amount,
                CurrencyISOCode = entity.CurrencyISOCode,
                MessageID = entity.MessageID,
                MerchantID = entity.MerchantID,
                StatusCode = entity.StatusCode,
                StatusDescription = entity.StatusDescription,
                RRN = entity.RRN,
                ApprovalCode = entity.ApprovalCode,
                PaymentMethod = entity.PaymentMethod,
                SecureHash = entity.SecureHash,
                CardNumber = entity.CardNumber,
                CardExpiryDate = entity.CardExpiryDate,
                CardHolderName = entity.CardHolderName,
                IsSuccess = entity.IsSuccess,
                ErrorMessage = entity.ErrorMessage,
                ProcessedAt = entity.ProcessedAt
            };
        }
    }

}
