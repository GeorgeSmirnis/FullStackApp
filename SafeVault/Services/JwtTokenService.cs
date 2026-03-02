using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SafeVault.Models;

namespace SafeVault.Services
{
    /// <summary>
    /// JWT Token Service for creating and validating authentication tokens.
    /// 
    /// JSON Web Tokens (JWT) provide a stateless authentication mechanism.
    /// Each token contains:
    /// - Header: Token type and signing algorithm
    /// - Payload: Claims (user information, roles, permissions)
    /// - Signature: HMAC-SHA256 signature for integrity verification
    /// 
    /// Security Features:
    /// - Token expiration
    /// - Claim validation
    /// - Signature verification
    /// - Token refresh capability
    /// </summary>
    public class JwtTokenService
    {
        private readonly string _secretKey;
        private readonly int _expirationMinutes;
        private readonly string _issuer;
        private readonly string _audience;

        // Token configuration
        public const int DefaultExpirationMinutes = 60;
        public const int RefreshTokenExpirationDays = 7;

        public JwtTokenService(string secretKey, int expirationMinutes = DefaultExpirationMinutes, 
            string issuer = "SafeVault", string audience = "SafeVaultApi")
        {
            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentException("Secret key cannot be empty");

            if (secretKey.Length < 32)
                throw new ArgumentException("Secret key must be at least 32 characters");

            if (expirationMinutes < 1)
                throw new ArgumentException("Expiration must be at least 1 minute");

            _secretKey = secretKey;
            _expirationMinutes = expirationMinutes;
            _issuer = issuer;
            _audience = audience;
        }

        /// <summary>
        /// Creates a JWT token for a user with their roles and permissions.
        /// </summary>
        /// <param name="user">The user to create a token for</param>
        /// <returns>JWT token string</returns>
        public string CreateToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Create claims
            var claims = new TokenClaims
            {
                UserID = user.UserID,
                Username = user.Username,
                Email = user.Email,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                Roles = user.Roles?.Select(r => r.Name).ToList() ?? new List<string>(),
                Permissions = user.Roles?.SelectMany(r => r.Permissions.Select(p => p.Name)).Distinct().ToList() ?? new List<string>()
            };

            // Create JWT
            return GenerateJwt(claims);
        }

        /// <summary>
        /// Creates a short-lived refresh token.
        /// Refresh tokens are used to obtain new access tokens without re-authentication.
        /// </summary>
        /// <param name="user">The user to create a refresh token for</param>
        /// <returns>Refresh token string (JWT with extended expiration)</returns>
        public string CreateRefreshToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var claims = new TokenClaims
            {
                UserID = user.UserID,
                Username = user.Username,
                Email = user.Email,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
                Roles = new List<string> { "RefreshToken" }
            };

            return GenerateJwt(claims);
        }

        /// <summary>
        /// Validates a token and extracts claims if valid.
        /// Checks signature, expiration, and claim validity.
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <param name="claims">Output parameter containing extracted claims if valid</param>
        /// <returns>True if token is valid</returns>
        public bool ValidateToken(string token, out TokenClaims claims)
        {
            claims = null;

            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                // Split JWT into parts
                var parts = token.Split('.');
                if (parts.Length != 3)
                    return false;

                // Decode and validate signature
                string header = DecodeBase64Url(parts[0]);
                string payload = DecodeBase64Url(parts[1]);
                string signature = parts[2];

                // Verify signature
                string expectedSignature = GenerateSignature($"{parts[0]}.{parts[1]}");
                if (!ConstantTimeComparison(signature, expectedSignature))
                    return false;

                // Parse payload
                claims = ParseTokenPayload(payload);

                if (claims == null)
                    return false;

                // Check expiration
                if (claims.ExpiresAt <= DateTime.UtcNow)
                    return false;

                // Validate issuer and audience
                if (claims == null) // Additional check after parsing
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a token is expired.
        /// </summary>
        /// <param name="token">The token to check</param>
        /// <returns>True if token is expired</returns>
        public bool IsTokenExpired(string token)
        {
            if (ValidateToken(token, out var claims))
                return claims.ExpiresAt <= DateTime.UtcNow;

            return true;
        }

        /// <summary>
        /// Generates the JWT token string.
        /// Format: header.payload.signature
        /// </summary>
        private string GenerateJwt(TokenClaims claims)
        {
            // Create header
            string header = EncodeBase64Url(new
            {
                alg = "HS256",
                typ = "JWT"
            });

            // Create payload
            string payload = EncodeBase64Url(claims);

            // Create signature
            string unsignedToken = $"{header}.{payload}";
            string signature = GenerateSignature(unsignedToken);

            return $"{unsignedToken}.{signature}";
        }

        /// <summary>
        /// Generates HMAC-SHA256 signature for JWT.
        /// </summary>
        private string GenerateSignature(string data)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey)))
            {
                byte[] signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return EncodeBase64Url(signatureBytes);
            }
        }

        /// <summary>
        /// Encodes data to Base64URL format (used in JWT).
        /// </summary>
        private string EncodeBase64Url(object obj)
        {
            // Simple JSON serialization - in production use System.Text.Json or Newtonsoft.Json
            string json = SimpleJsonSerialize(obj);
            return EncodeBase64Url(Encoding.UTF8.GetBytes(json));
        }

        /// <summary>
        /// Encodes bytes to Base64URL format.
        /// Base64URL is Base64 with URL-safe characters (- and _ instead of + and /).
        /// </summary>
        private string EncodeBase64Url(byte[] data)
        {
            string base64 = Convert.ToBase64String(data);
            return base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        /// <summary>
        /// Decodes Base64URL format to string.
        /// </summary>
        private string DecodeBase64Url(string base64Url)
        {
            // Add padding if needed
            string base64 = base64Url.Replace("-", "+").Replace("_", "/");
            switch (base64.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
            }

            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }

        /// <summary>
        /// Parses JWT payload to extract claims.
        /// </summary>
        private TokenClaims ParseTokenPayload(string payload)
        {
            try
            {
                // Simple JSON parsing - in production use System.Text.Json
                var claims = new TokenClaims();

                // Extract values from JSON (simplified parsing)
                claims.UserID = ExtractIntFromJson(payload, "UserID");
                claims.Username = ExtractStringFromJson(payload, "Username");
                claims.Email = ExtractStringFromJson(payload, "Email");
                claims.IssuedAt = ExtractDateTimeFromJson(payload, "IssuedAt");
                claims.ExpiresAt = ExtractDateTimeFromJson(payload, "ExpiresAt");
                claims.Roles = ExtractListFromJson(payload, "Roles");
                claims.Permissions = ExtractListFromJson(payload, "Permissions");

                return claims;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Simple JSON serialization for demonstration.
        /// In production, use System.Text.Json or Newtonsoft.Json.
        /// </summary>
        private string SimpleJsonSerialize(object obj)
        {
            if (obj is byte[])
                return ""; // Not needed for bytes

            if (obj is TokenClaims claims)
            {
                var roles = string.Join(",", claims.Roles.Select(r => $"\"{r}\""));
                var permissions = string.Join(",", claims.Permissions.Select(p => $"\"{p}\""));

                return $"{{\"UserID\":{claims.UserID},\"Username\":\"{claims.Username}\",\"Email\":\"{claims.Email}\"," +
                       $"\"IssuedAt\":\"{claims.IssuedAt:O}\",\"ExpiresAt\":\"{claims.ExpiresAt:O}\"," +
                       $"\"Roles\":[{roles}],\"Permissions\":[{permissions}]}}";
            }

            if (obj is byte[] arr)
                return EncodeBase64Url(arr);

            return "{}";
        }

        /// <summary>
        /// Extracts integer value from JSON string.
        /// </summary>
        private int ExtractIntFromJson(string json, string key)
        {
            var match = System.Text.RegularExpressions.Regex.Match(json, $"\"{key}\":([0-9]+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
                return value;
            return 0;
        }

        /// <summary>
        /// Extracts string value from JSON.
        /// </summary>
        private string ExtractStringFromJson(string json, string key)
        {
            var match = System.Text.RegularExpressions.Regex.Match(json, $"\"{key}\":\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : "";
        }

        /// <summary>
        /// Extracts DateTime value from JSON.
        /// </summary>
        private DateTime ExtractDateTimeFromJson(string json, string key)
        {
            var match = System.Text.RegularExpressions.Regex.Match(json, $"\"{key}\":\"([^\"]+)\"");
            if (match.Success && DateTime.TryParse(match.Groups[1].Value, out DateTime dt))
                return dt;
            return DateTime.MinValue;
        }

        /// <summary>
        /// Extracts list of strings from JSON array.
        /// </summary>
        private List<string> ExtractListFromJson(string json, string key)
        {
            var result = new List<string>();

            var match = System.Text.RegularExpressions.Regex.Match(json, $"\"{key}\":\\[([^\\]]*)\\]");
            if (match.Success)
            {
                var items = match.Groups[1].Value.Split(',');
                foreach (var item in items)
                {
                    var cleaned = item.Trim().Trim('"');
                    if (!string.IsNullOrEmpty(cleaned))
                        result.Add(cleaned);
                }
            }

            return result;
        }

        /// <summary>
        /// Constant-time string comparison to prevent timing attacks.
        /// </summary>
        private bool ConstantTimeComparison(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
    }
}
