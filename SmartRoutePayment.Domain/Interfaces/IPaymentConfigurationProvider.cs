using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Domain.Interfaces
{
    /// <summary>
    /// Provides payment gateway configuration
    /// Abstracts infrastructure configuration from application layer
    /// </summary>
    public interface IPaymentConfigurationProvider
    {
        /// <summary>
        /// Payment gateway API URL (Direct Post endpoint)
        /// </summary>
        string ApiUrl { get; }

        /// <summary>
        /// Merchant ID
        /// </summary>
        string MerchantId { get; }

        /// <summary>
        /// Authentication token for secure hash generation
        /// </summary>
        string AuthenticationToken { get; }

        /// <summary>
        /// Theme ID for payment page customization
        /// </summary>
        string ThemeId { get; }

        /// <summary>
        /// Currency ISO Code (e.g., "682" for SAR)
        /// </summary>
        string CurrencyIsoCode { get; }

        /// <summary>
        /// API Version
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Interface language (e.g., "en", "ar")
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Channel type (0 = Web, 1 = Mobile, 2 = Call Center)
        /// </summary>
        int Channel { get; }

        /// <summary>
        /// Quantity of items (default: 1)
        /// </summary>
        int Quantity { get; }

        /// <summary>
        /// Merchant's callback URL where PayOne sends payment response
        /// </summary>
        string ResponseBackUrl { get; }
    }
}
