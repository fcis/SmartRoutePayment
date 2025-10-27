using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.DTOs.Requests.RedirectModel
{
    /// <summary>
    /// DTO for transaction refund request
    /// Used to refund a previous transaction (partial or full)
    /// </summary>
    public class RefundRequestDto
    {
        /// <summary>
        /// Transaction ID of the original transaction to refund
        /// This is the TransactionID from the original payment
        /// </summary>
        public string OriginalTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Refund amount in decimal format
        /// For full refund: use the full original amount
        /// For partial refund: use amount less than original
        /// Example: 50.00 to refund SAR 50.00
        /// </summary>
        public decimal RefundAmount { get; set; }

        /// <summary>
        /// Sub-PUN/sub-Transaction to refund (optional)
        /// Used for split payments or installments
        /// </summary>
        public string? SubPun { get; set; }
    }
}
