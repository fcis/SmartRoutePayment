using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartRoutePayment.Application.DTOs.Requests;
using SmartRoutePayment.Application.Interfaces;

namespace SmartRoutePayment.API.Controllers
{
    /// <summary>
    /// Controller for transaction inquiry operations
    /// B2B backend-only communication with PayOne service
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InquiryController : ControllerBase
    {
        private readonly IInquiryService _inquiryService;
        private readonly ILogger<InquiryController> _logger;

        public InquiryController(
            IInquiryService inquiryService,
            ILogger<InquiryController> logger)
        {
            _inquiryService = inquiryService ?? throw new ArgumentNullException(nameof(inquiryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Inquire about a transaction status
        /// </summary>
        /// <param name="request">Inquiry request with original transaction ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transaction inquiry response</returns>
        /// <response code="200">Returns the inquiry response with transaction status</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="500">If an internal server error occurs</response>
        [HttpPost("transaction")]
        [ProducesResponseType(typeof(Application.DTOs.Responses.InquiryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InquireTransaction(
            [FromBody] InquiryRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Received inquiry request for OriginalTransactionID: {TransactionId}",
                    request?.OriginalTransactionID);

                if (request == null)
                {
                    _logger.LogWarning("Inquiry request is null");
                    return BadRequest(new { error = "Request body is required" });
                }

                var response = await _inquiryService.InquireTransactionAsync(request, cancellationToken);

                if (response.IsSuccess)
                {
                    _logger.LogInformation("Inquiry successful for TransactionID: {TransactionId}",
                        response.TransactionID);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Inquiry failed for OriginalTransactionID: {TransactionId}, Error: {Error}",
                        request.OriginalTransactionID,
                        response.ErrorMessage);
                    return Ok(response); // Still return 200 with error details in response
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error during inquiry");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during inquiry for OriginalTransactionID: {TransactionId}",
                    request?.OriginalTransactionID);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { error = "An error occurred while processing the inquiry request" });
            }
        }
    }
}
