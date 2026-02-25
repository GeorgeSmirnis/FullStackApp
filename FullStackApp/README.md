# InventoryHub - Full Stack Integration Project

## Activity Status
- ✅ **Activity 1**: Initial Integration Implementation - COMPLETED
- ✅ **Activity 2**: Integration Debugging - COMPLETED
- ⏳ **Activity 3**: Advanced JSON Structures (Coming Next)
- ⏳ **Activity 4**: Performance Optimization (Coming Next)

## Getting Started

### Quick Links
- [Main README](README.md) - You are here
- [Debugging Guide](DEBUGGING_GUIDE.md) - Issues fixed in Activity 2
- [Testing Guide](TESTING_GUIDE.md) - How to test the application
- [API Documentation](#api-documentation) - Endpoint details

## Project Structure
```
FullStackApp/
├── FullStackSolution.sln         # Solution file
├── ClientApp/                     # Blazor WebAssembly project
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── FetchProducts.razor  # Product listing component
│   │   └── Layout/
│   │       └── NavMenu.razor        # Navigation with Products link
│   ├── Program.cs                 # Client configuration
│   └── wwwroot/                   # Static assets
└── ServerApp/                     # ASP.NET Core Web API project
    ├── Program.cs                 # API configuration with CORS and products endpoint
    └── Properties/
        └── launchSettings.json    # Port configuration (5028)
```

## Technology Stack
- **Front-End**: Blazor WebAssembly with Bootstrap for styling
- **Back-End**: ASP.NET Core Minimal API
- **Communication**: HTTP with JSON serialization
- **CORS**: Enabled for cross-origin requests from Blazor client

## Features Implemented

### 1. ServerApp API (Backend)
- **Endpoint**: `GET /api/products`
- **Response**: JSON array of product objects with Id, Name, Price, and Stock
- **CORS**: Configured to allow requests from any origin (ideal for development)
- **Sample Products**:
  - Laptop: $1200.50 (25 in stock)
  - Headphones: $50.00 (100 in stock)
  - Keyboard: $129.99 (50 in stock)
  - Monitor: $299.99 (15 in stock)
  - Mouse: $29.99 (200 in stock)

### 2. ClientApp Component (Frontend)
The `FetchProducts.razor` component provides:

#### Features:
- **Product Display**: Tabular view of all products with formatting
- **Stock Status Badges**:
  - Green badge: Stock > 20 units
  - Yellow badge: 0 < Stock ≤ 20 units
  - Red badge: Out of stock
- **Price Formatting**: Displays prices with 2 decimal places
- **Loading State**: Shows spinner while fetching data
- **Error Handling**: Comprehensive error messages for:
  - API timeouts (10-second timeout set)
  - Connection failures
  - JSON parsing errors
  - Unexpected exceptions
- **Refresh Functionality**: Button to manually refresh product list
- **Responsive Design**: Uses Bootstrap classes for mobile-friendly layout

## API Integration Details

### HttpClient Configuration
The integration uses HttpClient to call the API with:
- **Timeout**: 10 seconds (prevents hanging requests)
- **Error Handling**: Try-catch blocks for multiple exception types
- **JSON Deserialization**: Automatic conversion to Product class

### CORS Configuration
The ServerApp includes CORS middleware that allows:
- Any origin (great for development)
- All HTTP methods
- All headers

### Product Model
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
    public int Stock { get; set; }
}
```

## Getting Started

### Prerequisites
- .NET 10.0 SDK or later
- Visual Studio Code or Visual Studio 2022
- Node.js (optional, for additional tooling)

### Setup Instructions

1. **Navigate to the project root**:
   ```powershell
   cd C:\Carreer\microsoftCourse\FullStackApp
   ```

2. **Open the solution in VS Code**:
   ```powershell
   code .
   ```

3. **Restore dependencies** (automatic with `dotnet run`, but can be explicit):
   ```powershell
   dotnet restore
   ```

### Running the Applications

#### Option 1: Using VS Code Terminal (Recommended)
1. Open VS Code integrated terminal (Ctrl + `)
2. Split the terminal (Ctrl + Shift + Alt + \) to create two terminal panels
3. In the first terminal:
   ```powershell
   cd ServerApp
   dotnet run
   ```
   Expected output: "Now listening on: http://localhost:5028"

4. In the second terminal:
   ```powershell
   cd ClientApp
   dotnet watch run
   ```
   Expected output: Application running on a local port (usually http://localhost:5273)

#### Option 2: Using Separate Command Line Windows
```powershell
# Terminal 1 - ServerApp
cd C:\Carreer\microsoftCourse\FullStackApp\ServerApp
dotnet run

# Terminal 2 - ClientApp
cd C:\Carreer\microsoftCourse\FullStackApp\ClientApp
dotnet watch run
```

### Testing the Integration

1. **Test the API Endpoint**:
   - Open browser and navigate to: `http://localhost:5028/api/products`
   - You should see JSON data with 5 products

2. **Test the Blazor Client**:
   - Open the ClientApp URL (usually `http://localhost:5273`)
   - Click "Products" in the navigation menu
   - Verify that all 5 products are displayed in a table format

3. **Expected Output**:
   ```
   ID | Name       | Price  | Stock
   ---|------------|--------|--------
   1  | Laptop     | 1200.50| 25 (green)
   2  | Headphones | 50.00  | 100 (green)
   3  | Keyboard   | 129.99 | 50 (green)
   4  | Monitor    | 299.99 | 15 (yellow)
   5  | Mouse      | 29.99  | 200 (green)
   ```

## Error Scenarios

The FetchProducts component gracefully handles:

1. **Server Not Running**: 
   - Error: "Failed to connect to the API. Make sure the server is running on http://localhost:5028."

2. **Network Timeout**:
   - Error: "The request timed out. Please make sure the server is running on http://localhost:5028"

3. **Invalid JSON Response**:
   - Error: "Failed to parse the API response. The server may have returned invalid JSON."

4. **Other Exceptions**:
   - Error: "An unexpected error occurred: [exception message]"

## Code Quality Best Practices Implemented

✅ **Async/Await**: All I/O operations use async patterns  
✅ **Error Handling**: Multiple catch blocks for specific exceptions  
✅ **Timeout Management**: 10-second timeout prevents hanging requests  
✅ **User Feedback**: Loading states and error messages improve UX  
✅ **Code Organization**: Separate component logic with @code block  
✅ **Responsive Design**: Bootstrap classes for mobile compatibility  
✅ **CORS Support**: Proper configuration for cross-origin requests  
✅ **Comments**: Clear explanations of key code sections  

## Troubleshooting

### Port Already in Use
If port 5028 is in use:
- Check `ServerApp\Properties\launchSettings.json`
- Modify the `applicationUrl` to use a different port
- Update the API URL in `FetchProducts.razor` accordingly

### CORS Errors in Browser Console
- Verify CORS is configured in ServerApp/Program.cs
- Check that the Blazor client is making requests to the correct API URL
- Ensure both apps are running

### Products Not Displaying
1. Check browser console (F12) for JavaScript errors
2. Check Network tab to see if API call is being made
3. Verify API returns JSON at `http://localhost:5028/api/products`
4. Ensure the Product class matches the API response structure

## Next Steps (Activity 2)
In the next activity, you'll:
- Debug integration issues
- Handle edge cases
- Optimize performance
- Implement caching strategies

## Notes
- This is a development configuration; for production, implement proper CORS restrictions
- Consider moving API URL to configuration files for environment-specific settings
- Add authentication/authorization for real-world scenarios
- Implement database connection for persistent data storage
