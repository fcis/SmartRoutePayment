using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.DTOs.Responses.RedirectModel
{
    /// <summary>
    /// DTO for transaction refund response
    /// Contains result of the refund operation
    /// </summary>
    public class RefundResponseDto
    {
        /// <summary>
        /// Indicates if refund was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Refund transaction ID (generated for this refund)
        /// Different from the original transaction ID
        /// </summary>
        public string RefundTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Original transaction ID that was refunded
        /// </summary>
        public string OriginalTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Status code from SmartRoute
        /// "00000" = Success
        /// </summary>
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// Status description
        /// </summary>
        public string StatusDescription { get; set; } = string.Empty;

        /// <summary>
        /// Refund amount
        /// </summary>
        public string Amount { get; set; } = string.Empty;

        /// <summary>
        /// Currency ISO code
        /// </summary>
        public string CurrencyIsoCode { get; set; } = string.Empty;

        /// <summary>
        /// Sub-PUN that was refunded (if applicable)
        /// </summary>
        public string? SubPun { get; set; }

        /// <summary>
        /// Receipt Reference Number for the refund
        /// </summary>
        public string? Rrn { get; set; }

        /// <summary>
        /// Error message (if any)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Timestamp when refund was processed
        /// </summary>
        public DateTime ProcessedAt { get; set; }
    }
}
