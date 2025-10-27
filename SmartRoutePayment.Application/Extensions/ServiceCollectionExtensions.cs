using Microsoft.Extensions.DependencyInjection;
using SmartRoutePayment.Application.Interfaces;
using SmartRoutePayment.Application.Services;
using SmartRoutePayment.Application.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Add this using if you are using FluentValidation
using FluentValidation;
using FluentValidation.AspNetCore;

namespace SmartRoutePayment.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register validators
            services.AddValidatorsFromAssemblyContaining<PaymentRequestValidator>();

            // Register application services
            services.AddScoped<IPaymentService, PaymentService>();

            return services;
        }
    }
}
