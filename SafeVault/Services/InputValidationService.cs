using System;
using System.Text.RegularExpressions;

namespace SafeVault.Services
{
    /// <summary>
    /// Secure input validation service that protects against XSS and injection attacks.
    /// This service sanitizes and validates user inputs before they are processed or stored.
    /// </summary>
    public class InputValidationService
    {
        // Define dangerous HTML/script patterns that could enable XSS attacks
        private static readonly string[] DangerousPatterns = new[]
        {
            @"<script[^>]*>.*?</script>",
            @"javascript:",
            @"on\w+\s*=",
            @"<iframe",
            @"<object",
            @"<embed",
            @"<img[^>]*onerror",
            @"<svg[^>]*onload"
        };

        /// <summary>
        /// Sanitizes user input by removing or escaping potentially harmful content.
        /// Protects against XSS attacks by stripping script tags and event handlers.
        /// </summary>
        /// <param name="input">The user input to sanitize</param>
        /// <returns>Sanitized input safe for processing</returns>
        public string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            string sanitized = input;

            // Remove script tags and dangerous patterns
            foreach (var pattern in DangerousPatterns)
            {
                sanitized = Regex.Replace(sanitized, pattern, string.Empty, RegexOptions.IgnoreCase);
            }

            return sanitized.Trim();
        }

        /// <summary>
        /// Validates and sanitizes username input.
        /// Username should contain only alphanumeric characters, underscores, and hyphens.
        /// </summary>
        /// <param name="username">The username to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            // Username: 3-50 characters, alphanumeric, underscore, hyphen only
            string pattern = @"^[a-zA-Z0-9_-]{3,50}$";
            return Regex.IsMatch(username, pattern);
        }

        /// <summary>
        /// Validates and sanitizes email input using RFC 5322 simplified pattern.
        /// </summary>
        /// <param name="email">The email to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Email validation pattern (simplified RFC 5322)
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }

        /// <summary>
        /// Escapes HTML special characters to prevent XSS when displaying user content.
        /// </summary>
        /// <param name="input">The input to escape</param>
        /// <returns>HTML-escaped string</returns>
        public string HtmlEncode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return System.Net.WebUtility.HtmlEncode(input);
        }

        /// <summary>
        /// Validates input length to prevent buffer overflow and resource exhaustion attacks.
        /// </summary>
        /// <param name="input">The input to validate</param>
        /// <param name="maxLength">Maximum allowed length</param>
        /// <returns>True if input is within acceptable length</returns>
        public bool ValidateLength(string input, int maxLength = 255)
        {
            return input != null && input.Length <= maxLength;
        }

        /// <summary>
        /// Comprehensive validation combining multiple checks.
        /// </summary>
        /// <param name="username">Username to validate</param>
        /// <param name="email">Email to validate</param>
        /// <returns>True if both username and email are valid</returns>
        public bool ValidateUserInput(string username, string email)
        {
            return ValidateUsername(username) && 
                   ValidateEmail(email) && 
                   ValidateLength(username, 50) && 
                   ValidateLength(email, 100);
        }
    }
}
