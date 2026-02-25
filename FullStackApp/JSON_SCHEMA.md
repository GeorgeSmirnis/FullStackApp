# InventoryHub JSON Structure Documentation

## Overview
This document describes the JSON structure used by the InventoryHub API for product data responses. The API returns properly nested JSON that includes product details and category information.

## API Endpoint

**URL**: `GET http://localhost:5028/api/productlist`

**Method**: HTTP GET

**Response Type**: JSON (application/json)

**Status Codes**:
- `200 OK`: Successfully retrieved product list
- `400 Bad Request`: Server error occurred

---

## JSON Response Structure

### Root Level
The API returns an array of product objects:

```json
[
  { /* Product Object 1 */ },
  { /* Product Object 2 */ },
  { /* Product Object 3 */ }
]
```

### Product Object Schema

Each product object contains the following properties:

```json
{
  "id": 1,
  "name": "Laptop",
  "price": 1200.50,
  "stock": 25,
  "category": {
    "id": 101,
    "name": "Electronics",
    "description": "Computing Devices"
  }
}
```

#### Product Properties

| Property | Type | Description | Example |
|----------|------|-------------|---------|
| `id` | integer | Unique product identifier | `1` |
| `name` | string | Product name | `"Laptop"` |
| `price` | number | Product price in USD | `1200.50` |
| `stock` | integer | Number of units in stock | `25` |
| `category` | object | Nested category object | See below |

#### Category Object

| Property | Type | Required | Description | Example |
|----------|------|----------|-------------|---------|
| `id` | integer | Yes | Category identifier | `101` |
| `name` | string | Yes | Category name | `"Electronics"` |
| `description` | string | No | Category description | `"Computing Devices"` |

---

## Sample API Response (Full)

```json
[
  {
    "id": 1,
    "name": "Laptop",
    "price": 1200.50,
    "stock": 25,
    "category": {
      "id": 101,
      "name": "Electronics",
      "description": "Computing Devices"
    }
  },
  {
    "id": 2,
    "name": "Headphones",
    "price": 50.00,
    "stock": 100,
    "category": {
      "id": 102,
      "name": "Accessories",
      "description": "Audio Equipment"
    }
  },
  {
    "id": 3,
    "name": "Keyboard",
    "price": 129.99,
    "stock": 50,
    "category": {
      "id": 101,
      "name": "Electronics",
      "description": "Computing Devices"
    }
  },
  {
    "id": 4,
    "name": "Monitor",
    "price": 299.99,
    "stock": 15,
    "category": {
      "id": 101,
      "name": "Electronics",
      "description": "Computing Devices"
    }
  },
  {
    "id": 5,
    "name": "Mouse",
    "price": 29.99,
    "stock": 200,
    "category": {
      "id": 102,
      "name": "Accessories",
      "description": "Input Devices"
    }
  },
  {
    "id": 6,
    "name": "USB-C Cable",
    "price": 15.99,
    "stock": 150,
    "category": {
      "id": 102,
      "name": "Accessories",
      "description": "Cables and Connectors"
    }
  },
  {
    "id": 7,
    "name": "Laptop Stand",
    "price": 49.99,
    "stock": 40,
    "category": {
      "id": 103,
      "name": "Peripherals",
      "description": "Laptop Accessories"
    }
  }
]
```

---

## C# Model Classes

The C# models used for JSON deserialization:

### Product Class
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public int Stock { get; set; }
    public Category? Category { get; set; }
}
```

### Category Class
```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
```

---

## JSON Validation

### Validation Rules

1. **Array Structure**: Response must be a JSON array
2. **Product Objects**: Each array element must be a product object
3. **Required Fields**: Every product must have `id`, `name`, `price`, and `stock`
4. **Category Nesting**: Each product must have a nested `category` object
5. **Category Fields**: Category must have `id` and `name` (description is optional)
6. **Data Types**:
   - `id` (product): integer > 0
   - `name`: non-empty string
   - `price`: number >= 0.00 (typically 2 decimal places)
   - `stock`: integer >= 0
   - `category.id`: integer > 0
   - `category.name`: non-empty string
   - `category.description`: string (optional)

### JSON Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "array",
  "items": {
    "type": "object",
    "properties": {
      "id": {
        "type": "integer",
        "minimum": 1
      },
      "name": {
        "type": "string",
        "minLength": 1
      },
      "price": {
        "type": "number",
        "minimum": 0
      },
      "stock": {
        "type": "integer",
        "minimum": 0
      },
      "category": {
        "type": "object",
        "properties": {
          "id": {
            "type": "integer",
            "minimum": 1
          },
          "name": {
            "type": "string",
            "minLength": 1
          },
          "description": {
            "type": ["string", "null"]
          }
        },
        "required": ["id", "name"]
      }
    },
    "required": ["id", "name", "price", "stock", "category"]
  }
}
```

---

## Testing the API Response

### Using PowerShell

```powershell
# Basic request
$response = Invoke-WebRequest -Uri "http://localhost:5028/api/productlist"
$response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10 | Out-String

# Pretty print
$data = Invoke-WebRequest -Uri "http://localhost:5028/api/productlist" | ConvertFrom-Json
$data | Format-Table Id, Name, @{Label="Category";Expression={$_.category.name}}, Price, Stock
```

### Using curl

```bash
# Linux/Mac
curl http://localhost:5028/api/productlist | jq .

# Windows with jq
curl http://localhost:5028/api/productlist | jq .

# Without jq (basic)
curl http://localhost:5028/api/productlist
```

### Using Postman

1. Create new GET request
2. URL: `http://localhost:5028/api/productlist`
3. Send
4. Response body should show JSON with 7 products
5. Verify Status: 200 OK
6. Check nested category objects in each product

### Using Browser DevTools

1. Open browser Developer Tools (F12)
2. Navigate to `/api/productlist` directly
3. Browser displays formatted JSON
4. Check Network tab for response headers

---

## JSON Structure Validation Tools

### Online Validators

- **JSONLint**: https://jsonlint.com/ - Validates JSON syntax
- **JSON Schema Validator**: https://www.jsonschemavalidator.net/ - Validates against schema
- **Beeceptor**: https://beeceptor.com/ - Mock API testing

### Visual Studio Code Extensions

- **REST Client**: Send requests directly from VS Code
- **Thunder Client**: Built-in REST client
- **JSON Tools**: Validate and format JSON

---

## Common JSON Issues and Solutions

### Issue 1: Case Sensitivity
**Problem**: JSON property names are different case than C# model
**Solution**: 
```csharp
var options = new System.Text.Json.JsonSerializerOptions 
{ 
    PropertyNameCaseInsensitive = true 
};
var products = System.Text.Json.JsonSerializer.Deserialize<Product[]>(json, options);
```

### Issue 2: Null Category
**Problem**: Category is null or missing
**Solution**: Product model has nullable Category: `public Category? Category { get; set; }`

### Issue 3: Array Expected, Got Object
**Problem**: API returns single object instead of array
**Solution**: API must return `new[] { ... }` or `new List<T>() { ... }`

### Issue 4: Extra Properties in JSON
**Problem**: API response includes fields not in C# model
**Solution**: Extra fields are ignored by default (no error needed)

### Issue 5: Missing Required Properties
**Problem**: JSON is missing a required property
**Solution**: Deserialization throws JsonException with detailed message

---

## Integration with Blazor Frontend

### How the Frontend Consumes This JSON

```csharp
// In FetchProducts.razor
private async Task LoadProducts()
{
    // Call API
    var apiUrl = "http://localhost:5028/api/productlist";
    
    // GetFromJsonAsync automatically deserializes the JSON
    products = await HttpClient.GetFromJsonAsync<Product[]>(apiUrl);
    
    // Now access nested category data
    foreach (var product in products)
    {
        var categoryName = product.Category?.Name; // "Electronics", "Accessories", etc.
        var categoryDesc = product.Category?.Description;
    }
}
```

### Displaying Nested Data in UI

```html
<!-- Display category in table -->
@foreach (var product in products)
{
    <tr>
        <td>@product.Name</td>
        <td>
            @if (product.Category != null)
            {
                <span>@product.Category.Name</span>
                <small>@product.Category.Description</small>
            }
        </td>
    </tr>
}
```

---

## Performance Considerations

1. **Response Size**: Current response is approximately 1.2 KB
2. **Deserialization Speed**: < 1ms on modern hardware
3. **Network Time**: Typically 10-50ms on localhost
4. **Caching**: Consider caching at application level for frequently accessed data

### Optimization Tips

1. **Pagination**: For large datasets, implement page-based pagination
2. **Filtering**: Add query parameters to filter by category
3. **Compression**: Enable gzip compression for API responses
4. **Caching Headers**: Add cache control headers for stable data

---

## Future Enhancements

### Planned Improvements

1. **Filtering**: `GET /api/productlist?categoryId=101`
2. **Sorting**: `GET /api/productlist?sortBy=price&order=asc`
3. **Pagination**: `GET /api/productlist?page=1&pageSize=10`
4. **Search**: `GET /api/productlist?search=laptop`
5. **Include Relations**: `GET /api/productlist/1` (single product with full details)

### Error Response Structure

```json
{
  "error": "Error message describing what went wrong",
  "timestamp": "2026-02-25T10:30:00Z"
}
```

---

## Conclusion

The InventoryHub API uses a well-structured JSON format with:
- ✅ Nested category objects
- ✅ Clear property naming conventions
- ✅ Proper data types
- ✅ Null-safe optional properties
- ✅ Support for future enhancements

The JSON structure follows RESTful API best practices and is compatible with the Blazor frontend for seamless data display and interactions.
