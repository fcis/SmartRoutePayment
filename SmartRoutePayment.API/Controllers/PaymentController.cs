using Microsoft.AspNetCore.Mvc;
using SmartRoutePayment.API.Models;
using SmartRoutePayment.Application.DTOs.Requests;
using SmartRoutePayment.Application.DTOs.Responses;
using SmartRoutePayment.Application.Interfaces;

namespace SmartRoutePayment.API.Controllers
{
    /// <summary>
    /// Payment Controller for Payone Direct Post Payment Integration
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Prepares payment parameters for Direct Post Payment
        /// Frontend will collect card details and post directly to Payone
        /// </summary>
        /// <param name="request">Payment preparation request (NO card data)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>All parameters needed for frontend to post to Payone</returns>
        [HttpPost("prepare")]
        [ProducesResponseType(typeof(ApiResponse<PreparePaymentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PreparePayment(
            [FromBody] PreparePaymentRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Preparing payment for amount: {Amount} SAR", request.Amount);

                // Validate request
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
                }

                // Prepare payment
                var response = await _paymentService.PreparePaymentAsync(request, cancellationToken);

                _logger.LogInformation("Payment prepared successfully. TransactionId: {TransactionId}", response.TransactionId);

                return Ok(ApiResponse<PreparePaymentResponseDto>.SuccessResponse(response, "Payment prepared successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid payment preparation request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing payment");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while preparing payment"));
            }
        }


    }
}