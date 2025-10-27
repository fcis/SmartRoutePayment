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
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default);
    }
}
