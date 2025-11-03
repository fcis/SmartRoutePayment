using SmartRoutePayment.Application.DTOs.Requests;
using SmartRoutePayment.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> ProcessPaymentAsync(
            PaymentRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Prepares payment parameters and generates secure hash for frontend
        /// Frontend will add card data and post directly to PayOne
        /// </summary>
        Task<PreparePaymentResponseDto> PreparePaymentAsync(
            PreparePaymentRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
