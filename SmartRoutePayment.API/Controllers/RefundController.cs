using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartRoutePayment.API.Models;
using SmartRoutePayment.Application.DTOs.Requests.RedirectModel;
using SmartRoutePayment.Application.DTOs.Responses.RedirectModel;
using SmartRoutePayment.Application.Interfaces;

namespace SmartRoutePayment.API.Controllers
{
    [Route("api/refund")]
    [ApiController]
    [Produces("application/json")]
    public class RefundController : ControllerBase
    {
        private readonly IRefundService _refundService;
        private readonly ILogger<RefundController> _logger;

        public RefundController(
            IRefundService refundService,
            ILogger<RefundController> logger)
        {
            _refundService = refundService;
            _logger = logger;
        }

        /// <summary>
        /// Refunds a specific transaction (partial or full)
        /// Creates a new refund transaction linked to the original payment
        /// </summary>
        /// <param name="request">Refund request with original transaction ID and refund amount</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refund confirmation with new refund transaction ID</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RefundResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefundTransaction(
            [FromBody] RefundRequestDto request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing refund for OriginalTransactionId: {TransactionId}, RefundAmount: {Amount}",
                request.OriginalTransactionId,
                request.RefundAmount);

            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.OriginalTransactionId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Validation failed",
                        new List<string> { "OriginalTransactionId is required" }));
                }

                if (request.RefundAmount <= 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Validation failed",
                        new List<string> { "RefundAmount must be greater than zero" }));
                }

                // Execute refund
                var result = await _refundService.RefundTransactionAsync(request, cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Refund successful. RefundTransactionId: {RefundTransactionId}, OriginalTransactionId: {OriginalTransactionId}, Amount: {Amount}",
                        result.RefundTransactionId,
                        result.OriginalTransactionId,
                        result.Amount);

                    return Ok(ApiResponse<RefundResponseDto>.SuccessResponse(
                        result,
                        "Refund processed successfully"));
                }
                else
                {
                    _logger.LogWarning(
                        "Refund failed. RefundTransactionId: {RefundTransactionId}, StatusCode: {StatusCode}, Error: {Error}",
                        result.RefundTransactionId,
                        result.StatusCode,
                        result.ErrorMessage);

                    return Ok(ApiResponse<RefundResponseDto>.SuccessResponse(
                        result,
                        "Refund processing completed with errors"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while processing refund",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Quick refund endpoint - refund using URL parameters
        /// </summary>
        /// <param name="originalTransactionId">Original transaction ID to refund</param>
        /// <param name="amount">Refund amount</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refund confirmation</returns>
        [HttpPost("{originalTransactionId}/refund")]
        [ProducesResponseType(typeof(ApiResponse<RefundResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> QuickRefund(
            string originalTransactionId,
            [FromQuery] decimal amount,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing quick refund for OriginalTransactionId: {TransactionId}, Amount: {Amount}",
                originalTransactionId,
                amount);

            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(originalTransactionId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Validation failed",
                        new List<string> { "OriginalTransactionId is required" }));
                }

                if (amount <= 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Validation failed",
                        new List<string> { "Amount must be greater than zero" }));
                }

                var request = new RefundRequestDto
                {
                    OriginalTransactionId = originalTransactionId,
                    RefundAmount = amount
                };

                // Execute refund
                var result = await _refundService.RefundTransactionAsync(request, cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Quick refund successful. RefundTransactionId: {RefundTransactionId}",
                        result.RefundTransactionId);

                    return Ok(ApiResponse<RefundResponseDto>.SuccessResponse(
                        result,
                        "Quick refund processed successfully"));
                }
                else
                {
                    _logger.LogWarning(
                        "Quick refund failed. RefundTransactionId: {RefundTransactionId}, Error: {Error}",
                        result.RefundTransactionId,
                        result.ErrorMessage);

                    return Ok(ApiResponse<RefundResponseDto>.SuccessResponse(
                        result,
                        "Quick refund processing completed with errors"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing quick refund");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while processing quick refund",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Health check endpoint for refund service
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(ApiResponse<object>.SuccessResponse(
                new { status = "healthy", service = "refund", timestamp = DateTime.UtcNow },
                "Refund service is running"));
        }
    }
}
