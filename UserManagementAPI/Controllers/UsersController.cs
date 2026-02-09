using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers;

/// <summary>
/// API controller for managing users
/// Provides CRUD endpoints for user management with comprehensive error handling
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly UserValidationService _validationService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserService userService, UserValidationService validationService, ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// GET: api/users
    /// Retrieves all users with exception handling
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<User>> GetAllUsers()
    {
        try
        {
            _logger.LogInformation("Retrieving all users at {Timestamp}", DateTime.UtcNow);
            var users = _userService.GetAllUsers();
            _logger.LogInformation("Successfully retrieved {UserCount} users", users.Count);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all users at {Timestamp}", DateTime.UtcNow);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving users", error = ex.Message });
        }
    }

    /// <summary>
    /// GET: api/users/{id}
    /// Retrieves a specific user by ID with validation and exception handling
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<User> GetUserById(int id)
    {
        try
        {
            _logger.LogInformation("Attempting to retrieve user with ID: {UserId}", id);

            // Validate ID
            if (!_validationService.ValidateId(id))
            {
                _logger.LogWarning("Invalid user ID provided: {UserId}", id);
                return BadRequest(new { message = "Invalid user ID. ID must be a positive integer." });
            }

            var user = _userService.GetUserById(id);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound(new { message = $"User with ID {id} not found" });
            }

            _logger.LogInformation("Successfully retrieved user with ID: {UserId}", id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user with ID {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving the user", error = ex.Message });
        }
    }

    /// <summary>
    /// POST: api/users
    /// Creates a new user with comprehensive validation and exception handling
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<User> CreateUser([FromBody] User? user)
    {
        try
        {
            if (user == null)
            {
                _logger.LogWarning("Null user object provided for creation");
                return BadRequest(new { message = "User object cannot be null" });
            }

            _logger.LogInformation("Attempting to create new user: {FirstName} {LastName}", user.FirstName, user.LastName);

            // Create user with validation
            var (createdUser, errors) = _userService.CreateUser(user);

            if (createdUser == null)
            {
                _logger.LogWarning("User creation failed with validation errors: {@Errors}", errors);
                return BadRequest(new { message = "User creation failed", errors });
            }

            _logger.LogInformation("Successfully created user with ID: {UserId}, Email: {Email}", createdUser.Id, createdUser.Email);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating user");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error occurred while creating the user", error = ex.Message });
        }
    }

    /// <summary>
    /// PUT: api/users/{id}
    /// Updates an existing user with validation and exception handling
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<object> UpdateUser(int id, [FromBody] User? updatedUser)
    {
        try
        {
            _logger.LogInformation("Attempting to update user with ID: {UserId}", id);

            if (updatedUser == null)
            {
                _logger.LogWarning("Null user object provided for update");
                return BadRequest(new { message = "Updated user object cannot be null" });
            }

            // Validate ID
            if (!_validationService.ValidateId(id))
            {
                _logger.LogWarning("Invalid user ID provided for update: {UserId}", id);
                return BadRequest(new { message = "Invalid user ID. ID must be a positive integer." });
            }

            // Update user with validation
            var (success, errors) = _userService.UpdateUser(id, updatedUser);

            if (!success)
            {
                if (errors.Any(e => e.Contains("not found")))
                {
                    _logger.LogWarning("User with ID {UserId} not found for update", id);
                    return NotFound(new { message = $"User with ID {id} not found" });
                }

                _logger.LogWarning("User update failed with validation errors: {@Errors}", errors);
                return BadRequest(new { message = "User update failed", errors });
            }

            var user = _userService.GetUserById(id);
            _logger.LogInformation("Successfully updated user with ID: {UserId}", id);
            return Ok(new { message = "User updated successfully", user });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user with ID {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the user", error = ex.Message });
        }
    }

    /// <summary>
    /// DELETE: api/users/{id}
    /// Deletes a user by ID with validation and exception handling
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult DeleteUser(int id)
    {
        try
        {
            _logger.LogInformation("Attempting to delete user with ID: {UserId}", id);

            // Validate ID
            if (!_validationService.ValidateId(id))
            {
                _logger.LogWarning("Invalid user ID provided for deletion: {UserId}", id);
                return BadRequest(new { message = "Invalid user ID. ID must be a positive integer." });
            }

            var (success, errors) = _userService.DeleteUser(id);

            if (!success)
            {
                _logger.LogWarning("User deletion failed for ID {UserId}: {@Errors}", id, errors);
                return NotFound(new { message = $"User with ID {id} not found" });
            }

            _logger.LogInformation("Successfully deleted user with ID: {UserId}", id);
            return Ok(new { message = $"User with ID {id} deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user with ID {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting the user", error = ex.Message });
        }
    }

    /// <summary>
    /// GET: api/users/department/{department}
    /// Retrieves all users in a specific department with exception handling
    /// </summary>
    [HttpGet("department/{department}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<User>> GetUsersByDepartment(string? department)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(department))
            {
                _logger.LogWarning("Empty department name provided for search");
                return BadRequest(new { message = "Department name cannot be empty" });
            }

            _logger.LogInformation("Retrieving users from department: {Department}", department);
            var users = _userService.GetUsersByDepartment(department);
            _logger.LogInformation("Retrieved {UserCount} users from department {Department}", users.Count, department);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving users from department {Department}", department);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving users by department", error = ex.Message });
        }
    }
}
