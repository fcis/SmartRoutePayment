using Microsoft.Extensions.Options;
using SmartRoutePayment.Domain.Entities.RedirectModel;
using SmartRoutePayment.Domain.Interfaces;
using SmartRoutePayment.Infrastructure.Configuration;
using SmartRoutePayment.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Infrastructure.Gateways.RedirectModel
{
    /// <summary>
    /// Gateway implementation for Redirect Payment operations
    /// Implements Redirection Communication Model
    /// </summary>
    public class RedirectPaymentGateway : IRedirectPaymentGateway
    {
        private readonly SmartRouteSettings _settings;
        private readonly RedirectSecureHashGenerator _secureHashGenerator;

        public RedirectPaymentGateway(
            IOptions<SmartRouteSettings> settings,
            RedirectSecureHashGenerator secureHashGenerator)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _secureHashGenerator = secureHashGenerator ?? throw new ArgumentNullException(nameof(secureHashGenerator));
        }

        /// <summary>
        /// Prepares redirect payment request with secure hash
        /// Returns form parameters for client-side redirect
        /// </summary>
        public Task<Dictionary<string, string>> PrepareRedirectPaymentAsync(
            RedirectPaymentRequest request,
            CancellationToken cancellationToken = default)
        {
            // Build request parameters
            var parameters = BuildRequestParameters(request);

            // Generate secure hash
            var secureHash = _secureHashGenerator.Generate(parameters, _settings.AuthenticationToken);
            parameters.Add("SecureHash", secureHash);

            // Return parameters ready for form submission
            return Task.FromResult(parameters);
        }

        /// <summary>
        /// Validates and parses redirect payment response from SmartRoute
        /// </summary>
        public Task<RedirectPaymentResponse> ValidateRedirectPaymentResponseAsync(
            Dictionary<string, string> responseParameters,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Parse response
                var response = ParseResponse(responseParameters);

                // Validate secure hash
                var receivedHash = responseParameters.GetValueOrDefault("Response.SecureHash", string.Empty);

                var isHashValid = _secureHashGenerator.Validate(
                    responseParameters,
                    receivedHash,
                    _settings.AuthenticationToken);

                if (!isHashValid)
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Invalid secure hash - response may be tampered";
                    return Task.FromResult(response);
                }

                // Check if payment was successful (00000 = Success)
                response.IsSuccess = response.StatusCode == "00000";
                response.ErrorMessage = response.IsSuccess
                    ? string.Empty
                    : response.StatusDescription;

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new RedirectPaymentResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error parsing response: {ex.Message}",
                    StatusCode = "ERROR",
                    StatusDescription = "Response parsing failed",
                    ProcessedAt = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Gets the SmartRoute payment URL for redirect
        /// </summary>
        public string GetPaymentUrl()
        {
            return _settings.PaymentUrl;
        }

        /// <summary>
        /// Builds request parameters dictionary for redirect payment
        /// </summary>
        private Dictionary<string, string> BuildRequestParameters(RedirectPaymentRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                { "MessageID", request.MessageId },
                { "TransactionID", request.TransactionId },
                { "MerchantID", _settings.MerchantId },
                { "Amount", request.Amount },
                { "CurrencyISOCode", request.CurrencyIsoCode },
                { "Language", request.Language },
                { "Version", request.Version },
                { "Channel", request.Channel.ToString() },
                { "Quantity", request.Quantity.ToString() }
            };

            // Add optional parameters if provided
            if (!string.IsNullOrWhiteSpace(request.ThemeId))
                parameters.Add("ThemeID", request.ThemeId);

            if (!string.IsNullOrWhiteSpace(request.PaymentDescription))
                parameters.Add("PaymentDescription", request.PaymentDescription);

            if (!string.IsNullOrWhiteSpace(request.ItemId))
                parameters.Add("ItemID", request.ItemId);

            if (!string.IsNullOrWhiteSpace(request.ResponseBackUrl))
                parameters.Add("ResponseBackURL", request.ResponseBackUrl);

            if (!string.IsNullOrWhiteSpace(request.GenerateToken))
                parameters.Add("GenerateToken", request.GenerateToken);

            if (!string.IsNullOrWhiteSpace(request.Token))
                parameters.Add("Token", request.Token);

            if (!string.IsNullOrWhiteSpace(request.AgreementId))
                parameters.Add("AgreementID", request.AgreementId);

            if (!string.IsNullOrWhiteSpace(request.AgreementType))
                parameters.Add("AgreementType", request.AgreementType);

            if (!string.IsNullOrWhiteSpace(request.PreferredPaymentMethod))
                parameters.Add("PreferredPaymentMethod", request.PreferredPaymentMethod);

            if (!string.IsNullOrWhiteSpace(request.FailedPaymentReplyUrl))
                parameters.Add("FailedPaymentReplyURL", request.FailedPaymentReplyUrl);

            return parameters;
        }

        /// <summary>
        /// Parses response parameters into RedirectPaymentResponse entity
        /// </summary>
        private static RedirectPaymentResponse ParseResponse(Dictionary<string, string> responseParams)
        {
            return new RedirectPaymentResponse
            {
                StatusCode = responseParams.GetValueOrDefault("Response.StatusCode", string.Empty),
                StatusDescription = responseParams.GetValueOrDefault("Response.StatusDescription", string.Empty),
                Amount = responseParams.GetValueOrDefault("Response.Amount", string.Empty),
                CurrencyIsoCode = responseParams.GetValueOrDefault("Response.CurrencyISOCode", string.Empty),
                AuthorizedAmount = responseParams.GetValueOrDefault("Response.AuthorizedAmount"),
                AuthorizedCurrencyIsoCode = responseParams.GetValueOrDefault("Response.AuthorizedCurrencyISOCode"),
                MerchantId = responseParams.GetValueOrDefault("Response.MerchantID", string.Empty),
                TransactionId = responseParams.GetValueOrDefault("Response.TransactionID", string.Empty),
                MessageId = responseParams.GetValueOrDefault("Response.MessageID", string.Empty),
                SecureHash = responseParams.GetValueOrDefault("Response.SecureHash", string.Empty),
                CardExpiryDate = responseParams.GetValueOrDefault("Response.CardExpiryDate"),
                CardHolderName = responseParams.GetValueOrDefault("Response.CardHolderName"),
                CardNumber = responseParams.GetValueOrDefault("Response.CardNumber"),
                GatewayStatusCode = responseParams.GetValueOrDefault("Response.GatewayStatusCode"),
                GatewayStatusDescription = responseParams.GetValueOrDefault("Response.GatewayStatusDescription"),
                GatewayName = responseParams.GetValueOrDefault("Response.GatewayName"),
                Rrn = responseParams.GetValueOrDefault("Response.RRN"),
                ApprovalCode = responseParams.GetValueOrDefault("Response.ApprovalCode"),
                Token = responseParams.GetValueOrDefault("Response.Token"),
                PaymentMethod = responseParams.GetValueOrDefault("Response.PaymentMethod"),
                IssuerName = responseParams.GetValueOrDefault("Response.IssuerName"),
                ProcessedAt = DateTime.UtcNow
            };
        }
    }
}
