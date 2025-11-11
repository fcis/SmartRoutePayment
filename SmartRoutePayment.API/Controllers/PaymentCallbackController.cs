using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartRoutePayment.API.Models;
using SmartRoutePayment.Application.DTOs.Responses;
using SmartRoutePayment.Application.Interfaces;

namespace SmartRoutePayment.API.Controllers
{
    /// <summary>
    /// Handles payment callbacks from Payone
    /// </summary>
    [ApiController]
    [Route("api/payment/callback")]
    public class PaymentCallbackController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentCallbackController> _logger;

        public PaymentCallbackController(
            IPaymentService paymentService,
            ILogger<PaymentCallbackController> logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult HandleCallback([FromForm] IFormCollection formData)
        {
            try
            {
                _logger.LogInformation("Payment callback received from PayOne");

                // Convert all form data to query parameters
                var queryParams = new List<string>();

                foreach (var item in formData)
                {
                    var key = Uri.EscapeDataString(item.Key);
                    var value = Uri.EscapeDataString(item.Value.ToString());
                    queryParams.Add($"{key}={value}");

                    // Log each parameter for debugging
                    _logger.LogInformation("PayOne Parameter: {Key} = {Value}", item.Key, item.Value);
                }

                var queryString = string.Join("&", queryParams);

                _logger.LogInformation("Redirecting to Angular with query string: {QueryString}", queryString);

                // Redirect to Angular payment result page
                return Redirect($"http://localhost:4200/payment/result?{queryString}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment callback");
                return Redirect("/payment/result?error=Payment+processing+error");
            }
        }
    }
}
