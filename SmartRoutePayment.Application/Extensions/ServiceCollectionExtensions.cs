using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using SmartRoutePayment.Application.Interfaces;
using SmartRoutePayment.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {


            // ============================================
            // Direct Post Model Services (Existing)
            // ============================================

            // Register Direct Post payment service
            services.AddScoped<IPaymentService, PaymentService>();

  
            // ============================================



            return services;
        }
    }
}

