using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartRoutePayment.API.Models;
using SmartRoutePayment.Application.DTOs.Requests;
using SmartRoutePayment.Application.DTOs.Responses;
using SmartRoutePayment.Application.Interfaces;

namespace SmartRoutePayment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IValidator<PaymentRequestDto> _validator;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            IValidator<PaymentRequestDto> validator,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _validator = validator;
            _logger = logger;
        }

        /// <summary>
        /// Process a payment using Mada card
        /// </summary>
        /// <param name="request">Payment request details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Payment response</returns>
        [HttpPost("process")]
        //[ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessPayment(
            [FromBody] PaymentRequestDto request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing payment request for amount: {Amount}", request.Amount);

            // Validate request
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("Payment request validation failed: {Errors}", string.Join(", ", errors));

                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    errors));
            }

            // Process payment
            var result = await _paymentService.ProcessPaymentAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Payment processed successfully. TransactionId: {TransactionId}, ApprovalCode: {ApprovalCode}",
                    result.TransactionId,
                    result.ApprovalCode);

                return Ok(ApiResponse<PaymentResponseDto>.SuccessResponse(
                    result,
                    "Payment processed successfully"));
            }

            _logger.LogWarning(
                "Payment processing failed. TransactionId: {TransactionId}, StatusCode: {StatusCode}, Error: {Error}",
                result.TransactionId,
                result.StatusCode,
                result.ErrorMessage);

            return Ok(ApiResponse<PaymentResponseDto>.SuccessResponse(
                result,
                "Payment processing completed with errors"));
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(ApiResponse<object>.SuccessResponse(
                new { status = "healthy", timestamp = DateTime.UtcNow },
                "API is running"));
        }
    }
}
