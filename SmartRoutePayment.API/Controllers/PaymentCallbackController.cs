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

        /// <summary>
        /// Receives payment callback from Payone (form POST)
        /// User is redirected here after payment completion
        /// </summary>
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> HandleCallback(
            [FromForm] IFormCollection formData,
            CancellationToken cancellationToken)
        {
            try
            {
                // Convert form data to dictionary
                var callbackData = formData.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToString()
                );

                _logger.LogInformation("Payment callback received. TransactionId: {TransactionId}",
                    callbackData.GetValueOrDefault("TransactionId", "Unknown"));

                // Validate and process callback
                var result = await _paymentService.HandlePaymentCallbackAsync(callbackData, cancellationToken);

                // Log result
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Payment successful. TransactionId: {TransactionId}, Amount: {Amount}",
                        result.TransactionId, result.Amount);
                }
                else
                {
                    _logger.LogWarning("Payment failed. TransactionId: {TransactionId}, StatusCode: {StatusCode}, Error: {Error}",
                        result.TransactionId, result.StatusCode, result.StatusDescription);
                }

                // Redirect to Angular success/failure page
                var redirectUrl = result.IsSuccess
                    ? $"/payment/success?transactionId={result.TransactionId}"
                    : $"/payment/failure?transactionId={result.TransactionId}&error={Uri.EscapeDataString(result.StatusDescription)}";

                return Redirect(redirectUrl);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid SecureHash in payment callback");
                return Redirect("/payment/failure?error=Invalid+payment+response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment callback");
                return Redirect("/payment/failure?error=Payment+processing+error");
            }
        }

        /// <summary>
        /// API endpoint to get callback result (for Angular to query)
        /// </summary>
        [HttpGet("result/{transactionId}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentCallbackDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCallbackResult(string transactionId)
        {
            try
            {
                // TODO: Retrieve payment result from database by TransactionId
                // For now, return not implemented

                _logger.LogInformation("Query payment result for TransactionId: {TransactionId}", transactionId);

                return NotFound(ApiResponse<object>.ErrorResponse("Payment result not found or not yet processed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment result");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error retrieving payment result"));
            }
        }
    }
}
