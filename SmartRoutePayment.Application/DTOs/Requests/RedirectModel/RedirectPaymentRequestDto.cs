using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.DTOs.Requests.RedirectModel
{
    /// <summary>
    /// DTO for initiating redirect payment
    /// Client sends this to prepare redirect form
    /// </summary>
    public class RedirectPaymentRequestDto
    {
        /// <summary>
        /// Payment amount in decimal format
        /// Example: 100.50 for SAR 100.50
        /// Will be converted to ISO format (10050 for SAR 100.50)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Payment description (optional)
        /// Example: "Order #12345 - Electronics Purchase"
        /// </summary>
        public string? PaymentDescription { get; set; }

        /// <summary>
        /// Custom item ID (optional)
        /// Example: "ITEM-12345"
        /// </summary>
        public string? ItemId { get; set; }

        /// <summary>
        /// Language preference for payment page
        /// Values: "en" (English), "ar" (Arabic)
        /// Default: "en"
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// Generate token for card tokenization (optional)
        /// Values: "Yes" or "No"
        /// If "Yes", a token will be returned in the response for future use
        /// </summary>
        public string? GenerateToken { get; set; }

        /// <summary>
        /// Existing card token (optional)
        /// Use this if customer wants to pay with previously saved card
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Agreement ID for recurring payments (optional)
        /// Max 20 characters, alphanumeric
        /// Used for subscription-based payments
        /// </summary>
        public string? AgreementId { get; set; }

        /// <summary>
        /// Agreement type (optional, required if AgreementId is provided)
        /// Values: "Recurring", "Unscheduled", "Other"
        /// </summary>
        public string? AgreementType { get; set; }

        /// <summary>
        /// Preferred payment method (optional)
        /// Values: APPLEPAY, URPAY, STCPAY, SADAD_BILLING, EMKAN, TABBY, 
        /// GOOGLE_PAY, VISA, MASTERCARD, AMEX, MADA
        /// If specified, payment page will pre-select this method
        /// </summary>
        public string? PreferredPaymentMethod { get; set; }

        /// <summary>
        /// Custom response callback URL (optional)
        /// If not provided, will use the one configured in appsettings
        /// </summary>
        public string? CustomResponseBackUrl { get; set; }
    }
}
