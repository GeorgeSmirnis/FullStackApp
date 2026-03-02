using NUnit.Framework;
using SafeVault.Services;
using SafeVault.Models;
using System;
using System.Collections.Generic;

namespace SafeVault.Tests
{
    [TestFixture]
    public class TestJwtTokens
    {
        private JwtTokenService _tokenService;
        private AuthorizationService _authService;
        private User _testUser;

        [SetUp]
        public void Setup()
        {
            // Create token service with test secret key (minimum 32 chars)
            _tokenService = new JwtTokenService("MySecretKeyThatIsAtLeast32Characters!", expirationMinutes: 60);
            _authService = new AuthorizationService();

            // Create test user with admin role
            _testUser = new User("test_user", "test@example.com")
            {
                UserID = 1,
                IsActive = true,
                Roles = new List<Role>()
            };
            _authService.AssignRoleToUser(_testUser, AuthorizationService.Roles.Admin);
        }

        // ==================== TOKEN CREATION TESTS ====================

        [Test]
        public void TestCreateToken_GeneratesValidToken()
        {
            // Act
            string token = _tokenService.CreateToken(_testUser);

            // Assert
            Assert.IsNotNull(token, "Token should not be null");
            Assert.IsNotEmpty(token, "Token should not be empty");
            Assert.That(token.Split('.').Length, Is.EqualTo(3), "JWT should have 3 parts (header.payload.signature)");
        }

        [Test]
        public void TestCreateToken_TokenCanBeVerified()
        {
            // Act
            string token = _tokenService.CreateToken(_testUser);
            bool isValid = _tokenService.ValidateToken(token, out var claims);

            // Assert
            Assert.IsTrue(isValid, "Created token should be valid");
            Assert.IsNotNull(claims, "Claims should be extracted");
        }

        [Test]
        public void TestCreateToken_TokenContainsCorrectClaims()
        {
            // Act
            string token = _tokenService.CreateToken(_testUser);
            _tokenService.ValidateToken(token, out var claims);

            // Assert
            Assert.AreEqual(_testUser.UserID, claims.UserID, "Token should contain correct user ID");
            Assert.AreEqual(_testUser.Username, claims.Username, "Token should contain correct username");
            Assert.AreEqual(_testUser.Email, claims.Email, "Token should contain correct email");
            Assert.Greater(claims.Roles.Count, 0, "Token should contain roles");
        }

        [Test]
        public void TestCreateToken_TokenIncludesRoles()
        {
            // Act
            string token = _tokenService.CreateToken(_testUser);
            _tokenService.ValidateToken(token, out var claims);

            // Assert
            Assert.Contains(AuthorizationService.Roles.Admin, claims.Roles, "Token should include admin role");
        }

        [Test]
        public void TestCreateToken_TokenIncludesPermissions()
        {
            // Act
            string token = _tokenService.CreateToken(_testUser);
            _tokenService.ValidateToken(token, out var claims);

            // Assert
            Assert.Greater(claims.Permissions.Count, 0, "Token should include permissions");
            Assert.Contains(AuthorizationService.Permissions.CreateUser, claims.Permissions, "Token should include appropriate permissions");
        }

        [Test]
        public void TestCreateToken_NullUser()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _tokenService.CreateToken(null));
        }

        // ==================== TOKEN VALIDATION TESTS ====================

        [Test]
        public void TestValidateToken_ValidToken()
        {
            // Arrange
            string token = _tokenService.CreateToken(_testUser);

            // Act
            bool isValid = _tokenService.ValidateToken(token, out var claims);

            // Assert
            Assert.IsTrue(isValid, "Valid token should pass validation");
            Assert.IsNotNull(claims, "Claims should be extracted from valid token");
        }

        [Test]
        public void TestValidateToken_InvalidToken()
        {
            // Arrange
            string invalidToken = "invalid.token.here";

            // Act
            bool isValid = _tokenService.ValidateToken(invalidToken, out var claims);

            // Assert
            Assert.IsFalse(isValid, "Invalid token should fail validation");
            Assert.IsNull(claims, "Claims should be null for invalid token");
        }

        [Test]
        public void TestValidateToken_TamperedToken()
        {
            // Arrange
            string token = _tokenService.CreateToken(_testUser);
            // Tamper with token by changing a character in the payload
            string tamperedToken = token.Substring(0, token.Length - 5) + "XXXXX";

            // Act
            bool isValid = _tokenService.ValidateToken(tamperedToken, out var claims);

            // Assert
            Assert.IsFalse(isValid, "Tampered token should fail validation");
        }

        [Test]
        public void TestValidateToken_EmptyToken()
        {
            // Act
            bool isValid = _tokenService.ValidateToken("", out var claims);

            // Assert
            Assert.IsFalse(isValid, "Empty token should fail validation");
        }

        [Test]
        public void TestValidateToken_NullToken()
        {
            // Act
            bool isValid = _tokenService.ValidateToken(null, out var claims);

            // Assert
            Assert.IsFalse(isValid, "Null token should fail validation");
        }

        // ==================== TOKEN EXPIRATION TESTS ====================

        [Test]
        public void TestIsTokenExpired_ValidToken()
        {
            // Arrange
            string token = _tokenService.CreateToken(_testUser);

            // Act
            bool isExpired = _tokenService.IsTokenExpired(token);

            // Assert
            Assert.IsFalse(isExpired, "Freshly created token should not be expired");
        }

        [Test]
        public void TestIsTokenExpired_ExpiredToken()
        {
            // Arrange - Create token service with very short expiration
            var shortLivedTokenService = new JwtTokenService(
                "MySecretKeyThatIsAtLeast32Characters!", 
                expirationMinutes: -1); // Already expired
            
            string token = shortLivedTokenService.CreateToken(_testUser);

            // Act
            bool isExpired = _tokenService.IsTokenExpired(token);

            // Assert
            Assert.IsTrue(isExpired, "Expired token should be identified as expired");
        }

        [Test]
        public void TestCreateToken_HasExpiration()
        {
            // Act
            string token = _tokenService.CreateToken(_testUser);
            _tokenService.ValidateToken(token, out var claims);

            // Assert
            Assert.Greater(claims.ExpiresAt, DateTime.UtcNow, "Token expiration should be in the future");
            Assert.Greater(claims.ExpiresAt, claims.IssuedAt, "Expiration should be after issuance");
        }

        // ==================== REFRESH TOKEN TESTS ====================

        [Test]
        public void TestCreateRefreshToken_GeneratesValidRefreshToken()
        {
            // Act
            string refreshToken = _tokenService.CreateRefreshToken(_testUser);

            // Assert
            Assert.IsNotNull(refreshToken, "Refresh token should not be null");
            Assert.IsNotEmpty(refreshToken, "Refresh token should not be empty");
        }

        [Test]
        public void TestCreateRefreshToken_HasLongerExpiration()
        {
            // Act
            string accessToken = _tokenService.CreateToken(_testUser);
            string refreshToken = _tokenService.CreateRefreshToken(_testUser);

            _tokenService.ValidateToken(accessToken, out var accessClaims);
            _tokenService.ValidateToken(refreshToken, out var refreshClaims);

            // Assert
            Assert.Greater(refreshClaims.ExpiresAt, accessClaims.ExpiresAt, "Refresh token should have longer expiration");
        }

        [Test]
        public void TestCreateRefreshToken_MarkedAsRefreshToken()
        {
            // Act
            string refreshToken = _tokenService.CreateRefreshToken(_testUser);
            _tokenService.ValidateToken(refreshToken, out var claims);

            // Assert
            Assert.Contains("RefreshToken", claims.Roles, "Refresh token should have RefreshToken role");
        }

        // ==================== TOKEN SIGNATURE VERIFICATION TESTS ====================

        [Test]
        public void TestTokenSignature_InvalidSecretKey()
        {
            // Arrange
            string token = _tokenService.CreateToken(_testUser);
            
            // Create service with different secret key
            var wrongTokenService = new JwtTokenService("DifferentSecretKeyThatIsAtLeast32Chars!");

            // Act
            bool isValid = wrongTokenService.ValidateToken(token, out var claims);

            // Assert
            Assert.IsFalse(isValid, "Token should fail validation with wrong secret key");
        }

        [Test]
        public void TestTokenSignature_ModifiedPayload()
        {
            // Arrange
            string token = _tokenService.CreateToken(_testUser);
            var parts = token.Split('.');
            
            // Modify the payload (middle part)
            string header = parts[0];
            string modifiedPayload = parts[1].Substring(0, parts[1].Length - 1) + "X";
            string signature = parts[2];
            string modifiedToken = $"{header}.{modifiedPayload}.{signature}";

            // Act
            bool isValid = _tokenService.ValidateToken(modifiedToken, out var claims);

            // Assert
            Assert.IsFalse(isValid, "Token should fail validation when payload is modified");
        }

        [Test]
        public void TestTokenSignature_ValidSignature()
        {
            // Arrange
            string token = _tokenService.CreateToken(_testUser);

            // Act
            bool isValid = _tokenService.ValidateToken(token, out var claims);

            // Assert
            Assert.IsTrue(isValid, "Token with valid signature should pass validation");
        }

        // ==================== ATTACK SCENARIO TESTS ====================

        [Test]
        public void TestAttack_TokenForged()
        {
            // Arrange - Attacker tries to create a fake token without valid secret
            string attackerSecretKey = "DifferentSecretKeyThatIsAtLeast32Key";
            var attackerTokenService = new JwtTokenService(attackerSecretKey);
            
            var fakeUser = new User("attacker", "attacker@example.com") { UserID = 999, Roles = new List<Role>() };
            _authService.AssignRoleToUser(fakeUser, AuthorizationService.Roles.Admin);
            
            // Act
            string forgedToken = attackerTokenService.CreateToken(fakeUser);
            bool isValid = _tokenService.ValidateToken(forgedToken, out var claims);

            // Assert
            Assert.IsFalse(isValid, "Forged token with different secret should fail validation");
        }

        [Test]
        public void TestAttack_TokenReplay()
        {
            // Arrange
            string token = _tokenService.CreateToken(_testUser);

            // Act
            // Token is valid multiple times (replay attack potential)
            bool isValid1 = _tokenService.ValidateToken(token, out var claims1);
            bool isValid2 = _tokenService.ValidateToken(token, out var claims2);

            // Assert
            Assert.IsTrue(isValid1, "Token should be valid on first validation");
            Assert.IsTrue(isValid2, "Token should be valid on second validation");
            // In production, implement token blacklist/revocation to prevent replay
        }

        [Test]
        public void TestAttack_TokenWithModifiedClaims()
        {
            // Arrange
            string token = _tokenService.CreateToken(_testUser);
            var parts = token.Split('.');

            // Try to modify the user ID in the payload manually
            // This demonstrates why tokens must be signed and verified
            string header = parts[0];
            string modifiedPayload = parts[1].Replace("1", "999"); // Try to change user ID
            string signature = parts[2];
            string modifiedToken = $"{header}.{modifiedPayload}.{signature}";

            // Act
            bool isValid = _tokenService.ValidateToken(modifiedToken, out var claims);

            // Assert
            Assert.IsFalse(isValid, "Token with modified claims should fail signature verification");
        }

        [Test]
        public void TestSecurity_TokenIsStateless()
        {
            // Arrange
            string token = _tokenService.CreateToken(_testUser);

            // Act
            _tokenService.ValidateToken(token, out var claimsAfter);

            // Assert
            // Token should contain all necessary information without server-side state
            Assert.AreEqual(_testUser.UserID, claimsAfter.UserID, "Token should be stateless and contain user info");
            Assert.Greater(claimsAfter.Roles.Count, 0, "Token should contain roles for stateless operation");
        }

        [Test]
        public void TestSecurity_TokenContainsEncryptedInformation()
        {
            // Arrange
            string token = _tokenService.CreateToken(_testUser);
            var parts = token.Split('.');
            string payload = parts[1]; // Payload is base64url encoded, not encrypted

            // Act
            // Try to decode payload manually
            string decodedPayload = System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(payload.Replace("-", "+").Replace("_", "/")));

            // Assert
            // Token is encoded but not encrypted - sensitive data should not be critical
            Assert.That(decodedPayload, Does.Contain("test_user"), "Payload is encoded (base64), not encrypted");
        }

        [Test]
        public void TestToken_WithoutRoles()
        {
            // Arrange
            var userWithoutRoles = new User("noroluser", "norole@example.com")
            {
                UserID = 2,
                IsActive = true,
                Roles = new List<Role>() // Empty roles
            };

            // Act
            string token = _tokenService.CreateToken(userWithoutRoles);
            bool isValid = _tokenService.ValidateToken(token, out var claims);

            // Assert
            Assert.IsTrue(isValid, "Token should be created even without roles");
            Assert.AreEqual(0, claims.Roles.Count, "Token should have no roles");
        }

        [Test]
        public void TestTokenIntegrity_SignatureVerification()
        {
            // Arrange
            string token = _tokenService.CreateToken(_testUser);

            // Act
            // Valid token should pass
            bool isValid = _tokenService.ValidateToken(token, out var claims);
            
            // Modify a character in the signature
            var parts = token.Split('.');
            string modifiedSignature = parts[2].Length > 1 
                ? parts[2].Substring(0, parts[2].Length - 1) + "X"
                : "X";
            string modifiedToken = $"{parts[0]}.{parts[1]}.{modifiedSignature}";
            bool isModifiedValid = _tokenService.ValidateToken(modifiedToken, out var _);

            // Assert
            Assert.IsTrue(isValid, "Valid token should pass");
            Assert.IsFalse(isModifiedValid, "Modified token should fail signature verification");
        }
    }
}
