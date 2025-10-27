using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.DTOs.Requests
{
    public class PaymentRequestDto
    {
        public decimal Amount { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string ExpiryDateMonth { get; set; } = string.Empty;
        public string ExpiryDateYear { get; set; } = string.Empty;
        public string SecurityCode { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public string? PaymentDescription { get; set; }
        public string? ItemId { get; set; }
    }
}
