using SmartRoutePayment.Domain.Entities.RedirectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Domain.Interfaces
{
    /// <summary>
    /// Gateway interface for Transaction Refund operations
    /// Implements B2B API Communication Model
    /// </summary>
    public interface IRefundGateway
    {
        /// <summary>
        /// Refunds a specific transaction partially or fully
        /// Sends a B2B API request to SmartRoute refund endpoint
        /// </summary>
        /// <param name="request">Refund request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refund response with refund status</returns>
        Task<RefundResponse> RefundTransactionAsync(
            RefundRequest request,
            CancellationToken cancellationToken = default);
    }
}
