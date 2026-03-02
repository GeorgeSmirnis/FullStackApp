using System;
using System.Security.Cryptography;
using System.Text;

namespace SafeVault.Services
{
    /// <summary>
    /// Secure authentication service handling password hashing and verification.
    /// Uses bcrypt-style hashing (via cryptographic functions) to securely store and verify passwords.
    /// 
    /// Security Features:
    /// - Password hashing using PBKDF2 with SHA-256
    /// - Random salt generation for each password
    /// - Configurable iterations for computational cost
    /// - Constant-time comparison to prevent timing attacks
    /// </summary>
    public class AuthenticationService
    {
        private readonly InputValidationService _validationService;
        
        // Configuration constants
        private const int SaltSize = 32;           // 256 bits
        private const int IterationCount = 100000; // High iteration count for security
        private const int HashSize = 32;            // 256 bits
        private const int MaxFailedAttempts = 5;
        private const int LockoutDurationMinutes = 15;

        public AuthenticationService()
        {
            _validationService = new InputValidationService();
        }

        /// <summary>
        /// Validates credentials and creates a hashed password for storage.
        /// This should be called during user registration.
        /// </summary>
        /// <param name="username">The username to validate and hash</param>
        /// <param name="password">The password to hash (minimum 8 characters)</param>
        /// <returns>Hashed password string ready for database storage</returns>
        /// <exception cref="ArgumentException">If credentials don't meet security requirements</exception>
        public string HashPassword(string username, string password)
        {
            // Validate username format
            if (!_validationService.ValidateUsername(username))
                throw new ArgumentException("Invalid username format");

            // Validate password strength
            ValidatePasswordStrength(password);

            // Generate random salt
            byte[] salt = GenerateSalt();

            // Hash password with salt using PBKDF2
            byte[] hash = HashPasswordWithSalt(password, salt);

            // Combine salt and hash for storage: salt.hash (base64 encoded)
            // Format: {iterations}${platform}${salt}${hash}
            string encodedSalt = Convert.ToBase64String(salt);
            string encodedHash = Convert.ToBase64String(hash);

            return $"{IterationCount}$PBKDF2-SHA256${encodedSalt}${encodedHash}";
        }

        /// <summary>
        /// Verifies a password against a stored hash.
        /// Uses constant-time comparison to prevent timing attacks.
        /// </summary>
        /// <param name="password">The password to verify</param>
        /// <param name="storedHash">The stored hash from the database</param>
        /// <returns>True if password matches the hash</returns>
        public bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                // Parse the stored hash format
                string[] parts = storedHash.Split('$');
                if (parts.Length != 4)
                    return false;

                // Extract components
                int iterations = int.Parse(parts[0]);
                string platform = parts[1];
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] storedHashBytes = Convert.FromBase64String(parts[3]);

                // Verify platform
                if (platform != "PBKDF2-SHA256")
                    return false;

                // Hash the provided password with the same salt
                byte[] computedHash = HashPasswordWithSalt(password, salt, iterations);

                // Constant-time comparison to prevent timing attacks
                return ConstantTimeComparison(computedHash, storedHashBytes);
            }
            catch
            {
                // Return false on any parsing or verification error
                // Don't expose specific error information
                return false;
            }
        }

        /// <summary>
        /// Validates password strength according to security policies.
        /// Requirements:
        /// - Minimum 8 characters
        /// - At least one uppercase letter
        /// - At least one lowercase letter
        /// - At least one digit
        /// - At least one special character
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <exception cref="ArgumentException">If password doesn't meet requirements</exception>
        public void ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty");

            if (password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters long");

            if (password.Length > 128)
                throw new ArgumentException("Password must not exceed 128 characters");

            bool hasUppercase = password.Any(char.IsUpper);
            bool hasLowercase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));

            if (!hasUppercase)
                throw new ArgumentException("Password must contain at least one uppercase letter");

            if (!hasLowercase)
                throw new ArgumentException("Password must contain at least one lowercase letter");

            if (!hasDigit)
                throw new ArgumentException("Password must contain at least one digit");

            if (!hasSpecialChar)
                throw new ArgumentException("Password must contain at least one special character");
        }

        /// <summary>
        /// Generates a new random salt for password hashing.
        /// </summary>
        /// <returns>Random salt bytes</returns>
        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Hashes a password with the provided salt using PBKDF2.
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <param name="salt">The salt to use</param>
        /// <param name="iterations">Number of iterations (uses default if not specified)</param>
        /// <returns>Hashed password bytes</returns>
        private byte[] HashPasswordWithSalt(string password, byte[] salt, int iterations = -1)
        {
            if (iterations < 0)
                iterations = IterationCount;

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(HashSize);
            }
        }

        /// <summary>
        /// Constant-time comparison to prevent timing attacks.
        /// All comparisons take the same amount of time regardless of where the difference occurs.
        /// </summary>
        /// <param name="hash1">First hash to compare</param>
        /// <param name="hash2">Second hash to compare</param>
        /// <returns>True if hashes match</returns>
        private bool ConstantTimeComparison(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length)
                return false;

            int result = 0;
            for (int i = 0; i < hash1.Length; i++)
            {
                result |= hash1[i] ^ hash2[i];
            }

            return result == 0;
        }

        /// <summary>
        /// Generates a password reset token. In production, this should be cryptographically secure
        /// and time-limited. This is a simplified version for demonstration.
        /// </summary>
        /// <returns>Reset token string</returns>
        public string GeneratePasswordResetToken()
        {
            byte[] tokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            return Convert.ToBase64String(tokenBytes);
        }

        /// <summary>
        /// Checks if a user account should be locked due to too many failed login attempts.
        /// </summary>
        /// <param name="failedAttempts">Number of failed login attempts</param>
        /// <param name="lastFailedAttemptTime">Time of last failed attempt</param>
        /// <returns>True if account should be locked</returns>
        public bool ShouldLockAccount(int failedAttempts, DateTime lastFailedAttemptTime)
        {
            if (failedAttempts < MaxFailedAttempts)
                return false;

            // Check if lockout period has expired
            var lockoutExpiration = lastFailedAttemptTime.AddMinutes(LockoutDurationMinutes);
            return DateTime.UtcNow < lockoutExpiration;
        }

        /// <summary>
        /// Gets the lockout duration in minutes.
        /// </summary>
        public int GetLockoutDurationMinutes()
        {
            return LockoutDurationMinutes;
        }

        /// <summary>
        /// Gets the maximum failed attempts before lockout.
        /// </summary>
        public int GetMaxFailedAttempts()
        {
            return MaxFailedAttempts;
        }
    }
}
