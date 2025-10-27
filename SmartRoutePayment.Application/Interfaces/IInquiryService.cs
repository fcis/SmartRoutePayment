using SmartRoutePayment.Application.DTOs.Requests.RedirectModel;
using SmartRoutePayment.Application.DTOs.Responses.RedirectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.Interfaces
{
    /// <summary>
    /// Service interface for Transaction Inquiry operations
    /// </summary>
    public interface IInquiryService
    {
        /// <summary>
        /// Inquires about the status of a specific transaction
        /// Sends B2B API request to SmartRoute
        /// </summary>
        /// <param name="request">Inquiry request with original transaction ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Inquiry response with transaction status and details</returns>
        Task<InquiryResponseDto> InquireTransactionAsync(
            InquiryRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
