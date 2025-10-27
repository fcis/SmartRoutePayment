using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.DTOs.Responses.RedirectModel
{
    /// <summary>
    /// DTO for transaction inquiry response
    /// Contains current status and details of the inquired transaction
    /// </summary>
    public class InquiryResponseDto
    {
        /// <summary>
        /// Indicates if inquiry request was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Transaction ID
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Original transaction status code
        /// "00000" = Success
        /// </summary>
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// Message status from inquiry operation
        /// </summary>
        public string MessageStatus { get; set; } = string.Empty;

        /// <summary>
        /// Transaction amount
        /// </summary>
        public string Amount { get; set; } = string.Empty;

        /// <summary>
        /// Currency ISO code
        /// </summary>
        public string CurrencyIsoCode { get; set; } = string.Empty;

        /// <summary>
        /// Reversal status of the transaction
        /// Values: "Not Reversed", "Reversed", "Partially Reversed"
        /// </summary>
        public string ReversalStatus { get; set; } = string.Empty;

        /// <summary>
        /// Payment method used
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Authorized amount (if MCP enabled)
        /// </summary>
        public string? AuthorizedAmount { get; set; }

        /// <summary>
        /// Authorized currency ISO code (if MCP enabled)
        /// </summary>
        public string? AuthorizedCurrencyIsoCode { get; set; }

        /// <summary>
        /// Gateway status code
        /// </summary>
        public string? GatewayStatusCode { get; set; }

        /// <summary>
        /// Gateway status description
        /// </summary>
        public string? GatewayStatusDescription { get; set; }

        /// <summary>
        /// Gateway name
        /// </summary>
        public string? GatewayName { get; set; }

        /// <summary>
        /// Receipt Reference Number
        /// </summary>
        public string? Rrn { get; set; }

        /// <summary>
        /// Approval code
        /// </summary>
        public string? ApprovalCode { get; set; }

        /// <summary>
        /// Card expiry date
        /// </summary>
        public string? CardExpiryDate { get; set; }

        /// <summary>
        /// Card holder name
        /// </summary>
        public string? CardHolderName { get; set; }

        /// <summary>
        /// Masked card number
        /// </summary>
        public string? CardNumber { get; set; }

        /// <summary>
        /// Refund status (if IncludeRefundIds was "Yes")
        /// Values: "Not Refunded", "Refunded", "Partially Refunded"
        /// </summary>
        public string? RefundStatus { get; set; }

        /// <summary>
        /// Comma-separated list of refund transaction IDs (if IncludeRefundIds was "Yes")
        /// </summary>
        public string? RefundIds { get; set; }

        /// <summary>
        /// Issuer name (if Version >= 3.1)
        /// </summary>
        public string? IssuerName { get; set; }

        /// <summary>
        /// Error message (if any)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Timestamp when inquiry was processed
        /// </summary>
        public DateTime ProcessedAt { get; set; }
    }
}
