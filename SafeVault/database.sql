-- SafeVault Database Schema
-- Secure schema for storing user data with appropriate constraints

-- Create Users table
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(100) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    CreatedDate DATETIME DEFAULT GETDATE(),
    LastModified DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);

-- Create index on Username for faster lookups
CREATE INDEX IX_Users_Username ON Users(Username);

-- Create index on Email for faster lookups
CREATE INDEX IX_Users_Email ON Users(Email);

-- Create an audit log table to track all changes (important for security)
CREATE TABLE AuditLog (
    AuditID INT PRIMARY KEY IDENTITY(1,1),
    Action VARCHAR(50) NOT NULL,
    TableName VARCHAR(50) NOT NULL,
    RecordID INT NOT NULL,
    OldValue VARCHAR(MAX),
    NewValue VARCHAR(MAX),
    ChangedBy VARCHAR(100),
    ChangedDate DATETIME DEFAULT GETDATE()
);

-- Create index on AuditLog for performance
CREATE INDEX IX_AuditLog_TableName ON AuditLog(TableName);

-- Create a view for non-sensitive data retrieval (principle of least privilege)
CREATE VIEW vw_UserList AS
SELECT UserID, Username, Email, CreatedDate
FROM Users
WHERE IsActive = 1;

-- Sample secure stored procedure for user creation
-- Note: This would be used alongside parameterized queries from application code
CREATE PROCEDURE usp_CreateUser
    @Username VARCHAR(100),
    @Email VARCHAR(100)
AS
BEGIN
    BEGIN TRY
        INSERT INTO Users (Username, Email)
        VALUES (@Username, @Email);
        
        -- Log the action
        INSERT INTO AuditLog (Action, TableName, RecordID, NewValue, ChangedBy)
        VALUES ('INSERT', 'Users', SCOPE_IDENTITY(), 
                'Username: ' + @Username + ', Email: ' + @Email, 'System');
        
        SELECT 'User created successfully' AS Message;
    END TRY
    BEGIN CATCH
        SELECT 'Error: ' + ERROR_MESSAGE() AS Message;
    END CATCH
END;

-- Secure stored procedure for user retrieval
CREATE PROCEDURE usp_GetUserByUsername
    @Username VARCHAR(100)
AS
BEGIN
    SELECT UserID, Username, Email, CreatedDate
    FROM Users
    WHERE Username = @Username
    AND IsActive = 1;
    
    -- Log the access
    INSERT INTO AuditLog (Action, TableName, RecordID, ChangedBy)
    VALUES ('SELECT', 'Users', 0, 'System');
END;

-- Secure stored procedure for user search
CREATE PROCEDURE usp_SearchUsers
    @SearchPattern VARCHAR(100)
AS
BEGIN
    -- Using LIKE with parameter (still secure with parameterized approach)
    SELECT UserID, Username, Email, CreatedDate
    FROM Users
    WHERE Username LIKE '%' + @SearchPattern + '%'
    AND IsActive = 1;
end;

-- Important database-level constraints for data integrity
-- These work alongside application-level validation

-- All tables should have timestamps for tracking modifications
-- All sensitive data should be encrypted at rest (not shown in this basic schema)
-- Access should be limited through database roles and permissions
