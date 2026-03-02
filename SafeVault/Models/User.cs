using System;
using System.Collections.Generic;

namespace SafeVault.Models
{
    /// <summary>
    /// Represents a user in the SafeVault system.
    /// This model is used for secure user data management with authentication and authorization support.
    /// </summary>
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        
        // Authentication fields
        public string PasswordHash { get; set; }  // Hashed password using bcrypt
        public string PasswordSalt { get; set; }  // Salt for additional security
        
        // Authorization fields
        public List<Role> Roles { get; set; } = new List<Role>();
        public bool IsActive { get; set; } = true;
        public bool IsEmailVerified { get; set; } = false;
        
        // Audit fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutUntil { get; set; }

        public User() { }

        public User(string username, string email)
        {
            Username = username;
            Email = email;
        }

        public User(string username, string email, string passwordHash)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
        }

        /// <summary>
        /// Checks if the user account is currently locked due to failed login attempts.
        /// </summary>
        public bool IsLockedOut()
        {
            if (!LockoutUntil.HasValue)
                return false;

            if (LockoutUntil.Value > DateTime.UtcNow)
                return true;

            // Unlock if lockout period has expired
            LockoutUntil = null;
            FailedLoginAttempts = 0;
            return false;
        }

        /// <summary>
        /// Returns true if user has any of the specified roles.
        /// </summary>
        public bool HasRole(string roleName)
        {
            return Roles?.Contains(new Role { Name = roleName }) ?? false;
        }

        /// <summary>
        /// Returns true if user has all of the specified roles.
        /// </summary>
        public bool HasAllRoles(params string[] roleNames)
        {
            foreach (var roleName in roleNames)
            {
                if (!HasRole(roleName))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if user has any admin privileges.
        /// </summary>
        public bool IsAdmin()
        {
            return HasRole("Admin");
        }
    }

    /// <summary>
    /// Represents a role in the SafeVault system for authorization.
    /// </summary>
    public class Role : IEquatable<Role>
    {
        public int RoleID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Permission> Permissions { get; set; } = new List<Permission>();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public Role() { }

        public Role(string name)
        {
            Name = name;
        }

        public Role(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public bool Equals(Role other)
        {
            if (other == null)
                return false;
            return this.Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Role);
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Returns true if this role has the specified permission.
        /// </summary>
        public bool HasPermission(string permissionName)
        {
            return Permissions?.Any(p => p.Name == permissionName) ?? false;
        }
    }

    /// <summary>
    /// Represents a permission that can be assigned to roles.
    /// </summary>
    public class Permission
    {
        public int PermissionID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public Permission() { }

        public Permission(string name)
        {
            Name = name;
        }

        public Permission(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    /// <summary>
    /// Represents the claims in an authentication token.
    /// </summary>
    public class TokenClaims
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Permissions { get; set; } = new List<string>();
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Login request model.
    /// </summary>
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// Authentication response with token.
    /// </summary>
    public class AuthenticationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public User User { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
