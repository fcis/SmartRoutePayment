using SmartRoutePayment.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Infrastructure.Security
{
    public class SecureHashGenerator : ISecureHashGenerator
    {
        public string Generate(Dictionary<string, string> parameters, string authenticationToken)
        {
            if (parameters == null || parameters.Count == 0)
                throw new ArgumentException("Parameters cannot be null or empty", nameof(parameters));

            if (string.IsNullOrWhiteSpace(authenticationToken))
                throw new ArgumentException("Authentication token cannot be null or empty", nameof(authenticationToken));

            // IMPORTANT: Exclude SecureHash itself and card sensitive data from hash generation
            var excludedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SecureHash",
            "CardNumber",
            "ExpiryDateYear",
            "ExpiryDateMonth",
            "SecurityCode",
            "CardHolderName"
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
                orderedString.Append(param.Value);
            }

            // Generate SHA256 hash
            return ComputeSha256Hash(orderedString.ToString());
        }

        public bool Validate(Dictionary<string, string> parameters, string receivedHash, string authenticationToken)
        {
            if (string.IsNullOrWhiteSpace(receivedHash))
                return false;

            var computedHash = Generate(parameters, authenticationToken);
            return string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);
        }

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
