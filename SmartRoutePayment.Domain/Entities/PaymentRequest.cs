using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Domain.Entities
{
    public class PaymentRequest
    {
        public string TransactionId { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyIsoCode { get; set; } = string.Empty;
        public int MessageId { get; set; }
        public int Quantity { get; set; }
        public int Channel { get; set; }
        public int PaymentMethod { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string ExpiryDateYear { get; set; } = string.Empty;
        public string ExpiryDateMonth { get; set; } = string.Empty;
        public string SecurityCode { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string ThemeId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string SecureHash { get; set; } = string.Empty;
        public string ResponseBackURL { get; set; } = string.Empty;
        public string PaymentDescription { get; set; } = string.Empty;
        public string ItemId { get; set; } = string.Empty;
    }
}
