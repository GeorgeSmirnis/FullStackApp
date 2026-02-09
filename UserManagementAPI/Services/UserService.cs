using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

/// <summary>
/// Service for managing user operations with validation and error handling
/// Provides CRUD operations with thread-safe access to in-memory data store
/// </summary>
public class UserService
{
    // In-memory data store for demonstration purposes
    private static List<User> _users = new();
    private static int _nextId = 1;
    
    // Lock object for thread-safe operations
    private static readonly object _lockObject = new object();
    
    // Validation service
    private readonly UserValidationService _validationService;

    public UserService(UserValidationService validationService)
    {
        _validationService = validationService;
        
        // Initialize with sample data (thread-safe)
        lock (_lockObject)
        {
            if (_users.Count == 0)
            {
                _users.AddRange(new[]
                {
                    new User
                    {
                        Id = _nextId++,
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "john.doe@techhive.com",
                        JobTitle = "Software Engineer",
                        Department = "Engineering",
                        HireDate = new DateTime(2022, 1, 15),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = _nextId++,
                        FirstName = "Jane",
                        LastName = "Smith",
                        Email = "jane.smith@techhive.com",
                        JobTitle = "HR Manager",
                        Department = "Human Resources",
                        HireDate = new DateTime(2021, 6, 20),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                });
            }
        }
    }

    /// <summary>
    /// Retrieves all users with thread-safe access
    /// </summary>
    public List<User> GetAllUsers()
    {
        lock (_lockObject)
        {
            return _users.ToList();
        }
    }

    /// <summary>
    /// Retrieves a user by ID with validation and thread-safe access
    /// </summary>
    public User? GetUserById(int id)
    {
        if (!_validationService.ValidateId(id))
        {
            return null;
        }

        lock (_lockObject)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }
    }

    /// <summary>
    /// Creates a new user with validation
    /// Returns a tuple with the created user and any error messages
    /// </summary>
    public (User? user, List<string> errors) CreateUser(User user)
    {
        var errors = new List<string>();

        if (user == null)
        {
            errors.Add("User object cannot be null");
            return (null, errors);
        }

        // Validate user data
        var (isValid, validationErrors) = _validationService.ValidateUserForCreation(user);
        if (!isValid)
        {
            return (null, validationErrors);
        }

        // Sanitize string inputs
        user.FirstName = _validationService.SanitizeString(user.FirstName);
        user.LastName = _validationService.SanitizeString(user.LastName);
        user.Email = _validationService.SanitizeString(user.Email);
        user.JobTitle = _validationService.SanitizeString(user.JobTitle);
        user.Department = _validationService.SanitizeString(user.Department);

        // Check for duplicate email
        lock (_lockObject)
        {
            if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add($"A user with email '{user.Email}' already exists");
                return (null, errors);
            }

            // Create the user with new ID and timestamp
            user.Id = _nextId++;
            user.CreatedAt = DateTime.UtcNow;
            _users.Add(user);
            
            return (user, errors);
        }
    }

    /// <summary>
    /// Updates an existing user with validation
    /// Returns a tuple with success status and any error messages
    /// </summary>
    public (bool success, List<string> errors) UpdateUser(int id, User updatedUser)
    {
        var errors = new List<string>();

        if (!_validationService.ValidateId(id))
        {
            errors.Add("Invalid user ID. ID must be a positive integer.");
            return (false, errors);
        }

        if (updatedUser == null)
        {
            errors.Add("Updated user object cannot be null");
            return (false, errors);
        }

        // Validate user data
        var (isValid, validationErrors) = _validationService.ValidateUserForUpdate(updatedUser);
        if (!isValid)
        {
            return (false, validationErrors);
        }

        lock (_lockObject)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                errors.Add($"User with ID {id} not found");
                return (false, errors);
            }

            // Check for duplicate email if email is being changed
            if (!string.IsNullOrWhiteSpace(updatedUser.Email) && 
                !updatedUser.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (_users.Any(u => u.Id != id && u.Email.Equals(updatedUser.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add($"A user with email '{updatedUser.Email}' already exists");
                    return (false, errors);
                }
            }

            // Update only non-empty fields
            if (!string.IsNullOrWhiteSpace(updatedUser.FirstName))
            {
                user.FirstName = _validationService.SanitizeString(updatedUser.FirstName);
            }

            if (!string.IsNullOrWhiteSpace(updatedUser.LastName))
            {
                user.LastName = _validationService.SanitizeString(updatedUser.LastName);
            }

            if (!string.IsNullOrWhiteSpace(updatedUser.Email))
            {
                user.Email = _validationService.SanitizeString(updatedUser.Email);
            }

            if (!string.IsNullOrWhiteSpace(updatedUser.JobTitle))
            {
                user.JobTitle = _validationService.SanitizeString(updatedUser.JobTitle);
            }

            if (!string.IsNullOrWhiteSpace(updatedUser.Department))
            {
                user.Department = _validationService.SanitizeString(updatedUser.Department);
            }

            if (updatedUser.HireDate != default(DateTime))
            {
                user.HireDate = updatedUser.HireDate;
            }

            // Always allow IsActive to be set
            user.IsActive = updatedUser.IsActive;

            return (true, errors);
        }
    }

    /// <summary>
    /// Deletes a user by ID with validation
    /// Returns a tuple with success status and any error messages
    /// </summary>
    public (bool success, List<string> errors) DeleteUser(int id)
    {
        var errors = new List<string>();

        if (!_validationService.ValidateId(id))
        {
            errors.Add("Invalid user ID. ID must be a positive integer.");
            return (false, errors);
        }

        lock (_lockObject)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                errors.Add($"User with ID {id} not found");
                return (false, errors);
            }

            _users.Remove(user);
            return (true, errors);
        }
    }

    /// <summary>
    /// Searches users by department with thread-safe access
    /// </summary>
    public List<User> GetUsersByDepartment(string department)
    {
        if (string.IsNullOrWhiteSpace(department))
        {
            return new List<User>();
        }

        lock (_lockObject)
        {
            return _users
                .Where(u => u.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    /// <summary>
    /// Checks if a user with the given email exists
    /// </summary>
    public bool UserEmailExists(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        lock (_lockObject)
        {
            return _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}
