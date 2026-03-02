using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SafeVault.Models;

namespace SafeVault.Services
{
    /// <summary>
    /// Secure user database service using parameterized queries to prevent SQL injection.
    /// All database operations use parameterized statements with placeholders.
    /// This ensures user input is never interpreted as SQL code.
    /// </summary>
    public class UserService
    {
        private readonly string _connectionString;
        private readonly InputValidationService _validationService;

        public UserService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _validationService = new InputValidationService();
        }

        /// <summary>
        /// Creates a new user with input validation and parameterized query.
        /// Uses SqlParameter to prevent SQL injection attacks.
        /// </summary>
        /// <param name="username">The username (will be validated)</param>
        /// <param name="email">The email (will be validated)</param>
        /// <returns>True if user was created successfully</returns>
        public bool CreateUser(string username, string email)
        {
            // Step 1: Validate input
            if (!_validationService.ValidateUserInput(username, email))
                throw new ArgumentException("Invalid username or email format");

            // Step 2: Sanitize input
            string sanitizedUsername = _validationService.SanitizeInput(username);
            string sanitizedEmail = _validationService.SanitizeInput(email);

            // Step 3: Execute parameterized query
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    // Using parameterized query - @Username and @Email are placeholders
                    // User input is passed separately, never concatenated into the SQL string
                    string query = "INSERT INTO Users (Username, Email) VALUES (@Username, @Email)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters - the value is kept separate from SQL syntax
                        command.Parameters.AddWithValue("@Username", sanitizedUsername);
                        command.Parameters.AddWithValue("@Email", sanitizedEmail);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Database operation failed", ex);
            }
        }

        /// <summary>
        /// Retrieves a user by username using parameterized query.
        /// Safe from SQL injection because username is passed as a parameter, not concatenated.
        /// </summary>
        /// <param name="username">The username to search for</param>
        /// <returns>User object if found, null otherwise</returns>
        public User GetUserByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    // Parameterized query with @Username placeholder
                    string query = "SELECT UserID, Username, Email FROM Users WHERE Username = @Username";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Pass username as parameter - it will be properly escaped
                        command.Parameters.AddWithValue("@Username", username);

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserID = (int)reader["UserID"],
                                    Username = reader["Username"].ToString(),
                                    Email = reader["Email"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Database query failed", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves a user by email using parameterized query.
        /// </summary>
        /// <param name="email">The email to search for</param>
        /// <returns>User object if found, null otherwise</returns>
        public User GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    // Parameterized query - @Email is a placeholder
                    string query = "SELECT UserID, Username, Email FROM Users WHERE Email = @Email";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserID = (int)reader["UserID"],
                                    Username = reader["Username"].ToString(),
                                    Email = reader["Email"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Database query failed", ex);
            }

            return null;
        }

        /// <summary>
        /// Searches users by username pattern using parameterized query.
        /// Even with wildcard search, parameterized queries prevent SQL injection.
        /// </summary>
        /// <param name="usernamePattern">The pattern to search for</param>
        /// <returns>List of matching users</returns>
        public List<User> SearchUsersByUsername(string usernamePattern)
        {
            var users = new List<User>();

            if (string.IsNullOrWhiteSpace(usernamePattern))
                return users;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    // LIKE with parameter - the pattern is still a parameter, not concatenated SQL
                    string query = "SELECT UserID, Username, Email FROM Users WHERE Username LIKE @Pattern";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Important: Even LIKE wildcards come from the parameter value, not the SQL string
                        command.Parameters.AddWithValue("@Pattern", "%" + usernamePattern + "%");

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new User
                                {
                                    UserID = (int)reader["UserID"],
                                    Username = reader["Username"].ToString(),
                                    Email = reader["Email"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Database query failed", ex);
            }

            return users;
        }

        /// <summary>
        /// Updates user email with multiple parameterized values.
        /// Shows how to use multiple parameters securely.
        /// </summary>
        /// <param name="userID">The user ID to update</param>
        /// <param name="newEmail">The new email address</param>
        /// <returns>True if update was successful</returns>
        public bool UpdateUserEmail(int userID, string newEmail)
        {
            if (!_validationService.ValidateEmail(newEmail))
                throw new ArgumentException("Invalid email format");

            string sanitizedEmail = _validationService.SanitizeInput(newEmail);

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    // Two parameters: @Email for the value, @UserID for the WHERE clause
                    string query = "UPDATE Users SET Email = @Email WHERE UserID = @UserID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", sanitizedEmail);
                        command.Parameters.AddWithValue("@UserID", userID);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Update operation failed", ex);
            }
        }

        /// <summary>
        /// Deletes a user by ID using parameterized query.
        /// </summary>
        /// <param name="userID">The user ID to delete</param>
        /// <returns>True if deletion was successful</returns>
        public bool DeleteUser(int userID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "DELETE FROM Users WHERE UserID = @UserID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userID);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Delete operation failed", ex);
            }
        }
    }
}
