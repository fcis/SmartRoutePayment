using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using SmartRoutePayment.Domain.Interfaces;
using SmartRoutePayment.Infrastructure.Configuration;
using SmartRoutePayment.Infrastructure.Gateways;
using SmartRoutePayment.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ============================================
            // Configuration
            // ============================================

            // Bind SmartRoute configuration settings
            services.Configure<SmartRouteSettings>(
                configuration.GetSection(SmartRouteSettings.SectionName));

            // Register configuration provider for application layer
            services.AddSingleton<IPaymentConfigurationProvider, PaymentConfigurationProvider>();

            // ============================================
            // Secure Hash Generators
            // ============================================

            // Register secure hash generator for Direct Post Model (existing)
            services.AddSingleton<ISecureHashGenerator, SecureHashGenerator>();



            // ============================================
            // Direct Post Model Gateway (Existing)
            // ============================================

            // Register HTTP client for SmartRoute Direct Post gateway
            services.AddHttpClient<ISmartRouteGateway, SmartRouteGateway>((serviceProvider, client) =>
            {
                var settings = configuration
                    .GetSection(SmartRouteSettings.SectionName)
                    .Get<SmartRouteSettings>();

                client.Timeout = TimeSpan.FromSeconds(settings?.TimeoutSeconds ?? 30);
                client.DefaultRequestHeaders.Add("Accept", "application/x-www-form-urlencoded");
            });



            return services;
        }
    }
}