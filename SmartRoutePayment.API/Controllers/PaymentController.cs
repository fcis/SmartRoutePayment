using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartRoutePayment.API.Models;
using SmartRoutePayment.Application.DTOs.Requests;
using SmartRoutePayment.Application.DTOs.Responses;
using SmartRoutePayment.Application.Interfaces;

namespace SmartRoutePayment.API.Controllers
{
    /// <summary>
    /// Payment controller for processing Mada card payments through SmartRoute Direct Post Model
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
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
        /// Process a payment using Mada card via Direct Post Payment
        /// </summary>
        /// <param name="request">Payment request details including card information</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Payment response with transaction status</returns>
        /// <response code="200">Payment processed successfully</response>
        /// <response code="400">Invalid request (validation failed)</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("process")]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessPayment(
            [FromBody] PaymentRequestDto request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing Direct Post Payment request for amount: {Amount} SAR",
                request.Amount / 100); // Convert fils to SAR for logging

            // No need for manual validation - FluentValidation handles this automatically
            // via AddFluentValidationAutoValidation() in Program.cs

            // Process payment
            var result = await _paymentService.ProcessPaymentAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Payment processed successfully. TransactionId: {TransactionId}, ApprovalCode: {ApprovalCode}, StatusCode: {StatusCode}",
                    result.TransactionId,
                    result.ApprovalCode,
                    result.StatusCode);

                return Ok(ApiResponse<PaymentResponseDto>.SuccessResponse(
                    result,
                    "Payment processed successfully"));
            }

            // Payment failed - return 400 Bad Request with error details
            _logger.LogWarning(
                "Payment processing failed. TransactionId: {TransactionId}, StatusCode: {StatusCode}, Error: {ErrorMessage}",
                result.TransactionId,
                result.StatusCode,
                result.ErrorMessage);

            return BadRequest(ApiResponse<PaymentResponseDto>.ErrorResponse(
                "Payment processing failed",
                new List<string> { result.ErrorMessage }));
        }

        /// <summary>
        /// Health check endpoint to verify API availability
        /// </summary>
        /// <returns>API health status</returns>
        /// <response code="200">API is healthy and running</response>
        [HttpGet("health")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            _logger.LogDebug("Health check requested");

            return Ok(ApiResponse<object>.SuccessResponse(
                new
                {
                    status = "healthy",
                    service = "SmartRoute Payment API",
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
                },
                "API is running"));
        }
    }
}
