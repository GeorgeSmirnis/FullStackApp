using System.Text.RegularExpressions;
using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

/// <summary>
/// Service for validating user data
/// Centralizes all validation logic to ensure consistency
/// </summary>
public class UserValidationService
{
    // Email regex pattern for validation
    private static readonly Regex EmailRegex = new(
        @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Maximum lengths for string fields
    private const int MaxFirstNameLength = 50;
    private const int MaxLastNameLength = 50;
    private const int MaxEmailLength = 100;
    private const int MaxJobTitleLength = 100;
    private const int MaxDepartmentLength = 50;

    // Minimum required date (used to validate HireDate)
    private static readonly DateTime MinAllowedDate = new(1900, 1, 1);

    /// <summary>
    /// Validates a complete user object for creation
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateUserForCreation(User user)
    {
        var errors = new List<string>();

        if (user == null)
        {
            errors.Add("User object cannot be null");
            return (false, errors);
        }

        ValidateFirstName(user.FirstName, errors);
        ValidateLastName(user.LastName, errors);
        ValidateEmail(user.Email, errors);
        ValidateJobTitle(user.JobTitle, errors);
        ValidateDepartment(user.Department, errors);
        ValidateHireDate(user.HireDate, errors);

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Validates a user object for updates
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateUserForUpdate(User user)
    {
        var errors = new List<string>();

        if (user == null)
        {
            errors.Add("User object cannot be null");
            return (false, errors);
        }

        // For updates, allow empty values for optional fields
        if (!string.IsNullOrWhiteSpace(user.FirstName))
        {
            ValidateFirstName(user.FirstName, errors);
        }

        if (!string.IsNullOrWhiteSpace(user.LastName))
        {
            ValidateLastName(user.LastName, errors);
        }

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            ValidateEmail(user.Email, errors);
        }

        if (!string.IsNullOrWhiteSpace(user.JobTitle))
        {
            ValidateJobTitle(user.JobTitle, errors);
        }

        if (!string.IsNullOrWhiteSpace(user.Department))
        {
            ValidateDepartment(user.Department, errors);
        }

        if (user.HireDate != default(DateTime))
        {
            ValidateHireDate(user.HireDate, errors);
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Validates FirstName field
    /// </summary>
    private void ValidateFirstName(string? firstName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            errors.Add("FirstName is required and cannot be empty or whitespace");
            return;
        }

        if (firstName.Length > MaxFirstNameLength)
        {
            errors.Add($"FirstName cannot exceed {MaxFirstNameLength} characters");
        }
    }

    /// <summary>
    /// Validates LastName field
    /// </summary>
    private void ValidateLastName(string? lastName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(lastName))
        {
            errors.Add("LastName is required and cannot be empty or whitespace");
            return;
        }

        if (lastName.Length > MaxLastNameLength)
        {
            errors.Add($"LastName cannot exceed {MaxLastNameLength} characters");
        }
    }

    /// <summary>
    /// Validates Email field with regex pattern
    /// </summary>
    private void ValidateEmail(string? email, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("Email is required and cannot be empty or whitespace");
            return;
        }

        if (email.Length > MaxEmailLength)
        {
            errors.Add($"Email cannot exceed {MaxEmailLength} characters");
            return;
        }

        if (!EmailRegex.IsMatch(email))
        {
            errors.Add("Email format is invalid. Please provide a valid email address (e.g., user@domain.com)");
        }
    }

    /// <summary>
    /// Validates JobTitle field
    /// </summary>
    private void ValidateJobTitle(string? jobTitle, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(jobTitle))
        {
            errors.Add("JobTitle is required and cannot be empty or whitespace");
            return;
        }

        if (jobTitle.Length > MaxJobTitleLength)
        {
            errors.Add($"JobTitle cannot exceed {MaxJobTitleLength} characters");
        }
    }

    /// <summary>
    /// Validates Department field
    /// </summary>
    private void ValidateDepartment(string? department, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(department))
        {
            errors.Add("Department is required and cannot be empty or whitespace");
            return;
        }

        if (department.Length > MaxDepartmentLength)
        {
            errors.Add($"Department cannot exceed {MaxDepartmentLength} characters");
        }
    }

    /// <summary>
    /// Validates HireDate field
    /// </summary>
    private void ValidateHireDate(DateTime hireDate, List<string> errors)
    {
        if (hireDate == default(DateTime))
        {
            errors.Add("HireDate is required");
            return;
        }

        if (hireDate < MinAllowedDate)
        {
            errors.Add($"HireDate cannot be before {MinAllowedDate:yyyy-MM-dd}");
            return;
        }

        if (hireDate > DateTime.UtcNow.AddDays(1)) // Allow 1 day buffer for timezone differences
        {
            errors.Add("HireDate cannot be in the future");
        }
    }

    /// <summary>
    /// Validates if an ID is valid (positive integer)
    /// </summary>
    public bool ValidateId(int id)
    {
        return id > 0;
    }

    /// <summary>
    /// Sanitizes string input by trimming whitespace
    /// </summary>
    public string SanitizeString(string? input)
    {
        return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim();
    }
}
