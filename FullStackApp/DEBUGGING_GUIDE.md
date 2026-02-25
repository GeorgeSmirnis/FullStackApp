# InventoryHub - Integration Debugging Guide

## Overview
This document details the debugging process used to resolve integration issues between the Blazor front-end and ASP.NET Core Minimal API back-end.

## Issues Identified and Resolved

### Issue 1: Incorrect API Route Mismatch
**Problem**: The front-end was calling `/api/products`, but the back-end was updated to `/api/productlist`.

**Symptoms**:
- 404 Not Found errors in browser console
- Network tab showing failed API requests
- "Failed to connect to the API" error message displayed to users

**Solution**:
1. Updated the API endpoint in `ServerApp/Program.cs`:
   ```csharp
   // OLD: app.MapGet("/api/products", () => { ... })
   // NEW: app.MapGet("/api/productlist", () => { ... })
   ```

2. Updated the API URL in `ClientApp/Components/Pages/FetchProducts.razor`:
   ```csharp
   // OLD: var apiUrl = "http://localhost:5028/api/products";
   // NEW: var apiUrl = "http://localhost:5028/api/productlist";
   ```

**Verification**:
- Test the endpoint directly: `http://localhost:5028/api/productlist`
- Should return JSON array of 5 products

---

### Issue 2: CORS Restrictions Blocking API Access
**Problem**: Security restrictions prevented the front-end from accessing the back-end API.

**Symptoms**:
- Browser console error: "Access to XMLHttpRequest has been blocked by CORS policy"
- Network tab shows CORS preflight request (OPTIONS) failing
- Products fail to load even if API path is correct

**Root Cause**:
Blazor WebAssembly runs on one origin (localhost:5273) while the API runs on another (localhost:5028). Cross-origin requests require explicit CORS configuration.

**Solution**:
Implemented CORS policy in `ServerApp/Program.cs`:

```csharp
// Add CORS service to dependency injection container
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.AllowAnyOrigin()      // Allow requests from any origin
              .AllowAnyMethod()      // Allow all HTTP methods
              .AllowAnyHeader();     // Allow all headers
    });
});

// Apply the CORS policy to all endpoints
app.UseCors("AllowBlazor");
```

**Key CORS Configuration Elements**:
- **AllowAnyOrigin()**: Permits requests from any domain (development only)
- **AllowAnyMethod()**: Allows GET, POST, PUT, DELETE, PATCH, etc.
- **AllowAnyHeader()**: Allows custom headers like Content-Type, Authorization, etc.

**Production Considerations**:
```csharp
// For production, use specific origins instead:
policy.WithOrigins("https://yourdomain.com", "https://app.yourdomain.com")
      .AllowAnyMethod()
      .AllowAnyHeader();
```

**Verification**:
1. Open browser DevTools (F12)
2. Go to Network tab
3. Click "Products" in navigation
4. Look for the API request:
   - No CORS errors in console
   - Request completes with 200 status
   - Response tab shows JSON data

---

### Issue 3: Malformed JSON Response Handling
**Problem**: A back-end developer accidentally modified the API response structure, breaking front-end deserialization.

**Symptoms**:
- Console error: "JsonException: The JSON value could not be converted to System.Product[]"
- Products fail to display
- Error message: "Failed to parse the API response"

**Solution**:
Enhanced error handling in `FetchProducts.razor` with fallback deserialization:

```csharp
private async Task LoadProducts()
{
    try
    {
        // PRIMARY: Try using GetFromJsonAsync with built-in deserialization
        products = await HttpClient.GetFromJsonAsync<Product[]>(apiUrl, cts.Token);
    }
    catch (System.Text.Json.JsonException jsonEx)
    {
        // FALLBACK: If JSON deserialization fails, manually handle the response
        Console.WriteLine($"JSON Deserialization Error: {jsonEx.Message}");
        
        var response = await HttpClient.GetAsync(apiUrl, cts.Token);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        
        // Log raw JSON for debugging
        Console.WriteLine($"Raw JSON response: {json}");
        
        // Attempt deserialization with case-insensitive property matching
        var options = new System.Text.Json.JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        };
        products = System.Text.Json.JsonSerializer.Deserialize<Product[]>(json, options);
    }
    // ... additional catch blocks for other exception types ...
}
```

**Debugging Features Added**:
1. **Console Logging**: `Console.WriteLine()` statements log:
   - API URL being called
   - Number of successfully loaded products
   - Raw JSON response
   - Specific error types and messages

2. **Case-Insensitive Deserialization**: Handles API responses where property names have different casing
   - JSON: `{ "id": 1, "name": "Laptop" }`
   - C#: `{ Id = 1, Name = "Laptop" }`

3. **Detailed Error Information**: Each exception type is caught separately:
   - `TaskCanceledException`: Timeout handling
   - `HttpRequestException`: Connection failures
   - `JsonException`: Deserialization errors
   - `Exception`: Catch-all for unexpected errors

**Verification**:
1. Open browser DevTools (F12)
2. Go to Console tab
3. Click "Products"
4. Should see log messages:
   ```
   Attempting to fetch products from: http://localhost:5028/api/productlist
   Successfully loaded 5 products
   ```

---

## Testing the Resolved Integration

### Pre-Test Checklist
- ✅ ServerApp is running on `http://localhost:5028`
- ✅ ClientApp is running on `http://localhost:5273` (or similar)
- ✅ Both applications have been rebuilt after code changes
- ✅ Browser cache is cleared or F5 hard refresh used

### Test Steps

#### 1. Test the API Endpoint Directly
```powershell
# In any terminal or PowerShell:
curl http://localhost:5028/api/productlist

# OR visit in browser:
# http://localhost:5028/api/productlist
```

**Expected Result**: JSON array with 5 products
```json
[
  {
    "id": 1,
    "name": "Laptop",
    "price": 1200.5,
    "stock": 25
  },
  // ... 4 more products
]
```

#### 2. Test the Client Application
1. Open `http://localhost:5273` (or the actual ClientApp port)
2. Click "Products" in navigation menu
3. Verify the following:

   | Check | Expected | Status |
   |-------|----------|--------|
   | Page loads without errors | Product table visible | ✓ |
   | Browser console (F12) | No CORS errors | ✓ |
   | Network tab | GET /api/productlist shows 200 | ✓ |
   | Product count | Exactly 5 products | ✓ |
   | Product data | Names, prices, stock visible | ✓ |
   | Stock badges | Green (>20), Yellow (0-20), Red (0) | ✓ |
   | Refresh button | Reloads products without error | ✓ |

#### 3. Test Error Handling
Simulate error scenarios to verify error handling:

**Scenario A: Server Down**
1. Stop the ServerApp
2. Click "Products" in ClientApp
3. Expected: Error message appears

**Scenario B: Malformed JSON** (Advanced Testing)
1. Temporarily modify API response to return invalid JSON
2. Products should still load (fallback parsing) or show detailed error

**Scenario C: Timeout**
1. Artificially delay the API response
2. After 10 seconds, timeout error should appear

---

## Debugging Browser Console Logs

### Successful Load
```javascript
Attempting to fetch products from: http://localhost:5028/api/productlist
Successfully loaded 5 products
```

### CORS Error
```javascript
Access to XMLHttpRequest at 'http://localhost:5028/api/productlist' from origin 'http://localhost:5273' 
has been blocked by CORS policy: No 'Access-Control-Allow-Origin' header is present on the requested resource.
```
**Solution**: Verify `app.UseCors("AllowBlazor")` is in ServerApp/Program.cs

### 404 Error
```javascript
HTTP Request Error: Response status code does not indicate success: 404 (Not Found).
```
**Solution**: Verify endpoint is `/api/productlist`, not `/api/products`

### JSON Parse Error
```javascript
JSON Parse Error: [error details about JSON structure]
```
**Solution**: Verify API response matches Product model structure

---

## Key Improvements Made

### 1. Error Handling
- ✅ Specific exception types for different failures
- ✅ Fallback deserialization for minor JSON format issues
- ✅ Case-insensitive property matching
- ✅ Console logging for debugging

### 2. CORS Configuration
- ✅ Proper CORS policy setup
- ✅ Comments explaining production considerations
- ✅ Support for future credential-based authentication

### 3. API Structure
- ✅ Consistent endpoint naming (`/api/productlist`)
- ✅ Error handling in API endpoint
- ✅ OpenAPI documentation support

### 4. Debugging Support
- ✅ Console.WriteLine() for tracking execution
- ✅ Detailed error messages
- ✅ Raw JSON response logging
- ✅ Request timeout implementation

---

## Common Pitfalls and Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| Products don't load | URL mismatch | Check both client and server use `/api/productlist` |
| CORS error in console | Missing CORS config | Ensure `app.UseCors("AllowBlazor")` is called |
| "Not Found" error | Wrong endpoint name | Verify endpoint in ServerApp/Program.cs |
| JSON parse error | Invalid JSON structure | Use fallback deserialization or log raw JSON |
| Timeout error | Server not running | Start ServerApp first: `dotnet run` |
| Refresh doesn't work | Button handler issue | Verify `@onclick="LoadProducts"` in Razor |

---

## Next Steps (Activity 3)

The integration is now stable. In Activity 3, you'll:
1. ✅ Ensure JSON structures match perfectly
2. ✅ Implement more complex JSON handling
3. ✅ Add filtering and search features
4. ✅ Implement pagination for large datasets

---

## Files Modified in This Activity

1. **ServerApp/Program.cs**
   - Changed endpoint from `/api/products` to `/api/productlist`
   - Enhanced CORS configuration with detailed comments
   - Added error handling wrapper in API endpoint

2. **ClientApp/Components/Pages/FetchProducts.razor**
   - Updated API URL to `/api/productlist`
   - Enhanced error handling with fallback deserialization
   - Added console logging for debugging
   - Implemented case-insensitive property matching

---

## Conclusion

All three critical issues have been resolved:
1. ✅ API route mismatch fixed
2. ✅ CORS restrictions properly configured
3. ✅ JSON deserialization error handling implemented

The application is now ready for Activity 3, where you'll work with more complex JSON structures and advanced features.
