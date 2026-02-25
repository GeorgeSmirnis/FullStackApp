var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Configure CORS to allow Blazor WebAssembly client to call this API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        // For development: Allow any origin, method, and header
        // NOTE: In production, replace with specific origins:
        //   policy.WithOrigins("https://yourdomain.com")
        policy.AllowAnyOrigin()          // Allow requests from any origin
              .AllowAnyMethod()          // Allow GET, POST, PUT, DELETE, etc.
              .AllowAnyHeader();         // Allow any HTTP headers (Content-Type, etc.)
    });
});

// Add response caching services for performance optimization
builder.Services.AddResponseCaching();

var app = builder.Build();

// Configure the HTTP request pipeline
// Apply CORS policy to all endpoints
app.UseCors("AllowBlazor");

// Enable response caching middleware for HTTP caching support
app.UseResponseCaching();

// Static product data - generated once at startup for performance
// In production, this would come from a database
var cachedProductList = new[]
{
    new
    {
        Id = 1,
        Name = "Laptop",
        Price = 1200.50,
        Stock = 25,
        Category = new { Id = 101, Name = "Electronics", Description = "Computing Devices" }
    },
    new
    {
        Id = 2,
        Name = "Headphones",
        Price = 50.00,
        Stock = 100,
        Category = new { Id = 102, Name = "Accessories", Description = "Audio Equipment" }
    },
    new
    {
        Id = 3,
        Name = "Keyboard",
        Price = 129.99,
        Stock = 50,
        Category = new { Id = 101, Name = "Electronics", Description = "Computing Devices" }
    },
    new
    {
        Id = 4,
        Name = "Monitor",
        Price = 299.99,
        Stock = 15,
        Category = new { Id = 101, Name = "Electronics", Description = "Computing Devices" }
    },
    new
    {
        Id = 5,
        Name = "Mouse",
        Price = 29.99,
        Stock = 200,
        Category = new { Id = 102, Name = "Accessories", Description = "Input Devices" }
    },
    new
    {
        Id = 6,
        Name = "USB-C Cable",
        Price = 15.99,
        Stock = 150,
        Category = new { Id = 102, Name = "Accessories", Description = "Cables and Connectors" }
    },
    new
    {
        Id = 7,
        Name = "Laptop Stand",
        Price = 49.99,
        Stock = 40,
        Category = new { Id = 103, Name = "Peripherals", Description = "Laptop Accessories" }
    }
};

// Products API endpoint - Optimized with caching headers
app.MapGet("/api/productlist", (HttpContext context) =>
{
    // Return product data as JSON with nested category information
    try
    {
        // Set caching headers for HTTP-level caching (5 minutes)
        context.Response.Headers.CacheControl = "public, max-age=300";
        context.Response.Headers.Add("Pragma", "cache");
        
        // Return cached data - no need to reconstruct the object each time
        return Results.Ok(cachedProductList);
    }
    catch (Exception ex)
    {
        // Return error response if something goes wrong
        return Results.BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
    }
})
.WithName("GetProductList")
.WithOpenApi()
.Produces(200)
.Produces(400)
.WithSummary("Get all products with category information")
.WithDescription("Returns a list of products including nested category objects. Supports filtering by category in future versions.");

app.Run();
