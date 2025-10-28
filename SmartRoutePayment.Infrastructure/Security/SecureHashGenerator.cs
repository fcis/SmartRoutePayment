using SmartRoutePayment.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Infrastructure.Security
{
    /// <summary>
    /// Generates and validates SHA256 secure hashes for SmartRoute Direct Post Payment
    /// Excludes sensitive card data from hash generation for security
    /// </summary>
    public class SecureHashGenerator : ISecureHashGenerator
    {
        /// <summary>
        /// Generates a secure hash for Direct Post Payment request
        /// </summary>
        /// <param name="parameters">Request parameters (card fields will be automatically excluded)</param>
        /// <param name="authenticationToken">SmartRoute authentication token</param>
        /// <returns>SHA256 hash in lowercase hexadecimal format</returns>
        /// <exception cref="ArgumentException">Thrown when parameters or token are invalid</exception>
        public string Generate(Dictionary<string, string> parameters, string authenticationToken)
        {
            if (parameters == null || parameters.Count == 0)
                throw new ArgumentException("Parameters cannot be null or empty", nameof(parameters));

            if (string.IsNullOrWhiteSpace(authenticationToken))
                throw new ArgumentException("Authentication token cannot be null or empty", nameof(authenticationToken));

            // IMPORTANT: Exclude SecureHash itself and card sensitive data from hash generation
            // Card fields are excluded for security - they should NOT be part of the hash
            var excludedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "SecureHash",        // Never include the hash itself
                "CardNumber",        // Sensitive: Card PAN
                "ExpiryDateYear",    // Sensitive: Card expiry
                "ExpiryDateMonth",   // Sensitive: Card expiry
                "SecurityCode",      // Sensitive: CVV/CVC
                "CardHolderName"     // Sensitive: Cardholder name
            };

            // Filter and sort parameters alphabetically by key (case-sensitive)
            var sortedParams = parameters
                .Where(p => !excludedKeys.Contains(p.Key))
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .ToList();

            // Create ordered string: AuthToken + Value1 + Value2 + ... (sorted alphabetically by key)
            // Format: TOKEN + sorted values concatenated (no separators)
            var orderedString = new StringBuilder();
            orderedString.Append(authenticationToken);

            foreach (var param in sortedParams)
            {
                orderedString.Append(param.Value);
            }

            // Generate SHA256 hash in lowercase hexadecimal format
            return ComputeSha256Hash(orderedString.ToString());
        }

        /// <summary>
        /// Validates a received secure hash against computed hash
        /// </summary>
        /// <param name="parameters">Response parameters (Response.SecureHash will be automatically excluded)</param>
        /// <param name="receivedHash">The secure hash received from SmartRoute</param>
        /// <param name="authenticationToken">SmartRoute authentication token</param>
        /// <returns>True if hashes match, false otherwise</returns>
        public bool Validate(Dictionary<string, string> parameters, string receivedHash, string authenticationToken)
        {
            if (string.IsNullOrWhiteSpace(receivedHash))
                return false;

            var computedHash = Generate(parameters, authenticationToken);

            // Case-insensitive comparison (both should be lowercase, but just to be safe)
            return string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Computes SHA256 hash and returns lowercase hexadecimal string
        /// </summary>
        /// <param name="input">Input string to hash</param>
        /// <returns>SHA256 hash in lowercase hexadecimal format (64 characters)</returns>
        private static string ComputeSha256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);

            // Convert byte array to lowercase hexadecimal string
            var builder = new StringBuilder(64); // SHA256 = 32 bytes = 64 hex chars
            foreach (var b in hashBytes)
            {
                builder.Append(b.ToString("x2")); // x2 = lowercase hex, 2 digits
            }

            return builder.ToString();
        }
    }
}
