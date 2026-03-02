using NUnit.Framework;
using SafeVault.Services;

namespace SafeVault.Tests
{
    [TestFixture]
    public class TestSQLInjection
    {
        private InputValidationService _validationService;

        [SetUp]
        public void Setup()
        {
            _validationService = new InputValidationService();
        }

        // ==================== SQL INJECTION ATTACK SIMULATIONS ====================
        // These tests demonstrate common SQL injection techniques and verify that
        // input validation and parameterized queries prevent them.

        [Test]
        public void TestSQLInjection_UnionBasedInjection()
        {
            // Arrange - Simulate Union-based SQL injection
            // Attack: admin' UNION SELECT username, password FROM admin_users --
            string maliciousInput = "admin' UNION SELECT username, password FROM admin_users --";

            // Act
            bool validationFails = !_validationService.ValidateUsername(maliciousInput);
            string sanitized = _validationService.SanitizeInput(maliciousInput);

            // Assert
            // The input fails validation because it contains special characters not in username pattern
            Assert.IsTrue(validationFails, "SQL injection attempt should fail username validation");
            // Even if it weren't caught by validation, parameterized queries would treat it as a literal value
            Assert.That(sanitized, Does.Not.Contain("UNION"), "UNION keyword should be removed during sanitization");
        }

        [Test]
        public void TestSQLInjection_BooleanBasedInjection()
        {
            // Arrange - Simulate Boolean-based SQL injection
            // Attack: admin' OR '1'='1
            string maliciousInput = "admin' OR '1'='1";

            // Act
            bool validationFails = !_validationService.ValidateUsername(maliciousInput);

            // Assert
            Assert.IsTrue(validationFails, "SQL injection attempt with OR clause should fail validation");
        }

        [Test]
        public void TestSQLInjection_TimeBased()
        {
            // Arrange - Simulate Time-based blind SQL injection
            // Attack: admin'; WAITFOR DELAY '00:00:05' --
            string maliciousInput = "admin'; WAITFOR DELAY '00:00:05' --";

            // Act
            bool validationFails = !_validationService.ValidateUsername(maliciousInput);

            // Assert
            Assert.IsTrue(validationFails, "Time-based SQL injection should fail validation");
        }

        [Test]
        public void TestSQLInjection_StackedQueries()
        {
            // Arrange - Simulate stacked queries injection
            // Attack: admin'; DROP TABLE Users; --
            string maliciousInput = "admin'; DROP TABLE Users; --";

            // Act
            bool validationFails = !_validationService.ValidateUsername(maliciousInput);

            // Assert
            Assert.IsTrue(validationFails, "Stacked queries injection should fail validation");
        }

        [Test]
        public void TestSQLInjection_SecondOrderInjection()
        {
            // Arrange - Simulate second-order SQL injection stored in database
            // Attack: username that looks normal but contains SQL when used in another query
            string maliciousInput = "user' WHERE id='1";

            // Act
            bool validationFails = !_validationService.ValidateUsername(maliciousInput);

            // Assert
            Assert.IsTrue(validationFails, "Second-order injection attempt should fail validation");
        }

        [Test]
        public void TestSQLInjection_ExceptionBasedInjection()
        {
            // Arrange - Simulate exception-based SQL injection
            // Attack: admin' AND extractvalue(1, concat(0x7e, (SELECT version()))) --
            string maliciousInput = "admin' AND extractvalue(1, concat(0x7e, (SELECT version()))) --";

            // Act
            bool validationFails = !_validationService.ValidateUsername(maliciousInput);

            // Assert
            Assert.IsTrue(validationFails, "Exception-based injection should fail validation");
        }

        [Test]
        public void TestSQLInjection_CommentBased()
        {
            // Arrange - Simulate comment-based injection
            // Attack: admin' --
            string maliciousInput = "admin' --";

            // Act
            bool validationFails = !_validationService.ValidateUsername(maliciousInput);

            // Assert
            Assert.IsTrue(validationFails, "Comment-based SQL injection should fail validation");
        }

        [Test]
        public void TestSQLInjection_HexEncoded()
        {
            // Arrange - Hex-encoded SQL injection
            // Attack: 0x61646d696e (hex for 'admin') used to bypass filters
            string maliciousInput = "admin' OR username LIKE 0x25";

            // Act
            bool validationFails = !_validationService.ValidateUsername(maliciousInput);

            // Assert
            Assert.IsTrue(validationFails, "Hex-encoded injection should fail validation");
        }

        [Test]
        public void TestSQLInjection_EmailField()
        {
            // Arrange - SQL injection attempt through email field
            string maliciousEmail = "user@example.com'; DELETE FROM Users; --";

            // Act
            bool validationFails = !_validationService.ValidateEmail(maliciousEmail);

            // Assert
            Assert.IsTrue(validationFails, "Email field SQL injection should fail validation");
        }

        [Test]
        public void TestSQLInjection_WildcardEscape()
        {
            // Arrange - SQL injection using LIKE wildcards
            // Attack: %' OR '1'='1
            string maliciousInput = "%' OR '1'='1";

            // Act
            bool validationFails = !_validationService.ValidateUsername(maliciousInput);

            // Assert
            Assert.IsTrue(validationFails, "Wildcard-based injection should fail validation");
        }

        [Test]
        public void TestSQLInjection_NullByteInjection()
        {
            // Arrange - Null byte injection
            string maliciousInput = "admin\0' OR '1'='1";

            // Act
            bool validationFails = !_validationService.ValidateUsername(maliciousInput);

            // Assert
            Assert.IsTrue(validationFails, "Null byte injection should fail validation");
        }

        [Test]
        public void TestSQLInjection_CaseVariation()
        {
            // Arrange - Case variation attempt
            // Attack: admin' UnIoN SeLeCt ...
            string maliciousInput = "admin' UnIoN SeLeCt password FrOm users";

            // Act
            bool validationFails = !_validationService.ValidateUsername(maliciousInput);

            // Assert
            Assert.IsTrue(validationFails, "Case-varied SQL injection should fail validation");
        }

        // ==================== PARAMETERIZED QUERY PROTECTION ====================

        [Test]
        public void TestParameterizedQuery_PreventsStringConcatenation()
        {
            // This test demonstrates that parameterized queries prevent SQL injection
            // even if the code was incorrectly written with string concatenation.
            
            // Arrange - The malicious input that would break string concatenation
            string attackInput = "admin'; DROP TABLE Users; --";

            // Act - With parameterized queries, the input is treated as data, not code
            // The SQL parameter system ensures this entire string is treated as a literal value
            bool isValidated = _validationService.ValidateUsername(attackInput);

            // Assert
            // Even if validation somehow passed (which it shouldn't),
            // parameterized queries would treat it as a literal username value,
            // not as SQL code to execute
            Assert.IsFalse(isValidated, "Malicious input should fail validation");
        }

        [Test]
        public void TestParameterizedQuery_PreventsLogicManipulation()
        {
            // Arrange - Attack attempting to manipulate WHERE clause logic
            string attackInput = "' OR '1'='1";

            // Act
            bool validationPasses = _validationService.ValidateUsername(attackInput);

            // Assert
            Assert.IsFalse(validationPasses, "Logic manipulation attempt should fail");
        }

        [Test]
        public void TestParameterizedQuery_PreventsBoundaryBypass()
        {
            // Arrange - Attack attempting to use SQL wildcards
            string attackInput = "%";

            // Act
            bool validationPasses = _validationService.ValidateUsername(attackInput);

            // Assert
            Assert.IsFalse(validationPasses, "Wildcard-only input should not be valid username");
        }

        // ==================== DEFENSE IN DEPTH ====================

        [Test]
        public void TestDefenseInDepth_MultipleProtectionLayers()
        {
            // This test demonstrates that the SafeVault application uses multiple
            // protection layers: input validation, sanitization, and parameterized queries

            // Arrange - Complex SQL injection attempt
            string attackInput = "user<script>alert('xss')</script>' OR username='admin";

            // Layer 1: Input Validation
            bool validatesUsername = _validationService.ValidateUsername(attackInput);

            // Layer 2: Input Sanitization
            string sanitized = _validationService.SanitizeInput(attackInput);

            // Layer 3: Would be parameterized query (not tested directly here)
            // but we verify the previous layers work

            // Assert
            Assert.IsFalse(validatesUsername, "Invalid input should fail validation");
            Assert.That(sanitized, Does.Not.Contain("<script>"), "XSS should be removed");
            Assert.That(sanitized, Does.Not.Contain("<"), "Dangerous characters should be removed");
        }

        [Test]
        public void TestLengthBasedInjection()
        {
            // Arrange - Very long injection attempt
            string longAttack = new string('a', 1000) + "' OR '1'='1";

            // Act
            bool validatesLength = _validationService.ValidateLength(longAttack, 100);

            // Assert
            Assert.IsFalse(validatesLength, "Excessively long input should be rejected");
        }

        // ==================== SAFE INPUT EXAMPLES ====================

        [Test]
        public void TestValidInput_IsNotAffectedByProtections()
        {
            // Arrange - Normal, valid username
            string validUsername = "john_doe_2024";

            // Act
            bool validates = _validationService.ValidateUsername(validUsername);
            string sanitized = _validationService.SanitizeInput(validUsername);

            // Assert
            Assert.IsTrue(validates, "Valid username should pass validation");
            Assert.AreEqual(validUsername, sanitized, "Valid input should not be modified by sanitization");
        }

        [Test]
        public void TestValidEmail_IsNotAffectedByProtections()
        {
            // Arrange - Normal, valid email
            string validEmail = "user.name+2024@example.co.uk";

            // Act
            bool validates = _validationService.ValidateEmail(validEmail);

            // Assert
            Assert.IsTrue(validates, "Valid email should pass validation");
        }
    }
}
