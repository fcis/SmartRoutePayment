using SmartRoutePayment.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SmartRoutePayment.Infrastructure.Security
{
    /// <summary>
    /// Secure hash generator for Redirectional Model
    /// Handles hash generation and validation for Redirect Payment, Inquiry, and Refund operations
    /// </summary>
    public class RedirectSecureHashGenerator : ISecureHashGenerator
    {
        /// <summary>
        /// Generates secure hash for Redirectional Model requests
        /// For Redirect Payment: Excludes SecureHash field only
        /// For Inquiry/Refund: Includes all parameters
        /// </summary>
        /// <param name="parameters">Request parameters</param>
        /// <param name="authenticationToken">Merchant authentication token</param>
        /// <returns>SHA-256 hex-encoded hash</returns>
        public string Generate(Dictionary<string, string> parameters, string authenticationToken)
        {
            if (parameters == null || parameters.Count == 0)
                throw new ArgumentException("Parameters cannot be null or empty", nameof(parameters));

            if (string.IsNullOrWhiteSpace(authenticationToken))
                throw new ArgumentException("Authentication token cannot be null or empty", nameof(authenticationToken));

            // For Redirectional Model: Only exclude SecureHash itself
            // All other parameters (including optional ones) are included if present
            var excludedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "SecureHash"
            };

            // Filter and sort parameters alphabetically by key
            var sortedParams = parameters
                .Where(p => !excludedKeys.Contains(p.Key))
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .ToList();

            // Create ordered string: AuthToken + Value1 + Value2 + ... (sorted alphabetically)
            var orderedString = new StringBuilder();
            orderedString.Append(authenticationToken);

            foreach (var param in sortedParams)
            {
                // Add parameter value (empty string if null)
                orderedString.Append(param.Value ?? string.Empty);
            }

            // Generate SHA256 hash
            return ComputeSha256Hash(orderedString.ToString());
        }

        /// <summary>
        /// Validates secure hash for Redirectional Model responses
        /// Handles URL-encoded parameters for StatusDescription and GatewayStatusDescription
        /// </summary>
        /// <param name="parameters">Response parameters</param>
        /// <param name="receivedHash">Secure hash received from SmartRoute</param>
        /// <param name="authenticationToken">Merchant authentication token</param>
        /// <returns>True if hash is valid, false otherwise</returns>
        public bool Validate(Dictionary<string, string> parameters, string receivedHash, string authenticationToken)
        {
            if (string.IsNullOrWhiteSpace(receivedHash))
                return false;

            // For response validation, we need to handle URL-encoded parameters
            var parametersForValidation = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var param in parameters)
            {
                // Skip the SecureHash field itself
                if (param.Key.Equals("Response.SecureHash", StringComparison.OrdinalIgnoreCase))
                    continue;

                // URL-encode StatusDescription and GatewayStatusDescription for validation
                if (param.Key.Equals("Response.StatusDescription", StringComparison.OrdinalIgnoreCase) ||
                    param.Key.Equals("Response.GatewayStatusDescription", StringComparison.OrdinalIgnoreCase))
                {
                    parametersForValidation[param.Key] = HttpUtility.UrlEncode(param.Value, Encoding.UTF8);
                }
                else
                {
                    parametersForValidation[param.Key] = param.Value;
                }
            }

            var computedHash = Generate(parametersForValidation, authenticationToken);
            return string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Computes SHA-256 hash and returns lowercase hexadecimal string
        /// </summary>
        /// <param name="input">Input string to hash</param>
        /// <returns>Lowercase hex-encoded SHA-256 hash</returns>
        private static string ComputeSha256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);

            // Convert byte array to lowercase hexadecimal string
            var builder = new StringBuilder();
            foreach (var b in hashBytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
