using NUnit.Framework;
using SafeVault.Services;
using SafeVault.Models;
using System;

namespace SafeVault.Tests
{
    [TestFixture]
    public class TestSecureDataAccess
    {
        private UserService _userService;
        private InputValidationService _validationService;
        
        // Using a test connection string (would normally use a test database)
        private const string TestConnectionString = "Server=.;Database=SafeVaultTest;Trusted_Connection=true;";

        [SetUp]
        public void Setup()
        {
            _validationService = new InputValidationService();
            _userService = new UserService(TestConnectionString);
        }

        // ==================== INPUT VALIDATION BEFORE DATABASE ACCESS ====================

        [Test]
        public void TestCreateUser_ValidatesBeforeQuery()
        {
            // Arrange - SQL injection attempt
            string maliciousUsername = "admin'; DROP TABLE Users; --";
            string validEmail = "test@example.com";

            // Act & Assert
            // The service should throw an exception during validation, before even attempting the query
            var ex = Assert.Throws<ArgumentException>(() => 
                _userService.CreateUser(maliciousUsername, validEmail));
            
            Assert.That(ex.Message, Does.Contain("Invalid"), "Should reject invalid username");
        }

        [Test]
        public void TestCreateUser_ValidatesEmailFormat()
        {
            // Arrange
            string validUsername = "test_user";
            string maliciousEmail = "test@example.com'; DELETE FROM Users; --";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => 
                _userService.CreateUser(validUsername, maliciousEmail));
            
            Assert.That(ex.Message, Does.Contain("Invalid"), "Should reject invalid email");
        }

        // ==================== PARAMETERIZED QUERY TESTS ====================

        [Test]
        public void TestGetUserByUsername_UsesParameterizedQuery()
        {
            // Arrange - Even though this might not actually execute against a test DB,
            // this test documents that the service uses parameterized queries
            string attackUsername = "admin' OR '1'='1' --";

            // Act
            // This should not cause a SQL syntax error or return unexpected results
            // because the username is passed as a parameter, not concatenated into SQL
            User result = _userService.GetUserByUsername(attackUsername);

            // Assert
            // Either no user is found (expected, since no actual user has that username)
            // or an exception is thrown about database connection (also expected in test env)
            // But what WON'T happen is the query being manipulated
            Assert.IsNull(result, "Injection attempt should not modify query logic");
        }

        [Test]
        public void TestGetUserByEmail_TreatsInputAsData()
        {
            // Arrange - Email injection attempt
            string attackEmail = "user@example.com' UNION SELECT username, password FROM admin_users --";

            // Act
            User result = _userService.GetUserByEmail(attackEmail);

            // Assert
            // The email is treated as a literal string, not SQL code
            Assert.IsNull(result, "UNION injection should not work with parameterized query");
        }

        [Test]
        public void TestSearchUsersByUsername_SafeWildcardHandling()
        {
            // Arrange - Wildcard injection attempt
            string attackPattern = "%' OR '1'='1' --";

            // Act
            var results = _userService.SearchUsersByUsername(attackPattern);

            // Assert
            // The pattern is passed as a parameter with wildcards,
            // not as a SQL injection vector
            Assert.IsNotNull(results, "Search should return results (possibly empty)");
            Assert.IsFalse(results.Count > 0 && results[0].Username == "admin", 
                "Wildcard injection should not bypass WHERE clause");
        }

        [Test]
        public void TestUpdateUserEmail_SecureMultiParameter()
        {
            // Arrange - Attack multiple parameters
            int userID = 1;
            string maliciousEmail = "hacker@malicious.com'; UPDATE Users SET Username = 'hacker' WHERE id = '";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => 
                _userService.UpdateUserEmail(userID, maliciousEmail));
            
            Assert.That(ex.Message, Does.Contain("Invalid"), "Invalid email should be rejected");
        }

        [Test]
        public void TestDeleteUser_IntegerParameter()
        {
            // Arrange
            int userID = 999; // Non-existent ID

            // Act
            // Even if someone tries to pass a string like "1; DROP TABLE Users; --",
            // the fact that we expect an int protects us
            bool result = _userService.DeleteUser(userID);

            // Assert
            // Will return false (no user deleted) rather than causing SQL injection
            Assert.IsFalse(result, "Deleting non-existent user should return false");
        }

        // ==================== ATTACK VECTOR TESTS ====================

        [Test]
        public void TestAttackVector_UnionSelect()
        {
            // Arrange
            string attackUsername = "user' UNION SELECT * FROM Users WHERE '1'='1";

            // Act
            bool isValid = _validationService.ValidateUsername(attackUsername);

            // Assert
            Assert.IsFalse(isValid, "UNION-based injection should be caught");
        }

        [Test]
        public void TestAttackVector_SubqueryInjection()
        {
            // Arrange
            string attackUsername = "user' AND (SELECT COUNT(*) FROM Users) > 0 AND '1'='1";

            // Act
            bool isValid = _validationService.ValidateUsername(attackUsername);

            // Assert
            Assert.IsFalse(isValid, "Subquery injection should be caught");
        }

        [Test]
        public void TestAttackVector_FunctionBasedInjection()
        {
            // Arrange
            string attackUsername = "user' AND SLEEP(5) AND '1'='1";

            // Act
            bool isValid = _validationService.ValidateUsername(attackUsername);

            // Assert
            Assert.IsFalse(isValid, "Function-based injection should be caught");
        }

        [Test]
        public void TestAttackVector_DatabaseNameExtraction()
        {
            // Arrange
            string attackUsername = "admin' UNION SELECT database(), user(), version() --";

            // Act
            bool isValid = _validationService.ValidateUsername(attackUsername);

            // Assert
            Assert.IsFalse(isValid, "Information extraction attempt should be caught");
        }

        [Test]
        public void TestAttackVector_TableSchemaExtraction()
        {
            // Arrange
            string attackUsername = "admin' UNION SELECT table_name FROM information_schema.tables --";

            // Act
            bool isValid = _validationService.ValidateUsername(attackUsername);

            // Assert
            Assert.IsFalse(isValid, "Schema extraction attempt should be caught");
        }

        // ==================== BYPASS ATTEMPT TESTS ====================

        [Test]
        public void TestBypassAttempt_URLEncoding()
        {
            // Arrange - URL encoded SQL injection
            string attackUsername = "admin%27%20OR%20%271%27%3D%271";

            // Act
            bool isValid = _validationService.ValidateUsername(attackUsername);

            // Assert
            Assert.IsFalse(isValid, "URL-encoded injection attempt should be caught");
        }

        [Test]
        public void TestBypassAttempt_Doubling()
        {
            // Arrange - Double quote bypass attempt
            string attackUsername = "admin''--";

            // Act
            bool isValid = _validationService.ValidateUsername(attackUsername);

            // Assert
            Assert.IsFalse(isValid, "Double quote bypass attempt should be caught");
        }

        [Test]
        public void TestBypassAttempt_LineContinuation()
        {
            // Arrange
            string attackUsername = "admin' --\nOR '1'='1";

            // Act
            bool isValid = _validationService.ValidateUsername(attackUsername);

            // Assert
            Assert.IsFalse(isValid, "Line continuation bypass should be caught");
        }

        [Test]
        public void TestBypassAttempt_MultilineComment()
        {
            // Arrange
            string attackUsername = "admin' /* */ OR '1'='1";

            // Act
            bool isValid = _validationService.ValidateUsername(attackUsername);

            // Assert
            Assert.IsFalse(isValid, "Multiline comment bypass should be caught");
        }

        // ==================== SAFE DATA ACCESS PATTERNS ====================

        [Test]
        public void TestValidDataAccess_NormalUsername()
        {
            // Arrange
            string normalUsername = "john_doe_2024";

            // Act
            bool isValid = _validationService.ValidateUsername(normalUsername);

            // Assert
            Assert.IsTrue(isValid, "Normal username should be valid");
        }

        [Test]
        public void TestValidDataAccess_NormalEmail()
        {
            // Arrange
            string normalEmail = "john.doe@example.com";

            // Act
            bool isValid = _validationService.ValidateEmail(normalEmail);

            // Assert
            Assert.IsTrue(isValid, "Normal email should be valid");
        }

        [Test]
        public void TestValidDataAccess_MultipleUsers()
        {
            // Arrange - Multiple valid usernames
            string[] validUsernames = 
            {
                "alice_smith",
                "bob-jones",
                "charlie123",
                "user_2024"
            };

            // Act & Assert
            foreach (var username in validUsernames)
            {
                Assert.IsTrue(_validationService.ValidateUsername(username), 
                    $"'{username}' should be valid");
            }
        }

        [Test]
        public void TestValidDataAccess_InternationalCharacters()
        {
            // Arrange - Username with accents (should fail - alphanumeric only)
            string username = "jöhn_döe";

            // Act
            bool isValid = _validationService.ValidateUsername(username);

            // Assert
            Assert.IsFalse(isValid, "International characters should not be allowed in username");
        }

        // ==================== ERROR HANDLING ====================

        [Test]
        public void TestErrorHandling_DatabaseExceptionHandling()
        {
            // Arrange - Invalid connection string
            var userService = new UserService("Invalid=Connection;String=Invalid");

            // Act & Assert
            // Should throw InvalidOperationException, not expose raw SQL errors
            var ex = Assert.Throws<InvalidOperationException>(() => 
                userService.GetUserByUsername("test_user"));
            
            Assert.That(ex.Message, Does.Contain("Database"));
        }

        [Test]
        public void TestErrorHandling_NullInputHandling()
        {
            // Arrange
            string nullUsername = null;

            // Act
            User result = _userService.GetUserByUsername(nullUsername);

            // Assert
            Assert.IsNull(result, "Null input should return null, not crash");
        }
    }
}
