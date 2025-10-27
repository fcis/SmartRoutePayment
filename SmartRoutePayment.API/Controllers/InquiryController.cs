using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartRoutePayment.API.Models;
using SmartRoutePayment.Application.DTOs.Requests.RedirectModel;
using SmartRoutePayment.Application.DTOs.Responses.RedirectModel;
using SmartRoutePayment.Application.Interfaces;

namespace SmartRoutePayment.API.Controllers
{
    [Route("api/inquiry")]
    [ApiController]
    [Produces("application/json")]
    public class InquiryController : ControllerBase
    {
        private readonly IInquiryService _inquiryService;
        private readonly ILogger<InquiryController> _logger;

        public InquiryController(
            IInquiryService inquiryService,
            ILogger<InquiryController> logger)
        {
            _inquiryService = inquiryService;
            _logger = logger;
        }

        /// <summary>
        /// Inquires about the status of a specific transaction
        /// Use this to check payment status, reversal status, and refund information
        /// </summary>
        /// <param name="request">Inquiry request with original transaction ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transaction status and details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<InquiryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InquireTransaction(
            [FromBody] InquiryRequestDto request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Inquiring transaction status for OriginalTransactionId: {TransactionId}",
                request.OriginalTransactionId);

            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.OriginalTransactionId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Validation failed",
                        new List<string> { "OriginalTransactionId is required" }));
                }

                // Execute inquiry
                var result = await _inquiryService.InquireTransactionAsync(request, cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Inquiry successful. TransactionId: {TransactionId}, StatusCode: {StatusCode}, ReversalStatus: {ReversalStatus}",
                        result.TransactionId,
                        result.StatusCode,
                        result.ReversalStatus);

                    return Ok(ApiResponse<InquiryResponseDto>.SuccessResponse(
                        result,
                        "Transaction inquiry completed successfully"));
                }
                else
                {
                    _logger.LogWarning(
                        "Inquiry failed. TransactionId: {TransactionId}, Error: {Error}",
                        result.TransactionId,
                        result.ErrorMessage);

                    return Ok(ApiResponse<InquiryResponseDto>.SuccessResponse(
                        result,
                        "Transaction inquiry completed with errors"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inquiring transaction status");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while inquiring transaction status",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Inquires about transaction with refund information included
        /// </summary>
        /// <param name="originalTransactionId">Original transaction ID to inquire</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transaction status with refund details</returns>
        [HttpGet("{originalTransactionId}")]
        [ProducesResponseType(typeof(ApiResponse<InquiryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTransactionStatus(
            string originalTransactionId,
            [FromQuery] bool includeRefundInfo = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Getting transaction status for OriginalTransactionId: {TransactionId}, IncludeRefundInfo: {IncludeRefundInfo}",
                originalTransactionId,
                includeRefundInfo);

            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(originalTransactionId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Validation failed",
                        new List<string> { "OriginalTransactionId is required" }));
                }

                var request = new InquiryRequestDto
                {
                    OriginalTransactionId = originalTransactionId,
                    IncludeRefundIds = includeRefundInfo ? "Yes" : null
                };

                // Execute inquiry
                var result = await _inquiryService.InquireTransactionAsync(request, cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Transaction status retrieved successfully. TransactionId: {TransactionId}, StatusCode: {StatusCode}",
                        result.TransactionId,
                        result.StatusCode);

                    return Ok(ApiResponse<InquiryResponseDto>.SuccessResponse(
                        result,
                        "Transaction status retrieved successfully"));
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to retrieve transaction status. TransactionId: {TransactionId}, Error: {Error}",
                        result.TransactionId,
                        result.ErrorMessage);

                    return Ok(ApiResponse<InquiryResponseDto>.SuccessResponse(
                        result,
                        "Transaction status retrieval completed with errors"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction status");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while retrieving transaction status",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Health check endpoint for inquiry service
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(ApiResponse<object>.SuccessResponse(
                new { status = "healthy", service = "inquiry", timestamp = DateTime.UtcNow },
                "Inquiry service is running"));
        }
    }
}
