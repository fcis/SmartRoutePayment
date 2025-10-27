using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.DTOs.Requests.RedirectModel
{
    /// <summary>
    /// DTO for transaction inquiry request
    /// Used to check status of a previous transaction
    /// </summary>
    public class InquiryRequestDto
    {
        /// <summary>
        /// Transaction ID of the original transaction to inquire about
        /// This is the TransactionID that was used in the payment request
        /// </summary>
        public string OriginalTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Include refund information in response (optional)
        /// Values: "Yes" or "No"
        /// If "Yes", response will include RefundStatus and RefundIds
        /// </summary>
        public string? IncludeRefundIds { get; set; }
    }
}

