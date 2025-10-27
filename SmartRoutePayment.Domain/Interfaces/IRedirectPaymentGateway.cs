using SmartRoutePayment.Domain.Entities.RedirectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Domain.Interfaces
{
    /// <summary>
    /// Gateway interface for Redirect Payment operations
    /// Implements Redirection Communication Model
    /// </summary>
    public interface IRedirectPaymentGateway
    {
        /// <summary>
        /// Prepares redirect payment request with secure hash
        /// Returns the payment URL and form parameters for client-side redirect
        /// </summary>
        /// <param name="request">Redirect payment request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dictionary containing form parameters to submit to SmartRoute</returns>
        Task<Dictionary<string, string>> PrepareRedirectPaymentAsync(
            RedirectPaymentRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates and parses the redirect payment response from SmartRoute
        /// Called when SmartRoute redirects back to merchant's ResponseBackURL
        /// </summary>
        /// <param name="responseParameters">Form parameters received from SmartRoute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Parsed and validated redirect payment response</returns>
        Task<RedirectPaymentResponse> ValidateRedirectPaymentResponseAsync(
            Dictionary<string, string> responseParameters,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the SmartRoute payment URL for redirect
        /// </summary>
        /// <returns>Payment URL</returns>
        string GetPaymentUrl();
    }
}
