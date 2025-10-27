using SmartRoutePayment.Application.DTOs.Requests.RedirectModel;
using SmartRoutePayment.Application.DTOs.Responses.RedirectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.Interfaces
{
    /// <summary>
    /// Service interface for Redirect Payment operations
    /// </summary>
    public interface IRedirectPaymentService
    {
        /// <summary>
        /// Initiates a redirect payment
        /// Returns payment URL and form parameters for client-side redirect
        /// </summary>
        /// <param name="request">Payment request details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Payment initiation response with redirect information</returns>
        Task<RedirectPaymentInitiationResponseDto> InitiatePaymentAsync(
            RedirectPaymentRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes callback response from SmartRoute
        /// Validates and parses the response when SmartRoute redirects back
        /// </summary>
        /// <param name="callbackParameters">Form parameters from SmartRoute callback</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Parsed and validated payment response</returns>
        Task<RedirectPaymentCallbackResponseDto> ProcessCallbackAsync(
            Dictionary<string, string> callbackParameters,
            CancellationToken cancellationToken = default);
    }
}
