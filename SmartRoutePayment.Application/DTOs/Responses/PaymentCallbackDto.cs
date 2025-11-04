using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.DTOs.Responses
{
    /// <summary>
    /// Payment callback response from Payone
    /// Received as form data after payment completion
    /// </summary>
    public class PaymentCallbackDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string CurrencyIsoCode { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
        public string? ApprovalCode { get; set; }
        public string? GatewayName { get; set; }
        public string? GatewayStatusCode { get; set; }
        public string? GatewayStatusDescription { get; set; }
        public string? CardNumber { get; set; } // Masked
        public string? CardExpiryDate { get; set; }
        public string? CardHolderName { get; set; }
        public string? Rrn { get; set; }
        public string? Token { get; set; }
        public string? IssuerName { get; set; }
        public string SecureHash { get; set; } = string.Empty;
        public string? PaymentDescription { get; set; }
        public string? ItemId { get; set; }

        /// <summary>
        /// Determines if payment was successful
        /// StatusCode "000" indicates success
        /// </summary>
        public bool IsSuccess => StatusCode == "000";
    }
}
