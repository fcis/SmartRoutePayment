using Microsoft.Extensions.Logging;
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
    /// CORRECTED: No exclusions - SecureHash simply isn't in the dictionary yet
    /// </summary>
    public class RedirectSecureHashGenerator : ISecureHashGenerator
    {
        private readonly ILogger<RedirectSecureHashGenerator>? _logger;

        public RedirectSecureHashGenerator(ILogger<RedirectSecureHashGenerator>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generates secure hash for REQUEST
        /// Based on SmartRoute documentation:
        /// 1. Sort ALL parameters alphabetically by KEY (using Ordinal comparison)
        /// 2. Concatenate: AuthToken + Value1 + Value2 + ... (in alphabetical order)
        /// 3. No exclusions - SecureHash field simply doesn't exist yet
        /// </summary>
        public string Generate(Dictionary<string, string> parameters, string authenticationToken)
        {
            if (parameters == null || parameters.Count == 0)
                throw new ArgumentException("Parameters cannot be null or empty", nameof(parameters));

            if (string.IsNullOrWhiteSpace(authenticationToken))
                throw new ArgumentException("Authentication token cannot be null or empty", nameof(authenticationToken));

            // NO EXCLUSIONS!
            // Sort ALL parameters alphabetically by KEY using Ordinal comparison
            // SecureHash won't be in the dictionary because it hasn't been generated yet
            var sortedParams = parameters
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .ToList();

            // Build ordered string: AuthToken + Value1 + Value2 + ...
            var orderedString = new StringBuilder();
            orderedString.Append(authenticationToken);

            // Debug logging
            _logger?.LogInformation("========== SECURE HASH GENERATION ==========");
            _logger?.LogInformation("Auth Token (masked): {Token}***",
                authenticationToken.Substring(0, Math.Min(8, authenticationToken.Length)));
            _logger?.LogInformation("Total Parameters: {Count}", sortedParams.Count);

            Console.WriteLine("\n========== SECURE HASH GENERATION ==========");
            Console.WriteLine($"Auth Token (masked): {authenticationToken.Substring(0, Math.Min(8, authenticationToken.Length))}***");
            Console.WriteLine($"Total Parameters: {sortedParams.Count}");
            Console.WriteLine("\nParameters in ALPHABETICAL order by KEY:");

            int index = 1;
            foreach (var param in sortedParams)
            {
                // Append value (empty string if null)
                var value = param.Value ?? string.Empty;
                orderedString.Append(value);

                _logger?.LogInformation("  [{Index}] Key='{Key}' Value='{Value}'", index, param.Key, value);
                Console.WriteLine($"  [{index,2}] {param.Key,-25} = '{value}'");
                index++;
            }

            var concatenatedString = orderedString.ToString();

            _logger?.LogInformation("Concatenated String Length: {Length}", concatenatedString.Length);
            _logger?.LogInformation("String Preview (first 150 chars): {Preview}...",
                concatenatedString.Substring(0, Math.Min(150, concatenatedString.Length)));

            Console.WriteLine($"\nConcatenated String Length: {concatenatedString.Length}");
            Console.WriteLine($"String Preview: {concatenatedString.Substring(0, Math.Min(150, concatenatedString.Length))}...");

            // Generate SHA-256 hash (lowercase hex)
            var hash = ComputeSha256Hash(concatenatedString);

            _logger?.LogInformation("Generated Hash: {Hash}", hash);
            _logger?.LogInformation("===========================================\n");

            Console.WriteLine($"\nGenerated SHA-256 Hash: {hash}");
            Console.WriteLine("===========================================\n");

            return hash;
        }

        /// <summary>
        /// Validates secure hash for RESPONSE
        /// IMPORTANT: For response validation, we need to exclude Response.SecureHash
        /// because it's comparing the received hash
        /// 
        /// Also: Response.StatusDescription and Response.GatewayStatusDescription 
        /// must be URL-encoded before hash validation
        /// </summary>
        public bool Validate(Dictionary<string, string> parameters, string receivedHash, string authenticationToken)
        {
            if (string.IsNullOrWhiteSpace(receivedHash))
                return false;

            // Prepare parameters for validation
            // NOW we exclude Response.SecureHash because we're comparing it
            var parametersForValidation = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var param in parameters)
            {
                // Skip the SecureHash field itself (we're validating against it)
                if (param.Key.Equals("Response.SecureHash", StringComparison.OrdinalIgnoreCase))
                    continue;

                // URL-encode StatusDescription and GatewayStatusDescription
                // This is documented in SmartRoute examples (.NET code)
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

            // Generate hash from response parameters
            var computedHash = Generate(parametersForValidation, authenticationToken);
            var isValid = string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);

            // Debug logging
            _logger?.LogInformation("========== HASH VALIDATION ==========");
            _logger?.LogInformation("Received Hash: {ReceivedHash}", receivedHash);
            _logger?.LogInformation("Computed Hash: {ComputedHash}", computedHash);
            _logger?.LogInformation("Match: {IsValid}", isValid ? "YES ✓" : "NO ✗");
            _logger?.LogInformation("====================================\n");

            Console.WriteLine("\n========== HASH VALIDATION ==========");
            Console.WriteLine($"Received Hash: {receivedHash}");
            Console.WriteLine($"Computed Hash: {computedHash}");
            Console.WriteLine($"Match: {(isValid ? "YES ✓" : "NO ✗")}");
            Console.WriteLine("====================================\n");

            return isValid;
        }

        /// <summary>
        /// Computes SHA-256 hash and returns lowercase hexadecimal string
        /// </summary>
        private static string ComputeSha256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);

            // Convert to lowercase hexadecimal (required by SmartRoute)
            var builder = new StringBuilder(hashBytes.Length * 2);
            foreach (var b in hashBytes)
            {
                builder.Append(b.ToString("x2")); // lowercase hex
            }

            return builder.ToString();
        }
    }
}
