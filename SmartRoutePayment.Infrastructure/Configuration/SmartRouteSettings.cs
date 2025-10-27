using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration settings for SmartRoute Payment Gateway
    /// Supports both Direct Post Model and Redirectional Model
    /// </summary>
    public class SmartRouteSettings
    {
        public const string SectionName = "SmartRoute";

        // ============================================
        // Direct Post Model URLs (Existing)
        // ============================================

        /// <summary>
        /// Direct Post API URL (for Direct Post Payment Model)
        /// Example: https://smartroute-url.com/SRPayMsgHandler
        /// </summary>
        public string ApiUrl { get; set; } = string.Empty;

        // ============================================
        // Redirectional Model URLs (New)
        // ============================================

        /// <summary>
        /// Payment Redirect URL (for Redirect Payment - Redirection Model)
        /// Customer will be redirected to this URL for payment
        /// Example: https://smartroute-url.com/SmartRoutePaymentWEB/SRPayMsgHandler
        /// </summary>
        public string PaymentUrl { get; set; } = string.Empty;

        /// <summary>
        /// Inquiry API URL (for Transaction Inquiry - B2B Communication Model)
        /// Server-to-server API call to check transaction status
        /// Example: https://smartroute-url.com/SRPayMsgHandler
        /// </summary>
        public string InquiryUrl { get; set; } = string.Empty;

        /// <summary>
        /// Refund API URL (for Transaction Refund - B2B Communication Model)
        /// Server-to-server API call to refund transactions
        /// Example: https://smartroute-url.com/SRPayMsgHandler
        /// </summary>
        public string RefundUrl { get; set; } = string.Empty;

        // ============================================
        // Merchant Credentials
        // ============================================

        /// <summary>
        /// Unique Merchant ID provided by SmartRoute
        /// </summary>
        public string MerchantId { get; set; } = string.Empty;

        /// <summary>
        /// Authentication Token for secure hash generation
        /// CRITICAL: Store securely (use User Secrets, Azure Key Vault, etc.)
        /// </summary>
        public string AuthenticationToken { get; set; } = string.Empty;

        // ============================================
        // Configuration Parameters
        // ============================================

        /// <summary>
        /// Theme ID for payment page customization
        /// </summary>
        public string ThemeId { get; set; } = string.Empty;

        /// <summary>
        /// Currency ISO Code (numeric format)
        /// Example: 682 for SAR (Saudi Riyal), 840 for USD
        /// </summary>
        public string CurrencyIsoCode { get; set; } = string.Empty;

        /// <summary>
        /// API Version
        /// 1.0 = Basic response
        /// 2.0+ = Includes PaymentMethod in response
        /// 3.1+ = Includes IssuerName in response
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Interface language (en = English, ar = Arabic)
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// Channel type
        /// 0 = Web, 1 = Mobile, 2 = Call Center
        /// </summary>
        public int Channel { get; set; } = 0;

        /// <summary>
        /// Quantity of items (default: 1)
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// HTTP request timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        // ============================================
        // Callback URLs (Redirectional Model)
        // ============================================

        /// <summary>
        /// Merchant's callback URL where SmartRoute sends payment response
        /// Customer is redirected here after payment completion
        /// Example: https://yoursite.com/api/payment/callback
        /// </summary>
        public string ResponseBackUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL for failed payment notifications (optional)
        /// Example: https://yoursite.com/api/payment/failed
        /// </summary>
        public string FailedPaymentReplyUrl { get; set; } = string.Empty;
    }
}
