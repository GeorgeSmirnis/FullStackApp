namespace UserManagementAPI.Models;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's job title
    /// </summary>
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Department the user belongs to
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Date when the user was hired
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Whether the user is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date when the user record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
