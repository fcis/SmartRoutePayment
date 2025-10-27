using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.DTOs.Responses.RedirectModel
{
    /// <summary>
    /// DTO for redirect payment initiation response
    /// Contains payment URL and form parameters for client-side redirect
    /// </summary>
    public class RedirectPaymentInitiationResponseDto
    {
        /// <summary>
        /// SmartRoute payment URL where user should be redirected
        /// </summary>
        public string PaymentUrl { get; set; } = string.Empty;

        /// <summary>
        /// Form parameters to submit to payment URL
        /// Client should create a form and auto-submit it
        /// </summary>
        public Dictionary<string, string> FormParameters { get; set; } = new();

        /// <summary>
        /// Transaction ID generated for this payment
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for redirect payment callback response
    /// Received when SmartRoute redirects back to merchant
    /// </summary>
    public class RedirectPaymentCallbackResponseDto
    {
        /// <summary>
        /// Indicates if payment was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Transaction ID
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

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
        /// Payment amount
        /// </summary>
        public string Amount { get; set; } = string.Empty;

        /// <summary>
        /// Currency ISO code
        /// </summary>
        public string CurrencyIsoCode { get; set; } = string.Empty;

        /// <summary>
        /// Masked card number (if available)
        /// Example: "************1234"
        /// </summary>
        public string? MaskedCardNumber { get; set; }

        /// <summary>
        /// Card expiry date (MMYY format)
        /// </summary>
        public string? CardExpiryDate { get; set; }

        /// <summary>
        /// Card holder name
        /// </summary>
        public string? CardHolderName { get; set; }

        /// <summary>
        /// Approval code from payment processor
        /// </summary>
        public string? ApprovalCode { get; set; }

        /// <summary>
        /// Receipt Reference Number
        /// </summary>
        public string? Rrn { get; set; }

        /// <summary>
        /// Gateway name that processed the payment
        /// </summary>
        public string? GatewayName { get; set; }

        /// <summary>
        /// Gateway status code
        /// </summary>
        public string? GatewayStatusCode { get; set; }

        /// <summary>
        /// Gateway status description
        /// </summary>
        public string? GatewayStatusDescription { get; set; }

        /// <summary>
        /// Card token (if tokenization was requested)
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
        /// Error message (if any)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Timestamp when payment was processed
        /// </summary>
        public DateTime ProcessedAt { get; set; }
    }
}
