using SmartRoutePayment.Domain.Entities.RedirectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Domain.Interfaces
{
    /// <summary>
    /// Gateway interface for Transaction Inquiry operations
    /// Implements B2B API Communication Model
    /// </summary>
    public interface IInquiryGateway
    {
        /// <summary>
        /// Inquires about the status of a specific transaction
        /// Sends a B2B API request to SmartRoute inquiry endpoint
        /// </summary>
        /// <param name="request">Inquiry request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Inquiry response with transaction status</returns>
        Task<InquiryResponse> InquireTransactionAsync(
            InquiryRequest request,
            CancellationToken cancellationToken = default);
    }
}
