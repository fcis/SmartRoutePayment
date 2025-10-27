using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartRoutePayment.API.Models;
using SmartRoutePayment.Application.DTOs.Requests.RedirectModel;
using SmartRoutePayment.Application.DTOs.Responses.RedirectModel;
using SmartRoutePayment.Application.Interfaces;

namespace SmartRoutePayment.API.Controllers
{
    [Route("api/redirect-payment")]
    [ApiController]
    [Produces("application/json")]
    public class RedirectPaymentController : ControllerBase
    {
        private readonly IRedirectPaymentService _redirectPaymentService;
        private readonly ILogger<RedirectPaymentController> _logger;

        public RedirectPaymentController(
            IRedirectPaymentService redirectPaymentService,
            ILogger<RedirectPaymentController> logger)
        {
            _redirectPaymentService = redirectPaymentService;
            _logger = logger;
        }

        /// <summary>
        /// Initiates redirect payment - returns payment URL and form parameters
        /// Client should use this to redirect user to SmartRoute payment page
        /// </summary>
        /// <param name="request">Payment request details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Payment URL and form parameters for redirect</returns>
        [HttpPost("initiate")]
        [ProducesResponseType(typeof(ApiResponse<RedirectPaymentInitiationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InitiatePayment(
            [FromBody] RedirectPaymentRequestDto request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initiating redirect payment for amount: {Amount}", request.Amount);

            try
            {
                // Validate amount
                if (request.Amount <= 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Validation failed",
                        new List<string> { "Amount must be greater than zero" }));
                }

                // Initiate payment
                var result = await _redirectPaymentService.InitiatePaymentAsync(request, cancellationToken);

                _logger.LogInformation(
                    "Redirect payment initiated successfully. TransactionId: {TransactionId}, PaymentUrl: {PaymentUrl}",
                    result.TransactionId,
                    result.PaymentUrl);

                return Ok(ApiResponse<RedirectPaymentInitiationResponseDto>.SuccessResponse(
                    result,
                    "Payment initiated successfully. Redirect user to PaymentUrl with form parameters."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating redirect payment");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while initiating payment",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Callback endpoint - SmartRoute redirects here after payment
        /// This endpoint receives form POST from SmartRoute with payment result
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Payment result</returns>
        [HttpPost("callback")]
        [Consumes("application/x-www-form-urlencoded")]
        [ProducesResponseType(typeof(ApiResponse<RedirectPaymentCallbackResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PaymentCallback(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received payment callback from SmartRoute");

            try
            {
                // Read form parameters from callback
                var callbackParameters = Request.Form.Keys
                    .ToDictionary(key => key, key => Request.Form[key].ToString());

                _logger.LogInformation("Processing callback with {Count} parameters", callbackParameters.Count);

                // Process callback
                var result = await _redirectPaymentService.ProcessCallbackAsync(
                    callbackParameters,
                    cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Payment successful. TransactionId: {TransactionId}, ApprovalCode: {ApprovalCode}",
                        result.TransactionId,
                        result.ApprovalCode);

                    return Ok(ApiResponse<RedirectPaymentCallbackResponseDto>.SuccessResponse(
                        result,
                        "Payment processed successfully"));
                }
                else
                {
                    _logger.LogWarning(
                        "Payment failed. TransactionId: {TransactionId}, StatusCode: {StatusCode}, Error: {Error}",
                        result.TransactionId,
                        result.StatusCode,
                        result.ErrorMessage);

                    return Ok(ApiResponse<RedirectPaymentCallbackResponseDto>.SuccessResponse(
                        result,
                        "Payment processing completed with errors"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment callback");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while processing payment callback",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Health check endpoint for redirect payment service
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(ApiResponse<object>.SuccessResponse(
                new { status = "healthy", service = "redirect-payment", timestamp = DateTime.UtcNow },
                "Redirect Payment service is running"));
        }
    }
}
