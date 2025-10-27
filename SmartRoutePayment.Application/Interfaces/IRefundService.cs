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
    /// Service interface for Transaction Refund operations
    /// </summary>
    public interface IRefundService
    {
        /// <summary>
        /// Refunds a specific transaction (partial or full)
        /// Sends B2B API request to SmartRoute
        /// </summary>
        /// <param name="request">Refund request with original transaction ID and amount</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refund response with refund confirmation</returns>
        Task<RefundResponseDto> RefundTransactionAsync(
            RefundRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
