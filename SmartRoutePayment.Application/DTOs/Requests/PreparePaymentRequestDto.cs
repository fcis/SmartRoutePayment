using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.DTOs.Requests
{
    /// <summary>
    /// DTO for preparing payment on backend (before Angular posts to PayOne)
    /// Contains only non-sensitive data - NO CARD INFORMATION
    /// </summary>
    public class PreparePaymentRequestDto
    {
        /// <summary>
        /// Payment amount in major currency unit (e.g., 50.00 SAR)
        /// Backend will convert to fils (5000)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Optional payment description
        /// </summary>
        public string? PaymentDescription { get; set; }

        /// <summary>
        /// Optional item ID
        /// </summary>
        public string? ItemId { get; set; }

        /// <summary>
        /// Message ID: 1=Payment, 2=PreAuth, 3=Verify
        /// Default: 1 (Payment)
        /// </summary>
        public int MessageId { get; set; } = 1;

        /// <summary>
        /// Payment Method: 1=Card (including Mada)
        /// Default: 1
        /// </summary>
        public int PaymentMethod { get; set; } = 1;
    }
}
