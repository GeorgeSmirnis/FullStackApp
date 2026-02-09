using UserManagementAPI.Middleware;
using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register UserValidationService as a singleton for consistent validation
builder.Services.AddSingleton<UserValidationService>();

// Register UserService as a singleton for in-memory operations
builder.Services.AddSingleton<UserService>();

// Add OpenAPI/Swagger support for API documentation
builder.Services.AddOpenApi();

// Add CORS policy to allow requests from different origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add logging with console and debug output
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// ============================================================
// MIDDLEWARE PIPELINE CONFIGURATION
// ORDER MATTERS: Process in this order:
// 1. Error Handling (catches all exceptions)
// 2. Authentication (validates tokens before processing)
// 3. Request Logging (logs all requests/responses)
// 4. CORS & Authorization
// 5. Controllers/Endpoints
// ============================================================

// 1. ERROR HANDLING MIDDLEWARE - FIRST (catches all exceptions)
app.UseMiddleware<ErrorHandlingMiddleware>();

// 2. AUTHENTICATION MIDDLEWARE - SECOND (validates tokens)
app.UseMiddleware<AuthenticationMiddleware>();

// 3. REQUEST LOGGING MIDDLEWARE - THIRD (logs requests/responses)
app.UseMiddleware<RequestLoggingMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use HTTPS redirection
app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAll");

// Add authorization middleware
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Add a health check endpoint (excluded from authentication)
app.MapGet("/health", () => 
{
    return Results.Ok(new
    {
        status = "API is running",
        timestamp = DateTime.UtcNow,
        version = "3.0.0 (With Middleware)",
        environment = app.Environment.EnvironmentName
    });
})
.WithName("HealthCheck")
.WithOpenApi();

app.Run();
