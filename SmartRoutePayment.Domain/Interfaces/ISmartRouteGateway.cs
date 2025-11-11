using SmartRoutePayment.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Domain.Interfaces
{
    public interface ISmartRouteGateway
    {
        /// <summary>
        /// Process Direct Post Payment through SmartRoute Gateway
        /// </summary>
        Task<PaymentResponse> ProcessPaymentAsync(
            PaymentRequest paymentRequest,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Inquire about a transaction status through SmartRoute Gateway (B2B)
        /// Server-to-server communication only
        /// </summary>
        Task<InquiryResponse> InquireTransactionAsync(
            InquiryRequest inquiryRequest,
            CancellationToken cancellationToken = default);
    }
}
