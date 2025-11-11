using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.DTOs.Responses
{
    /// <summary>
    /// DTO for transaction inquiry response
    /// </summary>
    public class InquiryResponseDto
    {
        public string MessageStatus { get; set; } = string.Empty;
        public string ReversalStatus { get; set; } = string.Empty;
        public string GatewayStatusCode { get; set; } = string.Empty;
        public string GatewayStatusDescription { get; set; } = string.Empty;
        public string GatewayName { get; set; } = string.Empty;
        public string TransactionID { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string CurrencyISOCode { get; set; } = string.Empty;
        public string MessageID { get; set; } = string.Empty;
        public string MerchantID { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
        public string RRN { get; set; } = string.Empty;
        public string ApprovalCode { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string SecureHash { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string CardExpiryDate { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }
}
