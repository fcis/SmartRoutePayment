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
            // Bind configuration
            services.Configure<SmartRouteSettings>(
                configuration.GetSection(SmartRouteSettings.SectionName));

            // Register secure hash generator
            services.AddSingleton<ISecureHashGenerator, SecureHashGenerator>();

            // Register HTTP client for SmartRoute gateway
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
