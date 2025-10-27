using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Domain.Entities.RedirectModel
{
    /// <summary>
    /// Request entity for Transaction Inquiry (B2B API Communication Model)
    /// Used to inquire about the status of a specific transaction
    /// </summary>
    public class InquiryRequest
    {
        /// <summary>
        /// MessageID = 2 for Transaction Inquiry
        /// </summary>
        public string MessageId { get; set; } = "2";

        /// <summary>
        /// Unique Merchant ID at SmartRoute
        /// </summary>
        public string MerchantId { get; set; } = string.Empty;

        /// <summary>
        /// Transaction ID of the original transaction to inquire about
        /// </summary>
        public string OriginalTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Secure hash using SHA-256
        /// </summary>
        public string SecureHash { get; set; } = string.Empty;

        /// <summary>
        /// Command version (default: 1.0)
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Include refund IDs in response
        /// Values: "Yes" or "No"
        /// If "Yes", response will include RefundStatus and RefundIds
        /// </summary>
        public string? IncludeRefundIds { get; set; }
    }
}
