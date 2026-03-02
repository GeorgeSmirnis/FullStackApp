using NUnit.Framework;
using SafeVault.Services;
using SafeVault.Models;
using System;
using System.Collections.Generic;

namespace SafeVault.Tests
{
    [TestFixture]
    public class TestAuthorization
    {
        private AuthorizationService _authService;
        private User _adminUser;
        private User _regularUser;
        private User _moderatorUser;

        [SetUp]
        public void Setup()
        {
            _authService = new AuthorizationService();

            // Create test users with different roles
            _adminUser = new User("admin_user", "admin@example.com")
            {
                UserID = 1,
                IsActive = true,
                Roles = new List<Role>()
            };
            _authService.AssignRoleToUser(_adminUser, AuthorizationService.Roles.Admin);

            _regularUser = new User("regular_user", "user@example.com")
            {
                UserID = 2,
                IsActive = true,
                Roles = new List<Role>()
            };
            _authService.AssignRoleToUser(_regularUser, AuthorizationService.Roles.User);

            _moderatorUser = new User("moderator_user", "moderator@example.com")
            {
                UserID = 3,
                IsActive = true,
                Roles = new List<Role>()
            };
            _authService.AssignRoleToUser(_moderatorUser, AuthorizationService.Roles.Moderator);
        }

        // ==================== ROLE ASSIGNMENT TESTS ====================

        [Test]
        public void TestAssignRoleToUser_SuccessfulAssignment()
        {
            // Arrange
            var user = new User("new_user", "new@example.com") { UserID = 4 };

            // Act
            bool result = _authService.AssignRoleToUser(user, AuthorizationService.Roles.User);

            // Assert
            Assert.IsTrue(result, "Role assignment should succeed");
            Assert.IsTrue(_authService.HasRole(user, AuthorizationService.Roles.User), "User should have role");
        }

        [Test]
        public void TestAssignRoleToUser_InexistentRole()
        {
            // Arrange
            var user = new User("new_user", "new@example.com") { UserID = 4 };

            // Act
            bool result = _authService.AssignRoleToUser(user, "NonExistentRole");

            // Assert
            Assert.IsFalse(result, "Assigning non-existent role should fail");
        }

        [Test]
        public void TestAssignRoleToUser_NullUser()
        {
            // Act
            bool result = _authService.AssignRoleToUser(null, AuthorizationService.Roles.User);

            // Assert
            Assert.IsFalse(result, "Assigning role to null user should fail");
        }

        [Test]
        public void TestRemoveRoleFromUser_SuccessfulRemoval()
        {
            // Arrange
            var user = new User("user", "user@example.com") { UserID = 5 };
            _authService.AssignRoleToUser(user, AuthorizationService.Roles.User);

            // Act
            bool result = _authService.RemoveRoleFromUser(user, AuthorizationService.Roles.User);

            // Assert
            Assert.IsTrue(result, "Role removal should succeed");
            Assert.IsFalse(_authService.HasRole(user, AuthorizationService.Roles.User), "User should not have role");
        }

        [Test]
        public void TestRemoveRoleFromUser_RoleNotAssigned()
        {
            // Arrange
            var user = new User("user", "user@example.com") { UserID = 6 };

            // Act
            bool result = _authService.RemoveRoleFromUser(user, AuthorizationService.Roles.User);

            // Assert
            Assert.IsFalse(result, "Removing non-assigned role should fail");
        }

        // ==================== PERMISSION TESTS ====================

        [Test]
        public void TestAddPermission_CreatesPermission()
        {
            // Arrange
            string permissionName = "TestPermission";

            // Act
            var permission = _authService.AddPermission(permissionName, "Test permission");

            // Assert
            Assert.IsNotNull(permission, "Permission should be created");
            Assert.AreEqual(permissionName, permission.Name, "Permission name should match");
        }

        [Test]
        public void TestGetPermission_RetrievesPermission()
        {
            // Arrange
            string permissionName = AuthorizationService.Permissions.ViewAllUsers;

            // Act
            var permission = _authService.GetPermission(permissionName);

            // Assert
            Assert.IsNotNull(permission, "Permission should be found");
            Assert.AreEqual(permissionName, permission.Name, "Permission name should match");
        }

        [Test]
        public void TestAssignPermissionToRole_SuccessfulAssignment()
        {
            // Arrange
            var role = _authService.AddRole("TestRole", "Test role");
            string permissionName = AuthorizationService.Permissions.ReadUser;

            // Act
            bool result = _authService.AssignPermissionToRole("TestRole", permissionName);

            // Assert
            Assert.IsTrue(result, "Permission assignment should succeed");
            Assert.IsTrue(role.HasPermission(permissionName), "Role should have permission");
        }

        [Test]
        public void TestRemovePermissionFromRole_SuccessfulRemoval()
        {
            // Arrange
            var role = _authService.AddRole("TestRole2", "Test role 2");
            string permissionName = AuthorizationService.Permissions.ReadUser;
            _authService.AssignPermissionToRole("TestRole2", permissionName);

            // Act
            bool result = _authService.RemovePermissionFromRole("TestRole2", permissionName);

            // Assert
            Assert.IsTrue(result, "Permission removal should succeed");
            Assert.IsFalse(role.HasPermission(permissionName), "Role should not have permission");
        }

        // ==================== AUTHORIZATION CHECK TESTS ====================

        [Test]
        public void TestHasPermission_AdminHasAllPermissions()
        {
            // Assert
            Assert.IsTrue(_authService.HasPermission(_adminUser, AuthorizationService.Permissions.CreateUser), "Admin should have CreateUser");
            Assert.IsTrue(_authService.HasPermission(_adminUser, AuthorizationService.Permissions.DeleteUser), "Admin should have DeleteUser");
            Assert.IsTrue(_authService.HasPermission(_adminUser, AuthorizationService.Permissions.ManageRoles), "Admin should have ManageRoles");
        }

        [Test]
        public void TestHasPermission_RegularUserLimitedPermissions()
        {
            // Assert
            Assert.IsTrue(_authService.HasPermission(_regularUser, AuthorizationService.Permissions.ViewOwnProfile), "User should have ViewOwnProfile");
            Assert.IsFalse(_authService.HasPermission(_regularUser, AuthorizationService.Permissions.DeleteUser), "User should not have DeleteUser");
            Assert.IsFalse(_authService.HasPermission(_regularUser, AuthorizationService.Permissions.ManageRoles), "User should not have ManageRoles");
        }

        [Test]
        public void TestHasPermission_ModeratorSpecificPermissions()
        {
            // Assert
            Assert.IsTrue(_authService.HasPermission(_moderatorUser, AuthorizationService.Permissions.ModerateContent), "Moderator should have ModerateContent");
            Assert.IsTrue(_authService.HasPermission(_moderatorUser, AuthorizationService.Permissions.ViewAuditLog), "Moderator should have ViewAuditLog");
            Assert.IsFalse(_authService.HasPermission(_moderatorUser, AuthorizationService.Permissions.ManageRoles), "Moderator should not have ManageRoles");
        }

        [Test]
        public void TestHasPermission_InactiveUserNoPermissions()
        {
            // Arrange
            _adminUser.IsActive = false;

            // Act
            bool hasPermission = _authService.HasPermission(_adminUser, AuthorizationService.Permissions.CreateUser);

            // Assert
            Assert.IsFalse(hasPermission, "Inactive admin should not have permissions");
        }

        [Test]
        public void TestHasPermission_NullUser()
        {
            // Act
            bool result = _authService.HasPermission(null, AuthorizationService.Permissions.ViewOwnProfile);

            // Assert
            Assert.IsFalse(result, "Null user should not have permissions");
        }

        [Test]
        public void TestHasAnyPermission_UserWithOnePermission()
        {
            // Act
            bool result = _authService.HasAnyPermission(_regularUser,
                AuthorizationService.Permissions.DeleteUser,
                AuthorizationService.Permissions.ViewOwnProfile);

            // Assert
            Assert.IsTrue(result, "User should have at least one permission");
        }

        [Test]
        public void TestHasAnyPermission_UserWithNoneOfPermissions()
        {
            // Act
            bool result = _authService.HasAnyPermission(_regularUser,
                AuthorizationService.Permissions.DeleteUser,
                AuthorizationService.Permissions.ManageRoles);

            // Assert
            Assert.IsFalse(result, "User should not have any of these permissions");
        }

        [Test]
        public void TestHasAllPermissions_AdminHasAll()
        {
            // Act
            bool result = _authService.HasAllPermissions(_adminUser,
                AuthorizationService.Permissions.CreateUser,
                AuthorizationService.Permissions.DeleteUser,
                AuthorizationService.Permissions.ManageRoles);

            // Assert
            Assert.IsTrue(result, "Admin should have all permissions");
        }

        [Test]
        public void TestHasAllPermissions_RegularUserDoesNotHaveAll()
        {
            // Act
            bool result = _authService.HasAllPermissions(_regularUser,
                AuthorizationService.Permissions.ViewOwnProfile,
                AuthorizationService.Permissions.DeleteUser);

            // Assert
            Assert.IsFalse(result, "Regular user should not have all permissions");
        }

        // ==================== ROLE TESTS ====================

        [Test]
        public void TestHasRole_UserHasRole()
        {
            // Act
            bool hasRole = _authService.HasRole(_adminUser, AuthorizationService.Roles.Admin);

            // Assert
            Assert.IsTrue(hasRole, "Admin user should have Admin role");
        }

        [Test]
        public void TestHasRole_UserDoesNotHaveRole()
        {
            // Act
            bool hasRole = _authService.HasRole(_regularUser, AuthorizationService.Roles.Admin);

            // Assert
            Assert.IsFalse(hasRole, "Regular user should not have Admin role");
        }

        [Test]
        public void TestIsAdministrator_AdminUser()
        {
            // Act
            bool isAdmin = _authService.IsAdministrator(_adminUser);

            // Assert
            Assert.IsTrue(isAdmin, "Admin user should be identified as administrator");
        }

        [Test]
        public void TestIsAdministrator_RegularUser()
        {
            // Act
            bool isAdmin = _authService.IsAdministrator(_regularUser);

            // Assert
            Assert.IsFalse(isAdmin, "Regular user should not be administrator");
        }

        // ==================== RESOURCE LEVEL AUTHORIZATION TESTS ====================

        [Test]
        public void TestCanAccessResource_OwnResource()
        {
            // Arrange
            int resourceOwnerId = _regularUser.UserID;

            // Act
            bool canAccess = _authService.CanAccessResource(_regularUser,
                AuthorizationService.Permissions.UpdateOwnProfile, resourceOwnerId);

            // Assert
            Assert.IsTrue(canAccess, "User should access their own resource");
        }

        [Test]
        public void TestCanAccessResource_OthersResourceWithoutPermission()
        {
            // Arrange
            int otherUserId = 999;

            // Act
            bool canAccess = _authService.CanAccessResource(_regularUser,
                AuthorizationService.Permissions.UpdateUser, otherUserId);

            // Assert
            Assert.IsFalse(canAccess, "Regular user should not access others' resource");
        }

        [Test]
        public void TestCanAccessResource_OthersResourceWithAdminRole()
        {
            // Arrange
            int otherUserId = 999;

            // Act
            bool canAccess = _authService.CanAccessResource(_adminUser,
                AuthorizationService.Permissions.UpdateUser, otherUserId);

            // Assert
            Assert.IsTrue(canAccess, "Admin should access any resource");
        }

        [Test]
        public void TestCanAccessResource_WithoutBasePermission()
        {
            // Arrange
            int resourceOwnerId = _regularUser.UserID;

            // Act
            bool canAccess = _authService.CanAccessResource(_regularUser,
                AuthorizationService.Permissions.DeleteUser, resourceOwnerId);

            // Assert
            Assert.IsFalse(canAccess, "User without permission should not access resource");
        }

        // ==================== ADMIN PANEL ACCESS TESTS ====================

        [Test]
        public void TestCanAccessAdminPanel_AdminUser()
        {
            // Act
            bool canAccess = _authService.CanAccessAdminPanel(_adminUser);

            // Assert
            Assert.IsTrue(canAccess, "Admin should access admin panel");
        }

        [Test]
        public void TestCanAccessAdminPanel_ModeratorUser()
        {
            // Act
            bool canAccess = _authService.CanAccessAdminPanel(_moderatorUser);

            // Assert
            Assert.IsTrue(canAccess, "Moderator should access admin panel for moderation");
        }

        [Test]
        public void TestCanAccessAdminPanel_RegularUser()
        {
            // Act
            bool canAccess = _authService.CanAccessAdminPanel(_regularUser);

            // Assert
            Assert.IsFalse(canAccess, "Regular user should not access admin panel");
        }

        [Test]
        public void TestCanAccessAdminPanel_InactiveAdmin()
        {
            // Arrange
            _adminUser.IsActive = false;

            // Act
            bool canAccess = _authService.CanAccessAdminPanel(_adminUser);

            // Assert
            Assert.IsFalse(canAccess, "Inactive admin should not access admin panel");
        }

        [Test]
        public void TestCanAccessAdminPanel_NullUser()
        {
            // Act
            bool canAccess = _authService.CanAccessAdminPanel(null);

            // Assert
            Assert.IsFalse(canAccess, "Null user should not access admin panel");
        }

        // ==================== ROLE AND PERMISSION RETRIEVAL TESTS ====================

        [Test]
        public void TestGetAllRoles_ReturnsAllRoles()
        {
            // Act
            var roles = _authService.GetAllRoles();

            // Assert
            Assert.IsNotNull(roles, "Roles list should not be null");
            Assert.Greater(roles.Count, 0, "Should have default roles");
            Assert.IsTrue(roles.Any(r => r.Name == AuthorizationService.Roles.Admin), "Should include Admin role");
            Assert.IsTrue(roles.Any(r => r.Name == AuthorizationService.Roles.User), "Should include User role");
        }

        [Test]
        public void TestGetAllPermissions_ReturnsAllPermissions()
        {
            // Act
            var permissions = _authService.GetAllPermissions();

            // Assert
            Assert.IsNotNull(permissions, "Permissions list should not be null");
            Assert.Greater(permissions.Count, 0, "Should have default permissions");
        }

        [Test]
        public void TestGetUserPermissions_ReturnsUserPermissions()
        {
            // Act
            var permissions = _authService.GetUserPermissions(_regularUser);

            // Assert
            Assert.IsNotNull(permissions, "Permissions list should not be null");
            Assert.Greater(permissions.Count, 0, "User should have permissions");
            Assert.IsTrue(permissions.Contains(AuthorizationService.Permissions.ViewOwnProfile), "Should include ViewOwnProfile");
        }

        [Test]
        public void TestGetUserPermissions_AdminHasMorePermissions()
        {
            // Act
            var adminPermissions = _authService.GetUserPermissions(_adminUser);
            var userPermissions = _authService.GetUserPermissions(_regularUser);

            // Assert
            Assert.Greater(adminPermissions.Count, userPermissions.Count, "Admin should have more permissions than regular user");
        }

        // ==================== ATTACK SCENARIO TESTS ====================

        [Test]
        public void TestAttack_PrivilegeEscalation_DirectRoleAssignment()
        {
            // Arrange
            var user = new User("attacker", "attacker@example.com") { UserID = 10, Roles = new List<Role>() };
            _authService.AssignRoleToUser(user, AuthorizationService.Roles.User);

            // Act
            // Attacker tries to assign themselves admin role
            _authService.AssignRoleToUser(user, AuthorizationService.Roles.Admin);

            // Assert
            // In real system, this should only be done through secure admin interface
            Assert.IsTrue(_authService.HasRole(user, AuthorizationService.Roles.Admin),
                "This demonstrates why authorization must be validated server-side");
        }

        [Test]
        public void TestAttack_UnauthorizedDataAccess()
        {
            // Arrange
            int anotherUserId = 999;

            // Act
            bool canDelete = _authService.HasPermission(_regularUser, AuthorizationService.Permissions.DeleteUser);

            // Assert
            Assert.IsFalse(canDelete, "Regular user should not be able to delete other users");
        }

        [Test]
        public void TestAttack_AdminPanelAccess()
        {
            // Act
            bool canAccessAdmin = _authService.CanAccessAdminPanel(_regularUser);

            // Assert
            Assert.IsFalse(canAccessAdmin, "Regular user should not access admin panel");
        }

        // ==================== PRINCIPLE OF LEAST PRIVILEGE TESTS ====================

        [Test]
        public void TestPrincipleOfLeastPrivilege_GuestUser()
        {
            // Arrange
            var guestUser = new User("guest", "guest@example.com") { UserID = 11, IsActive = true, Roles = new List<Role>() };
            _authService.AssignRoleToUser(guestUser, AuthorizationService.Roles.GuestUser);

            // Act
            var permissions = _authService.GetUserPermissions(guestUser);

            // Assert
            Assert.AreEqual(1, permissions.Count, "Guest should have minimal permissions");
            Assert.IsTrue(permissions.Contains(AuthorizationService.Permissions.ViewOwnProfile), "Guest should only view own profile");
        }

        [Test]
        public void TestRoleHierarchy_ModeratorVsAdmin()
        {
            // Act
            var moderatorPermissions = _authService.GetUserPermissions(_moderatorUser);
            var adminPermissions = _authService.GetUserPermissions(_adminUser);

            // Assert
            Assert.Less(moderatorPermissions.Count, adminPermissions.Count, "Moderator should have fewer permissions than admin");
            
            // All moderator permissions should be in admin permissions
            foreach (var perm in moderatorPermissions)
            {
                Assert.IsTrue(adminPermissions.Contains(perm), $"Admin should have moderator permission: {perm}");
            }
        }

        [Test]
        public void TestMultipleRoles_UserWithMultipleRoles()
        {
            // Arrange
            var user = new User("multi_role", "multi@example.com") { UserID = 12, IsActive = true, Roles = new List<Role>() };
            
            // Act
            _authService.AssignRoleToUser(user, AuthorizationService.Roles.User);
            _authService.AssignRoleToUser(user, AuthorizationService.Roles.Moderator);
            var permissions = _authService.GetUserPermissions(user);

            // Assert
            Assert.Greater(permissions.Count, 1, "User with multiple roles should have unified permissions");
            Assert.IsTrue(_authService.HasRole(user, AuthorizationService.Roles.User), "Should have User role");
            Assert.IsTrue(_authService.HasRole(user, AuthorizationService.Roles.Moderator), "Should have Moderator role");
        }
    }
}
