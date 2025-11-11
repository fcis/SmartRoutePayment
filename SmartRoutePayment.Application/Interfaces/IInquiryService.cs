using SmartRoutePayment.Application.DTOs.Requests;
using SmartRoutePayment.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.Interfaces
{
    /// <summary>
    /// Service for transaction inquiry operations
    /// Handles B2B communication with PayOne for transaction status checks
    /// </summary>
    public interface IInquiryService
    {
        /// <summary>
        /// Inquire about a transaction status
        /// </summary>
        /// <param name="request">Inquiry request containing the original transaction ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Inquiry response with transaction status and details</returns>
        Task<InquiryResponseDto> InquireTransactionAsync(
            InquiryRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
