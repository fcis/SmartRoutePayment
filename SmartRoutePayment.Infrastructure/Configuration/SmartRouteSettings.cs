using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Infrastructure.Configuration
{
    public class SmartRouteSettings
    {
        public const string SectionName = "SmartRoute";

        public string ApiUrl { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public string AuthenticationToken { get; set; } = string.Empty;
        public string ThemeId { get; set; } = string.Empty;
        public string CurrencyIsoCode { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
        public string Language { get; set; } = "en";
        public int Channel { get; set; } = 0;
        public int Quantity { get; set; } = 1;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
