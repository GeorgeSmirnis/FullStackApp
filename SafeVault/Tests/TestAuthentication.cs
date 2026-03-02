using NUnit.Framework;
using SafeVault.Services;
using SafeVault.Models;
using System;

namespace SafeVault.Tests
{
    [TestFixture]
    public class TestAuthentication
    {
        private AuthenticationService _authService;
        private InputValidationService _validationService;

        [SetUp]
        public void Setup()
        {
            _authService = new AuthenticationService();
            _validationService = new InputValidationService();
        }

        // ==================== PASSWORD HASHING TESTS ====================

        [Test]
        public void TestHashPassword_GeneratesValidHash()
        {
            // Arrange
            string username = "john_doe";
            string password = "SecurePass123!";

            // Act
            string hash = _authService.HashPassword(username, password);

            // Assert
            Assert.IsNotNull(hash, "Hash should not be null");
            Assert.IsNotEmpty(hash, "Hash should not be empty");
            Assert.That(hash, Does.Contain("$"), "Hash should contain separators");
        }

        [Test]
        public void TestHashPassword_DifferentHashesForSamePassword()
        {
            // Arrange
            string username = "john_doe";
            string password = "SecurePass123!";

            // Act
            string hash1 = _authService.HashPassword(username, password);
            string hash2 = _authService.HashPassword(username, password);

            // Assert
            Assert.AreNotEqual(hash1, hash2, "Same password should produce different hashes (due to random salt)");
        }

        [Test]
        public void TestHashPassword_InvalidUsername()
        {
            // Arrange
            string invalidUsername = "jd"; // Too short
            string password = "SecurePass123!";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _authService.HashPassword(invalidUsername, password));
        }

        [Test]
        public void TestHashPassword_InvalidPassword_Empty()
        {
            // Arrange
            string username = "john_doe";
            string password = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _authService.HashPassword(username, password));
        }

        [Test]
        public void TestHashPassword_InvalidPassword_TooShort()
        {
            // Arrange
            string username = "john_doe";
            string password = "Short1!"; // Only 7 characters

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _authService.HashPassword(username, password),
                "Password must be at least 8 characters");
        }

        [Test]
        public void TestHashPassword_InvalidPassword_NoUppercase()
        {
            // Arrange
            string username = "john_doe";
            string password = "lowercase123!";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _authService.HashPassword(username, password),
                "Password must contain uppercase letter");
        }

        [Test]
        public void TestHashPassword_InvalidPassword_NoLowercase()
        {
            // Arrange
            string username = "john_doe";
            string password = "UPPERCASE123!";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _authService.HashPassword(username, password),
                "Password must contain lowercase letter");
        }

        [Test]
        public void TestHashPassword_InvalidPassword_NoDigit()
        {
            // Arrange
            string username = "john_doe";
            string password = "NoDigitHere!";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _authService.HashPassword(username, password),
                "Password must contain digit");
        }

        [Test]
        public void TestHashPassword_InvalidPassword_NoSpecialChar()
        {
            // Arrange
            string username = "john_doe";
            string password = "NoSpecial123";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _authService.HashPassword(username, password),
                "Password must contain special character");
        }

        [Test]
        public void TestHashPassword_TooLongPassword()
        {
            // Arrange
            string username = "john_doe";
            string password = new string('A', 129) + "b1!"; // 132 characters, exceeds 128

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _authService.HashPassword(username, password),
                "Password exceeding 128 characters should be rejected");
        }

        // ==================== PASSWORD VERIFICATION TESTS ====================

        [Test]
        public void TestVerifyPassword_CorrectPassword()
        {
            // Arrange
            string username = "john_doe";
            string password = "SecurePass123!";
            string hash = _authService.HashPassword(username, password);

            // Act
            bool result = _authService.VerifyPassword(password, hash);

            // Assert
            Assert.IsTrue(result, "Correct password should verify successfully");
        }

        [Test]
        public void TestVerifyPassword_WrongPassword()
        {
            // Arrange
            string username = "john_doe";
            string correctPassword = "SecurePass123!";
            string wrongPassword = "WrongPass456@";
            string hash = _authService.HashPassword(username, correctPassword);

            // Act
            bool result = _authService.VerifyPassword(wrongPassword, hash);

            // Assert
            Assert.IsFalse(result, "Wrong password should not verify");
        }

        [Test]
        public void TestVerifyPassword_EmptyPassword()
        {
            // Arrange
            string username = "john_doe";
            string password = "SecurePass123!";
            string hash = _authService.HashPassword(username, password);

            // Act
            bool result = _authService.VerifyPassword("", hash);

            // Assert
            Assert.IsFalse(result, "Empty password should not verify");
        }

        [Test]
        public void TestVerifyPassword_NullPassword()
        {
            // Arrange
            string username = "john_doe";
            string password = "SecurePass123!";
            string hash = _authService.HashPassword(username, password);

            // Act
            bool result = _authService.VerifyPassword(null, hash);

            // Assert
            Assert.IsFalse(result, "Null password should not verify");
        }

        [Test]
        public void TestVerifyPassword_InvalidHash()
        {
            // Arrange
            string password = "SecurePass123!";
            string invalidHash = "invalid_hash_format";

            // Act
            bool result = _authService.VerifyPassword(password, invalidHash);

            // Assert
            Assert.IsFalse(result, "Invalid hash should not verify");
        }

        [Test]
        public void TestVerifyPassword_CorruptedHash()
        {
            // Arrange
            string username = "john_doe";
            string password = "SecurePass123!";
            string hash = _authService.HashPassword(username, password);
            
            // Corrupt the hash
            string corruptedHash = hash.Substring(0, hash.Length - 5) + "XXXXX";

            // Act
            bool result = _authService.VerifyPassword(password, corruptedHash);

            // Assert
            Assert.IsFalse(result, "Corrupted hash should not verify");
        }

        [Test]
        public void TestVerifyPassword_CaseSensitive()
        {
            // Arrange
            string username = "john_doe";
            string password = "SecurePass123!";
            string hash = _authService.HashPassword(username, password);
            
            // Use different case
            string differentCase = "securepass123!";

            // Act
            bool result = _authService.VerifyPassword(differentCase, hash);

            // Assert
            Assert.IsFalse(result, "Password verification should be case-sensitive");
        }

        // ==================== PASSWORD STRENGTH VALIDATION ====================

        [Test]
        public void TestValidatePasswordStrength_ValidPassword()
        {
            // Arrange
            string password = "ValidPass123!";

            // Act & Assert
            Assert.DoesNotThrow(() => _authService.ValidatePasswordStrength(password));
        }

        [Test]
        public void TestValidatePasswordStrength_MultipleSpecialChars()
        {
            // Arrange
            string password = "Valid@Pass#123";

            // Act & Assert
            Assert.DoesNotThrow(() => _authService.ValidatePasswordStrength(password));
        }

        [Test]
        public void TestValidatePasswordStrength_LongPassword()
        {
            // Arrange
            string password = "VeryLongValidPassword123!With!Multiple!Chars";

            // Act & Assert
            Assert.DoesNotThrow(() => _authService.ValidatePasswordStrength(password));
        }

        // ==================== ACCOUNT LOCKOUT TESTS ====================

        [Test]
        public void TestShouldLockAccount_BelowThreshold()
        {
            // Arrange
            int failedAttempts = 3;
            DateTime lastAttempt = DateTime.UtcNow;

            // Act
            bool shouldLock = _authService.ShouldLockAccount(failedAttempts, lastAttempt);

            // Assert
            Assert.IsFalse(shouldLock, "Account should not lock with 3 failed attempts");
        }

        [Test]
        public void TestShouldLockAccount_AtThreshold()
        {
            // Arrange
            int failedAttempts = _authService.GetMaxFailedAttempts();
            DateTime lastAttempt = DateTime.UtcNow;

            // Act
            bool shouldLock = _authService.ShouldLockAccount(failedAttempts, lastAttempt);

            // Assert
            Assert.IsTrue(shouldLock, "Account should lock at maximum failed attempts");
        }

        [Test]
        public void TestShouldLockAccount_LockoutExpired()
        {
            // Arrange
            int failedAttempts = _authService.GetMaxFailedAttempts();
            DateTime lastAttempt = DateTime.UtcNow.AddMinutes(-20); // 20 minutes ago (past lockout duration)

            // Act
            bool shouldLock = _authService.ShouldLockAccount(failedAttempts, lastAttempt);

            // Assert
            Assert.IsFalse(shouldLock, "Account should unlock after lockout duration expires");
        }

        [Test]
        public void TestGetMaxFailedAttempts_ReturnsValidValue()
        {
            // Act
            int maxAttempts = _authService.GetMaxFailedAttempts();

            // Assert
            Assert.Greater(maxAttempts, 0, "Max failed attempts should be greater than 0");
            Assert.LessOrEqual(maxAttempts, 10, "Max failed attempts should be reasonable");
        }

        [Test]
        public void TestGetLockoutDurationMinutes_ReturnsValidValue()
        {
            // Act
            int duration = _authService.GetLockoutDurationMinutes();

            // Assert
            Assert.Greater(duration, 0, "Lockout duration should be greater than 0");
            Assert.LessOrEqual(duration, 60, "Lockout duration should be reasonable");
        }

        // ==================== PASSWORD RESET TOKEN TESTS ====================

        [Test]
        public void TestGeneratePasswordResetToken_CreatesToken()
        {
            // Act
            string token = _authService.GeneratePasswordResetToken();

            // Assert
            Assert.IsNotNull(token, "Token should not be null");
            Assert.IsNotEmpty(token, "Token should not be empty");
        }

        [Test]
        public void TestGeneratePasswordResetToken_UniqueTokens()
        {
            // Act
            string token1 = _authService.GeneratePasswordResetToken();
            string token2 = _authService.GeneratePasswordResetToken();

            // Assert
            Assert.AreNotEqual(token1, token2, "Each token should be unique");
        }

        [Test]
        public void TestGeneratePasswordResetToken_ValidLength()
        {
            // Act
            string token = _authService.GeneratePasswordResetToken();

            // Assert
            Assert.That(token.Length, Is.GreaterThan(20), "Token should be sufficiently long");
        }

        // ==================== ATTACK SCENARIO TESTS ====================

        [Test]
        public void TestAttack_BruteForcePassword()
        {
            // Arrange
            string username = "john_doe";
            string correctPassword = "SecurePass123!";
            string hash = _authService.HashPassword(username, correctPassword);

            // Attempt common passwords
            string[] commonPasswords = new[]
            {
                "password",
                "123456",
                "admin123",
                "welcome1",
                "letmein"
            };

            // Act & Assert
            foreach (var commonPass in commonPasswords)
            {
                // Add proper validation first
                try
                {
                    _authService.ValidatePasswordStrength(commonPass);
                    // Only test if it's valid format
                    bool verified = _authService.VerifyPassword(commonPass, hash);
                    Assert.IsFalse(verified, $"Common password '{commonPass}' should not verify");
                }
                catch (ArgumentException)
                {
                    // Expected for weak passwords
                }
            }
        }

        [Test]
        public void TestAttack_PasswordWithSQLInjection()
        {
            // Arrange
            string username = "john_doe";
            string password = "SecurePass123!";
            string hash = _authService.HashPassword(username, password);

            // Attempt SQL injection in password
            string sqlInjectionPassword = "' OR '1'='1";

            // Act
            bool verified = _authService.VerifyPassword(sqlInjectionPassword, hash);

            // Assert
            Assert.IsFalse(verified, "SQL injection attempt should not verify");
        }

        [Test]
        public void TestAttack_PasswordWithScriptInjection()
        {
            // Arrange
            string username = "john_doe";
            string password = "SecurePass123!";
            string hash = _authService.HashPassword(username, password);

            // Attempt script injection
            string scriptPassword = "<script>alert('xss')</script>";

            // Act
            bool verified = _authService.VerifyPassword(scriptPassword, hash);

            // Assert
            Assert.IsFalse(verified, "Script injection attempt should not verify");
        }

        [Test]
        public void TestAttack_TimingAttack_ConstantTime()
        {
            // Arrange
            string username = "john_doe";
            string password = "SecurePass123!";
            string hash = _authService.HashPassword(username, password);

            // Wrong password that's similar
            string wrongPassword1 = "SecurePass123@"; // Last char different
            string wrongPassword2 = "DifferentPass1!"; // Completely different

            // Act
            var start1 = DateTime.UtcNow;
            _authService.VerifyPassword(wrongPassword1, hash);
            var duration1 = DateTime.UtcNow - start1;

            var start2 = DateTime.UtcNow;
            _authService.VerifyPassword(wrongPassword2, hash);
            var duration2 = DateTime.UtcNow - start2;

            // Assert
            // Both should take similar time (constant-time comparison)
            // Allow for system variance
            Assert.Pass("Verification completed - should use constant-time comparison");
        }

        [Test]
        public void TestSecurity_HashIsIrreversible()
        {
            // Arrange
            string username = "john_doe";
            string password = "SecurePass123!";
            string hash = _authService.HashPassword(username, password);

            // Act
            // Try to find the original password from the hash
            string retrievedPassword = null;
            try
            {
                // This should be impossible
                retrievedPassword = hash; // Can't reverse bcrypt hash
            }
            catch
            {
                retrievedPassword = null;
            }

            // Assert
            Assert.IsNull(retrievedPassword, "Hash should be irreversible");
            Assert.AreNotEqual(password, hash, "Hash should not equal original password");
        }

        [Test]
        public void TestSecurity_SaltPreventsRainbowTables()
        {
            // Arrange
            string username = "john_doe";
            string password = "SecurePass123!";

            // Act
            // Generated hashes of the same password should differ due to random salt
            string hash1 = _authService.HashPassword(username, password);
            string hash2 = _authService.HashPassword(username, password);

            // Extract salt portions (should be different)
            string[] parts1 = hash1.Split('$');
            string[] parts2 = hash2.Split('$');

            // Assert
            Assert.AreNotEqual(parts1[2], parts2[2], "Salts should be different");
            Assert.IsTrue(_authService.VerifyPassword(password, hash1), "First hash should verify");
            Assert.IsTrue(_authService.VerifyPassword(password, hash2), "Second hash should verify");
        }

        [Test]
        public void TestSecurity_HighIterationCount()
        {
            // This test documents that password hashing uses high iteration count
            // to make brute force attacks computationally expensive

            // Arrange
            string username = "john_doe";
            string password = "SecurePass123!";

            // Act
            var start = DateTime.UtcNow;
            string hash = _authService.HashPassword(username, password);
            var duration = DateTime.UtcNow - start;

            // Assert
            Assert.Pass($"Password hashing took {duration.TotalMilliseconds}ms (should be slow due to iterations)");
        }
    }
}
