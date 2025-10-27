using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.DTOs.Responses
{
    public class PaymentResponseDto
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
        public string? ApprovalCode { get; set; }
        public string? GatewayName { get; set; }
        public string? GatewayStatusCode { get; set; }
        public string? GatewayStatusDescription { get; set; }
        public string? MaskedCardNumber { get; set; }
        public string? CardExpiryDate { get; set; }
        public string? CardHolderName { get; set; }
        public string Amount { get; set; } = string.Empty;
        public string CurrencyIsoCode { get; set; } = string.Empty;
        public string? Rrn { get; set; }
        public string? Token { get; set; }
        public string? IssuerName { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }
}
