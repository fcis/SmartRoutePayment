using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Domain.Entities
{
    /// <summary>
    /// Request entity for transaction inquiry operation
    /// Used for B2B server-to-server communication with PayOne
    /// </summary>
    public class InquiryRequest
    {
        /// <summary>
        /// Original Transaction ID to inquire about
        /// This is the transaction ID from the original payment/pre-auth operation
        /// </summary>
        public string OriginalTransactionID { get; set; } = string.Empty;

        /// <summary>
        /// Message ID (optional override, default is 2 for inquiry)
        /// </summary>
        public int? MessageID { get; set; }

        /// <summary>
        /// Version (optional override, uses config default if not provided)
        /// </summary>
        public string? Version { get; set; }
    }
}
