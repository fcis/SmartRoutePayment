using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Domain.Entities
{
    public class PaymentResponse
    {
        public string MessageId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
        public string GatewayStatusCode { get; set; } = string.Empty;
        public string GatewayName { get; set; } = string.Empty;
        public string GatewayStatusDescription { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string ApprovalCode { get; set; } = string.Empty;
        public string CardExpiryDate { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public string CurrencyIsoCode { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty; // Masked
        public string MerchantId { get; set; } = string.Empty;
        public string Rrn { get; set; } = string.Empty; // Retrieval Reference Number
        public string SecureHash { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string IssuerName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;

        // Additional helper properties
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
