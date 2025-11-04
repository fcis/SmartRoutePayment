using Microsoft.Extensions.Options;
using SmartRoutePayment.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Infrastructure.Configuration
{
    /// <summary>
    /// Implementation of payment configuration provider
    /// Reads from SmartRouteSettings and exposes to application layer
    /// </summary>
    public class PaymentConfigurationProvider : IPaymentConfigurationProvider
    {
        private readonly SmartRouteSettings _settings;

        public PaymentConfigurationProvider(IOptions<SmartRouteSettings> settings)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public string ApiUrl => _settings.ApiUrl;
        public string MerchantId => _settings.MerchantId;
        public string AuthenticationToken => _settings.AuthenticationToken;
        public string ThemeId => _settings.ThemeId;
        public string CurrencyIsoCode => _settings.CurrencyIsoCode;
        public string Version => _settings.Version;
        public string Language => _settings.Language;
        public int Channel => _settings.Channel;
        public int Quantity => _settings.Quantity;
    }
}
