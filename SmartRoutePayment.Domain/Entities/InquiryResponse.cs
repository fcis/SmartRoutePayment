using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Domain.Entities
{
    /// <summary>
    /// Response entity for transaction inquiry operation
    /// Contains the current status and details of the inquired transaction
    /// </summary>
    public class InquiryResponse
    {
        /// <summary>
        /// Message status code (00000 = success)
        /// </summary>
        public string MessageStatus { get; set; } = string.Empty;

        /// <summary>
        /// Reversal status (1 = not reversed, 2 = reversed)
        /// </summary>
        public string ReversalStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gateway status code from the payment processor
        /// </summary>
        public string GatewayStatusCode { get; set; } = string.Empty;

        /// <summary>
        /// Gateway status description
        /// </summary>
        public string GatewayStatusDescription { get; set; } = string.Empty;

        /// <summary>
        /// Name of the gateway that processed the transaction
        /// </summary>
        public string GatewayName { get; set; } = string.Empty;

        /// <summary>
        /// Transaction ID
        /// </summary>
        public string TransactionID { get; set; } = string.Empty;

        /// <summary>
        /// Transaction amount in smallest currency unit (e.g., fils/halalas)
        /// </summary>
        public string Amount { get; set; } = string.Empty;

        /// <summary>
        /// Currency ISO code (numeric format, e.g., 400 for SAR)
        /// </summary>
        public string CurrencyISOCode { get; set; } = string.Empty;

        /// <summary>
        /// Message ID from response
        /// </summary>
        public string MessageID { get; set; } = string.Empty;

        /// <summary>
        /// Merchant ID from response
        /// </summary>
        public string MerchantID { get; set; } = string.Empty;

        /// <summary>
        /// Status code (00000 = success)
        /// </summary>
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// Status description message
        /// </summary>
        public string StatusDescription { get; set; } = string.Empty;

        /// <summary>
        /// Retrieval Reference Number
        /// </summary>
        public string RRN { get; set; } = string.Empty;

        /// <summary>
        /// Approval code from the bank/gateway
        /// </summary>
        public string ApprovalCode { get; set; } = string.Empty;

        /// <summary>
        /// Payment method (e.g., "1:Visa")
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Secure hash for response validation
        /// </summary>
        public string SecureHash { get; set; } = string.Empty;

        /// <summary>
        /// Masked card number
        /// </summary>
        public string CardNumber { get; set; } = string.Empty;

        /// <summary>
        /// Card expiry date (YYMM format)
        /// </summary>
        public string CardExpiryDate { get; set; } = string.Empty;

        /// <summary>
        /// Card holder name
        /// </summary>
        public string CardHolderName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the inquiry was successful (StatusCode == "00000")
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message if inquiry failed
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the inquiry was processed
        /// </summary>
        public DateTime ProcessedAt { get; set; }
    }
}
