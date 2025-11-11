using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.DTOs.Requests
{
    /// <summary>
    /// DTO for transaction inquiry request
    /// </summary>
    public class InquiryRequestDto
    {
        /// <summary>
        /// Original Transaction ID to inquire about
        /// </summary>
        public string OriginalTransactionID { get; set; } = string.Empty;

        /// <summary>
        /// Message ID (optional, defaults to 2)
        /// </summary>
        public int? MessageID { get; set; }

        /// <summary>
        /// Version (optional, uses config default if not provided)
        /// </summary>
        public string? Version { get; set; }
    }
}
