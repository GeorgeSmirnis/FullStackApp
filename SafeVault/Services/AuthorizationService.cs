using System;
using System.Collections.Generic;
using System.Linq;
using SafeVault.Models;

namespace SafeVault.Services
{
    /// <summary>
    /// Authorization Service for Role-Based Access Control (RBAC).
    /// 
    /// This service manages:
    /// - User role assignment
    /// - Permission assignment to roles
    /// - Authorization checks for resources and operations
    /// - Access control enforcement
    /// 
    /// Security Principle: Least Privilege
    /// Users should have the minimum permissions required to perform their duties.
    /// </summary>
    public class AuthorizationService
    {
        // Predefined roles
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string User = "User";
            public const string Moderator = "Moderator";
            public const string GuestUser = "Guest";
        }

        // Predefined permissions
        public static class Permissions
        {
            // User management
            public const string CreateUser = "CreateUser";
            public const string ReadUser = "ReadUser";
            public const string UpdateUser = "UpdateUser";
            public const string DeleteUser = "DeleteUser";

            // User data access
            public const string ViewAllUsers = "ViewAllUsers";
            public const string ViewSensitiveData = "ViewSensitiveData";

            // Admin operations
            public const string ManageRoles = "ManageRoles";
            public const string ManagePermissions = "ManagePermissions";
            public const string ViewAuditLog = "ViewAuditLog";
            public const string ModerateContent = "ModerateContent";

            // User operations
            public const string UpdateOwnProfile = "UpdateOwnProfile";
            public const string ViewOwnProfile = "ViewOwnProfile";
            public const string ChangePassword = "ChangePassword";
        }

        private readonly Dictionary<string, Role> _roles;
        private readonly Dictionary<string, Permission> _permissions;

        public AuthorizationService()
        {
            _permissions = new Dictionary<string, Permission>();
            _roles = new Dictionary<string, Role>();

            // Initialize default permissions and roles
            InitializeDefaultPermissions();
            InitializeDefaultRoles();
        }

        /// <summary>
        /// Initializes all default permissions.
        /// </summary>
        private void InitializeDefaultPermissions()
        {
            AddPermission(Permissions.CreateUser, "Create new user accounts");
            AddPermission(Permissions.ReadUser, "Read user information");
            AddPermission(Permissions.UpdateUser, "Update user information");
            AddPermission(Permissions.DeleteUser, "Delete user accounts");
            AddPermission(Permissions.ViewAllUsers, "View all users in the system");
            AddPermission(Permissions.ViewSensitiveData, "Access sensitive user data");
            AddPermission(Permissions.ManageRoles, "Create and manage roles");
            AddPermission(Permissions.ManagePermissions, "Create and manage permissions");
            AddPermission(Permissions.ViewAuditLog, "View system audit logs");
            AddPermission(Permissions.ModerateContent, "Moderate user content");
            AddPermission(Permissions.UpdateOwnProfile, "Update own profile");
            AddPermission(Permissions.ViewOwnProfile, "View own profile");
            AddPermission(Permissions.ChangePassword, "Change password");
        }

        /// <summary>
        /// Initializes default roles with their permissions.
        /// Follows the principle of least privilege.
        /// </summary>
        private void InitializeDefaultRoles()
        {
            // Admin role - full access
            var adminRole = new Role(Roles.Admin, "Administrator with full system access")
            {
                Permissions = new List<Permission>
                {
                    _permissions[Permissions.CreateUser],
                    _permissions[Permissions.ReadUser],
                    _permissions[Permissions.UpdateUser],
                    _permissions[Permissions.DeleteUser],
                    _permissions[Permissions.ViewAllUsers],
                    _permissions[Permissions.ViewSensitiveData],
                    _permissions[Permissions.ManageRoles],
                    _permissions[Permissions.ManagePermissions],
                    _permissions[Permissions.ViewAuditLog],
                    _permissions[Permissions.ModerateContent],
                    _permissions[Permissions.UpdateOwnProfile],
                    _permissions[Permissions.ViewOwnProfile],
                    _permissions[Permissions.ChangePassword]
                }
            };
            _roles[Roles.Admin] = adminRole;

            // Moderator role - moderate content and view logs
            var moderatorRole = new Role(Roles.Moderator, "Moderator for user-generated content")
            {
                Permissions = new List<Permission>
                {
                    _permissions[Permissions.ReadUser],
                    _permissions[Permissions.ViewAllUsers],
                    _permissions[Permissions.ViewAuditLog],
                    _permissions[Permissions.ModerateContent],
                    _permissions[Permissions.UpdateOwnProfile],
                    _permissions[Permissions.ViewOwnProfile],
                    _permissions[Permissions.ChangePassword]
                }
            };
            _roles[Roles.Moderator] = moderatorRole;

            // User role - basic permissions for regular users
            var userRole = new Role(Roles.User, "Regular user with basic permissions")
            {
                Permissions = new List<Permission>
                {
                    _permissions[Permissions.ReadUser],
                    _permissions[Permissions.UpdateOwnProfile],
                    _permissions[Permissions.ViewOwnProfile],
                    _permissions[Permissions.ChangePassword]
                }
            };
            _roles[Roles.User] = userRole;

            // Guest role - minimal permissions
            var guestRole = new Role(Roles.GuestUser, "Guest user with read-only access")
            {
                Permissions = new List<Permission>
                {
                    _permissions[Permissions.ViewOwnProfile]
                }
            };
            _roles[Roles.GuestUser] = guestRole;
        }

        /// <summary>
        /// Adds a permission to the system.
        /// </summary>
        /// <param name="name">Permission name</param>
        /// <param name="description">Permission description</param>
        /// <returns>The created permission</returns>
        public Permission AddPermission(string name, string description)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Permission name cannot be empty");

            if (_permissions.ContainsKey(name))
                return _permissions[name];

            var permission = new Permission(name, description);
            _permissions[name] = permission;
            return permission;
        }

        /// <summary>
        /// Gets a permission by name.
        /// </summary>
        /// <param name="name">Permission name</param>
        /// <returns>Permission or null if not found</returns>
        public Permission GetPermission(string name)
        {
            return _permissions.ContainsKey(name) ? _permissions[name] : null;
        }

        /// <summary>
        /// Adds a role to the system.
        /// </summary>
        /// <param name="name">Role name</param>
        /// <param name="description">Role description</param>
        /// <returns>The created role</returns>
        public Role AddRole(string name, string description = "")
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Role name cannot be empty");

            if (_roles.ContainsKey(name))
                return _roles[name];

            var role = new Role(name, description);
            _roles[name] = role;
            return role;
        }

        /// <summary>
        /// Gets a role by name.
        /// </summary>
        /// <param name="name">Role name</param>
        /// <returns>Role or null if not found</returns>
        public Role GetRole(string name)
        {
            return _roles.ContainsKey(name) ? _roles[name] : null;
        }

        /// <summary>
        /// Assigns a permission to a role.
        /// </summary>
        /// <param name="roleName">Role name</param>
        /// <param name="permissionName">Permission name</param>
        /// <returns>True if assignment succeeded</returns>
        public bool AssignPermissionToRole(string roleName, string permissionName)
        {
            if (!_roles.ContainsKey(roleName))
                return false;

            if (!_permissions.ContainsKey(permissionName))
                return false;

            var role = _roles[roleName];
            var permission = _permissions[permissionName];

            // Avoid duplicates
            if (role.Permissions.Any(p => p.Name == permissionName))
                return true;

            role.Permissions.Add(permission);
            return true;
        }

        /// <summary>
        /// Removes a permission from a role.
        /// </summary>
        /// <param name="roleName">Role name</param>
        /// <param name="permissionName">Permission name</param>
        /// <returns>True if removal succeeded</returns>
        public bool RemovePermissionFromRole(string roleName, string permissionName)
        {
            if (!_roles.ContainsKey(roleName))
                return false;

            var role = _roles[roleName];
            var permission = role.Permissions.FirstOrDefault(p => p.Name == permissionName);

            if (permission == null)
                return false;

            role.Permissions.Remove(permission);
            return true;
        }

        /// <summary>
        /// Assigns a role to a user.
        /// </summary>
        /// <param name="user">User to assign role to</param>
        /// <param name="roleName">Role name</param>
        /// <returns>True if assignment succeeded</returns>
        public bool AssignRoleToUser(User user, string roleName)
        {
            if (user == null || !_roles.ContainsKey(roleName))
                return false;

            if (user.Roles == null)
                user.Roles = new List<Role>();

            // Avoid duplicates
            if (user.Roles.Any(r => r.Name == roleName))
                return true;

            user.Roles.Add(_roles[roleName]);
            user.LastModified = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        /// <param name="user">User to remove role from</param>
        /// <param name="roleName">Role name</param>
        /// <returns>True if removal succeeded</returns>
        public bool RemoveRoleFromUser(User user, string roleName)
        {
            if (user == null || user.Roles == null)
                return false;

            var role = user.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null)
                return false;

            user.Roles.Remove(role);
            user.LastModified = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Checks if a user has a specific permission.
        /// This is the core authorization check.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="permissionName">Permission to check</param>
        /// <returns>True if user has permission</returns>
        public bool HasPermission(User user, string permissionName)
        {
            if (user == null || !user.IsActive)
                return false;

            if (user.Roles == null || user.Roles.Count == 0)
                return false;

            // Check if user's roles have the permission
            return user.Roles.Any(role => role.HasPermission(permissionName));
        }

        /// <summary>
        /// Checks if a user has ANY of the specified permissions.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="permissionNames">Permissions to check</param>
        /// <returns>True if user has any permission</returns>
        public bool HasAnyPermission(User user, params string[] permissionNames)
        {
            if (user == null || permissionNames == null || permissionNames.Length == 0)
                return false;

            return permissionNames.Any(p => HasPermission(user, p));
        }

        /// <summary>
        /// Checks if a user has ALL of the specified permissions.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="permissionNames">Permissions to check</param>
        /// <returns>True if user has all permissions</returns>
        public bool HasAllPermissions(User user, params string[] permissionNames)
        {
            if (user == null || permissionNames == null || permissionNames.Length == 0)
                return false;

            return permissionNames.All(p => HasPermission(user, p));
        }

        /// <summary>
        /// Checks if user has a specific role.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="roleName">Role name</param>
        /// <returns>True if user has the role</returns>
        public bool HasRole(User user, string roleName)
        {
            if (user == null || user.Roles == null)
                return false;

            return user.Roles.Any(r => r.Name == roleName);
        }

        /// <summary>
        /// Checks if user is an administrator.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <returns>True if user is admin</returns>
        public bool IsAdministrator(User user)
        {
            return HasRole(user, Roles.Admin);
        }

        /// <summary>
        /// Checks if user can perform an operation on a resource.
        /// For example: Can user update user with ID 123?
        /// </summary>
        /// <param name="user">User performing the action</param>
        /// <param name="permission">Required permission</param>
        /// <param name="resourceOwnerId">ID of resource owner (for resource-level authorization)</param>
        /// <returns>True if authorization granted</returns>
        public bool CanAccessResource(User user, string permission, int resourceOwnerId)
        {
            // If user doesn't have base permission, deny
            if (!HasPermission(user, permission))
                return false;

            // Users can always access their own resources
            if (user.UserID == resourceOwnerId)
                return true;

            // Otherwise, check if user has permission to access others' resources
            // For example, admins can access any resource
            if (IsAdministrator(user))
                return true;

            return false;
        }

        /// <summary>
        /// Gets all roles in the system.
        /// </summary>
        /// <returns>List of all roles</returns>
        public List<Role> GetAllRoles()
        {
            return _roles.Values.ToList();
        }

        /// <summary>
        /// Gets all permissions in the system.
        /// </summary>
        /// <returns>List of all permissions</returns>
        public List<Permission> GetAllPermissions()
        {
            return _permissions.Values.ToList();
        }

        /// <summary>
        /// Gets user's effective permissions (union of all role permissions).
        /// </summary>
        /// <param name="user">User to get permissions for</param>
        /// <returns>List of permission names</returns>
        public List<string> GetUserPermissions(User user)
        {
            if (user == null || user.Roles == null)
                return new List<string>();

            return user.Roles
                .SelectMany(r => r.Permissions)
                .Select(p => p.Name)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Validates that a user has required permissions to access an admin function.
        /// </summary>
        /// <param name="user">User to check</param>
        /// <returns>True if user is authorized for admin access</returns>
        public bool CanAccessAdminPanel(User user)
        {
            if (user == null || !user.IsActive)
                return false;

            return HasAnyPermission(user,
                Permissions.ManageRoles,
                Permissions.ManagePermissions,
                Permissions.ViewAuditLog,
                Permissions.DeleteUser);
        }
    }
}
