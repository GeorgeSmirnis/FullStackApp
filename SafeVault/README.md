# SafeVault - Secure Web Application Security Implementation

## Overview
SafeVault is a demonstration application implementing comprehensive security measures to protect against common web vulnerabilities including SQL Injection and Cross-Site Scripting (XSS) attacks.

## Project Structure

```
SafeVault/
├── Services/
│   ├── InputValidationService.cs      # Input validation and sanitization
│   └── UserService.cs                 # Secure database access with parameterized queries
├── Models/
│   └── User.cs                        # User data model
├── Tests/
│   ├── TestInputValidation.cs         # Comprehensive input validation tests
│   ├── TestSQLInjection.cs            # SQL injection vulnerability tests
│   └── TestSecureDataAccess.cs        # Secure data access pattern tests
├── wwwroot/
│   └── webform.html                   # Secure web form with client-side validation
├── database.sql                       # Database schema with security best practices
└── README.md                          # This file
```

## Security Features Implemented

### 1. Input Validation Service (`InputValidationService.cs`)

**Purpose:** Validates and sanitizes all user inputs before processing.

**Key Methods:**
- `ValidateUsername()` - Validates username format (alphanumeric, underscore, hyphen; 3-50 chars)
- `ValidateEmail()` - Validates email using RFC 5322 pattern
- `SanitizeInput()` - Removes dangerous HTML/script patterns
- `ValidateLength()` - Prevents buffer overflow attacks
- `HtmlEncode()` - Escapes HTML special characters for safe display
- `ValidateUserInput()` - Comprehensive validation combining all checks

**Attack Prevention:**
- **XSS Prevention:** Removes `<script>`, `javascript:`, event handlers (`onerror`, `onload`, etc.), `<iframe>`, `<object>`, `<embed>`, `<svg>`
- **Input Validation:** Strict regex patterns limit what characters are accepted
- **Length Limits:** Prevents resource exhaustion and buffer overflow attempts

**Example:**
```csharp
var validator = new InputValidationService();

// Validate before using in database
if (validator.ValidateUserInput(username, email))
{
    // Safe to use in database query
    string sanitized = validator.SanitizeInput(username);
}
```

### 2. Secure User Service (`UserService.cs`)

**Purpose:** Implements all database operations using parameterized queries to prevent SQL injection.

**Key Methods:**
- `CreateUser()` - Validates, sanitizes, then inserts user
- `GetUserByUsername()` - Retrieves user by username
- `GetUserByEmail()` - Retrieves user by email
- `SearchUsersByUsername()` - Searches with LIKE pattern (still parameterized)
- `UpdateUserEmail()` - Updates user email securely
- `DeleteUser()` - Deletes user by ID

**SQL Injection Prevention:**
All methods use **parameterized queries** with SqlParameter placeholders:

```csharp
// SECURE: Parameter is separate from SQL syntax
string query = "INSERT INTO Users (Username, Email) VALUES (@Username, @Email)";
command.Parameters.AddWithValue("@Username", sanitizedUsername);
command.Parameters.AddWithValue("@Email", sanitizedEmail);

// VULNERABLE (example of what NOT to do):
// string query = $"INSERT INTO Users (Username, Email) VALUES ('{username}', '{email}')";
// This allows SQL injection!
```

**Defense Layers:**
1. **Input Validation** - Rejects invalid format before query
2. **Input Sanitization** - Removes dangerous characters
3. **Parameterized Queries** - Separates SQL syntax from data
4. **Error Handling** - Doesn't expose raw SQL errors

### 3. Web Form (`webform.html`)

**Features:**
- HTML5 input validation (`minlength`, `maxlength`, `pattern`, `type="email"`)
- Client-side JavaScript validation before submission
- Security information for users
- Safe form submission with error handling
- NO inline script attributes (preventing DOM XSS)

**Security Attributes:**
```html
<input 
    type="text" 
    id="username" 
    name="username"
    minlength="3"
    maxlength="50"
    pattern="[a-zA-Z0-9_-]{3,50}"
    required
>
```

## Testing Strategy

### Test Coverage: 80+ Security Test Cases

#### 1. **Input Validation Tests** (`TestInputValidation.cs`)
- Valid username/email formats
- Invalid formats, lengths, special characters
- XSS attack simulations (script tags, event handlers, iframe injection)
- HTML encoding tests

**Example Test:**
```csharp
[Test]
public void TestXSSAttack_AlertScript()
{
    string xssAttempt = "<script>alert('XSS Attack')</script>";
    string sanitized = _validationService.SanitizeInput(xssAttempt);
    
    Assert.IsFalse(sanitized.Contains("<script>"), "Script injection should be prevented");
}
```

#### 2. **SQL Injection Tests** (`TestSQLInjection.cs`)
- Union-based injection: `admin' UNION SELECT ...`
- Boolean-based injection: `admin' OR '1'='1`
- Time-based blind injection: `'; WAITFOR DELAY '00:00:05'`
- Stacked queries: `'; DROP TABLE Users;`
- Second-order injection
- Exception-based injection
- Comment-based injection
- Hex-encoded injection
- Wildcard injection
- Null byte injection
- Case variation bypass attempts

**Example Test:**
```csharp
[Test]
public void TestSQLInjection_UnionBasedInjection()
{
    string maliciousInput = "admin' UNION SELECT username, password FROM admin_users --";
    
    bool validationFails = !_validationService.ValidateUsername(maliciousInput);
    
    Assert.IsTrue(validationFails, "SQL injection attempt should fail validation");
}
```

#### 3. **Secure Data Access Tests** (`TestSecureDataAccess.cs`)
- Database input validation
- Parameterized query usage
- Multiple parameter handling
- Wildcard safety
- Attack vector demonstrations
- Defense-in-depth testing
- Valid data access patterns
- Error handling

**Example Test:**
```csharp
[Test]
public void TestParameterizedQuery_PreventsLogicManipulation()
{
    string attackInput = "' OR '1'='1";
    
    bool validationPasses = _validationService.ValidateUsername(attackInput);
    
    Assert.IsFalse(validationPasses, "Logic manipulation attempt should fail");
}
```

## Running the Tests

### With NUnit
```bash
# Install NUnit
dotnet add package NUnit
dotnet add package NUnit3TestAdapter

# Run all tests
dotnet test

# Run specific test class
dotnet test --filter ClassName=SafeVault.Tests.TestSQLInjection

# Run with verbose output
dotnet test --verbosity detailed
```

## Defense in Depth Architecture

The application uses multiple layers of security:

```
┌─────────────────────────────────────────────────┐
│        User Input (Web Form)                    │
└────────────────────┬────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────┐
│    Client-Side Validation (HTML5 + JS)          │
│  - Format validation                            │
│  - Length checks                                │
│  - Real-time malicious character detection      │
└────────────────────┬────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────┐
│    Server-Side Input Validation                 │
│  - Regex pattern validation                     │
│  - Length validation                            │
│  - Format verification                          │
└────────────────────┬────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────┐
│    Input Sanitization                           │
│  - Remove script tags                           │
│  - Remove event handlers                        │
│  - Remove dangerous protocols                   │
└────────────────────┬────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────┐
│    Parameterized Query Execution                │
│  - SQL separated from data                      │
│  - Placeholders for parameters                  │
│  - No string concatenation                      │
└────────────────────┬────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────┐
│        Database (SQL Server)                    │
│  - Constraints and triggers                     │
│  - Audit logging                                │
│  - Access controls                              │
└─────────────────────────────────────────────────┘
```

## Common Attack Vectors and How They're Prevented

### SQL Injection Attack
**Attack:** `username: admin' --; password: any`
**Prevention:** 
1. Input validation fails (invalid characters)
2. Parameterized query treats entire string as literal value
3. Database receives: `Username = "admin' --"` (literal string, not SQL)

### XSS Attack
**Attack:** `<script>alert('Hacked')</script>`
**Prevention:**
1. Validation fails (invalid characters)
2. Sanitization removes `<script>` tags
3. HTML encoding escapes remaining special characters
4. Browser receives HTML-encoded content, displays as text

### UNION-Based SQL Injection
**Attack:** `username: admin' UNION SELECT * FROM Users --`
**Prevention:**
1. Single quotes, UNION, and space characters fail validation
2. Input rejected before reaching database
3. If somehow reached DB, parameterized query treats as literal string

## Database Security Schema

The included `database.sql` file demonstrates:
- Proper table design with constraints
- Unique constraints on sensitive fields (username, email)
- Indexes for query performance
- Audit logging table for tracking changes
- Secure stored procedures with parameter usage
- Views for principle of least privilege
- Timestamp tracking for all modifications

## Configuration and Deployment

### appsettings.json Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SafeVault;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Production Recommendations
1. **Use encrypted connections** (SQL Server with SSL)
2. **Implement database-level encryption** (Transparent Data Encryption)
3. **Apply principle of least privilege** to database accounts
4. **Enable SQL Audit logging** for compliance
5. **Implement rate limiting** on form submissions
6. **Use HTTPS** for all communications
7. **Implement Content Security Policy (CSP)** headers
8. **Add CORS** if needed, but keep it restrictive
9. **Implement CSRF protection** with tokens
10. **Regular security updates** and patch management

## Code Examples

### Example 1: Safe User Creation
```csharp
var validator = new InputValidationService();
var userService = new UserService(connectionString);

// User input
string username = userInputUsername;
string email = userInputEmail;

// Step 1: Validate
if (!validator.ValidateUserInput(username, email))
{
    return BadRequest("Invalid input");
}

// Step 2: Service handles sanitization and parameterized query
var success = userService.CreateUser(username, email);
```

### Example 2: Safe User Lookup
```csharp
var userService = new UserService(connectionString);

// Even if user input is malicious, parameterized query protects:
User user = userService.GetUserByUsername(userInput);
// userInput is treated as a literal string, not SQL code
```

### Example 3: Safe Search
```csharp
var userService = new UserService(connectionString);

// Safe wildcard search with parameterized query
var results = userService.SearchUsersByUsername(searchPattern);
// Pattern is passed as parameter, preventing injection
```

## Testing Checklist

- [x] Username validation tests (valid, invalid, edge cases)
- [x] Email validation tests (valid, invalid formats)
- [x] Input sanitization tests (XSS removal)
- [x] SQL injection tests (10+ attack patterns)
- [x] Length validation tests
- [x] Parameterized query tests
- [x] Error handling tests
- [x] Valid data access patterns
- [x] Defense-in-depth tests

## Next Steps (Activity 2)

This foundation will be expanded with:
1. **Authentication System** - Secure login with password hashing (bcrypt/PBKDF2)
2. **Authorization System** - Role-based access control (RBAC)
3. **JWT Tokens** - Secure session management
4. **Password Reset** - Secure token-based password recovery
5. **Two-Factor Authentication** - Additional security layer

## Security References

- OWASP Top 10: https://owasp.org/Top10/
- SQL Injection Prevention: https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html
- XSS Prevention: https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html
- Input Validation: https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html
- Parameterized Queries: https://microsoft.github.io/ParamQuery/

## License
Educational material for security awareness and training.

## Author
SafeVault Security Demonstration - 2024
