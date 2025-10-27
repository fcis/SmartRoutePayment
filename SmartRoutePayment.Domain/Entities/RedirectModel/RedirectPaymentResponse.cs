using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Domain.Entities.RedirectModel
{
    /// <summary>
    /// Response entity for Redirect Payment
    /// Received from SmartRoute after payment processing
    /// </summary>
    public class RedirectPaymentResponse
    {
        /// <summary>
        /// Response status code from SmartRoute
        /// Example: "00000" for success
        /// </summary>
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// Message describing the response status
        /// UTF-8 encoded
        /// </summary>
        public string StatusDescription { get; set; } = string.Empty;

        /// <summary>
        /// Purchase amount
        /// </summary>
        public string Amount { get; set; } = string.Empty;

        /// <summary>
        /// ISO formatted currency code (numeric)
        /// </summary>
        public string CurrencyIsoCode { get; set; } = string.Empty;

        /// <summary>
        /// Authorized amount (if MCP enabled)
        /// </summary>
        public string? AuthorizedAmount { get; set; }

        /// <summary>
        /// Authorized currency ISO code (if MCP enabled)
        /// </summary>
        public string? AuthorizedCurrencyIsoCode { get; set; }

        /// <summary>
        /// Unique Merchant ID
        /// </summary>
        public string MerchantId { get; set; } = string.Empty;

        /// <summary>
        /// Unique transaction identifier
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Message ID (should be "1" for redirect payment response)
        /// </summary>
        public string MessageId { get; set; } = string.Empty;

        /// <summary>
        /// Secure hash for response validation
        /// </summary>
        public string SecureHash { get; set; } = string.Empty;

        /// <summary>
        /// Card expiry date (MMYY format)
        /// Example: 1221 for December 2021
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
        /// Gateway response code
        /// </summary>
        public string? GatewayStatusCode { get; set; }

        /// <summary>
        /// Gateway response description
        /// UTF-8 encoded
        /// </summary>
        public string? GatewayStatusDescription { get; set; }

        /// <summary>
        /// Gateway name that processed the transaction
        /// </summary>
        public string? GatewayName { get; set; }

        /// <summary>
        /// Receipt Reference Number
        /// </summary>
        public string? Rrn { get; set; }

        /// <summary>
        /// Approval code from payment processor
        /// </summary>
        public string? ApprovalCode { get; set; }

        /// <summary>
        /// Generated token for card (if GenerateToken was "Yes")
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Payment method used
        /// Available if Version >= 2.0
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Issuer name
        /// Available if Version >= 3.1
        /// </summary>
        public string? IssuerName { get; set; }

        /// <summary>
        /// Indicates if payment was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message (if any)
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when response was processed
        /// </summary>
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
