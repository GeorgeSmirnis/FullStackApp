# Integration Testing Guide

## Quick Start - Minimal API Debugging Demonstration

This guide provides step-by-step instructions to test the debugged integration between the Blazor frontend and Minimal API backend.

---

## Setup (First Time Only)

### 1. Navigate to Project Root
```powershell
cd C:\Carreer\microsoftCourse\FullStackApp
```

### 2. Verify Projects Exist
```powershell
# List contents to verify both projects
dir

# Expected output:
# - ClientApp (folder)
# - ServerApp (folder)
# - FullStackSolution.sln (file)
# - README.md (file)
```

---

## Running the Applications

### Option A: Using VS Code (Recommended)

#### Step 1: Open Project in VS Code
```powershell
code .
```

#### Step 2: Open Integrated Terminal
- Press `Ctrl + `` (backtick)` to open the integrated terminal

#### Step 3: Split Terminal (Create Two Panels)
- Press `Ctrl + Shift + Alt + \` (Windows)
- Or use menu: Terminal > Split Terminal

#### Step 4: Start ServerApp
In the first terminal panel:
```powershell
cd ServerApp
dotnet run
```

**Wait for**: "Now listening on: http://localhost:5028"

#### Step 5: Start ClientApp
In the second terminal panel:
```powershell
cd ClientApp
dotnet watch run
```

**Wait for**: "HTTPS: http://localhost:5273" or similar message

---

### Option B: Using Separate Command Prompts

#### Terminal 1 - Start ServerApp
```powershell
cd C:\Carreer\microsoftCourse\FullStackApp\ServerApp
dotnet run
```

#### Terminal 2 - Start ClientApp
```powershell
cd C:\Carreer\microsoftCourse\FullStackApp\ClientApp
dotnet run
```

---

## Testing the API Endpoint

### Test 1: Direct API Call (No Browser)
```powershell
# Using PowerShell (Windows 10+)
curl http://localhost:5028/api/productlist

# OR using Invoke-WebRequest
Invoke-WebRequest -Uri "http://localhost:5028/api/productlist" | ConvertTo-Json
```

**Expected Output** (5 products in JSON):
```json
[
  {
    "id": 1,
    "name": "Laptop",
    "price": 1200.5,
    "stock": 25
  },
  ...
]
```

### Test 2: API Endpoint in Browser
1. Open browser
2. Navigate to: `http://localhost:5028/api/productlist`
3. **Expected**: JSON data displayed in browser
4. **Not Expected**: Error message or blank page

---

## Testing the Blazor Client

### Test 1: Basic Client Load
1. Open browser
2. Navigate to: `http://localhost:5273` (or the actual ClientApp port)
3. **Expected**: 
   - "ClientApp" or "Home" page loads
   - Navigation menu visible with "Products" link

### Test 2: Navigate to Products
1. Click "Products" in the navigation menu
2. **Expected**:
   - Page navigates to `/fetchproducts`
   - Product table loads
   - 5 products displayed in table format

### Test 3: Verify Product Data Display
Check the product table displays:

| ID | Name | Price | Stock |
|----|------|-------|-------|
| 1 | Laptop | $1200.50 | 25 |
| 2 | Headphones | $50.00 | 100 |
| 3 | Keyboard | $129.99 | 50 |
| 4 | Monitor | $299.99 | 15 |
| 5 | Mouse | $29.99 | 200 |

**Verify**:
- All 5 products displayed ✓
- Prices have 2 decimal places ✓
- Stock numbers visible ✓
- Stock badges colored correctly:
  - Green: Stock > 20 ✓
  - Yellow: 0 < Stock ≤ 20 ✓
  - Red: Stock = 0 ✓

### Test 4: Refresh Button Functionality
1. Click the "Refresh Products" button
2. **Expected**: 
   - Table refreshes
   - No errors in console
   - Same 5 products displayed

---

## Browser Developer Tools Testing

### Open Developer Tools
- Press `F12` or `Ctrl + Shift + I`

### Check Console Tab
1. Go to Console tab
2. You should see logs like:
   ```
   Attempting to fetch products from: http://localhost:5028/api/productlist
   Successfully loaded 5 products
   ```

**Red Flags** (Errors that indicate issues):
- ❌ "Access to XMLHttpRequest has been blocked by CORS policy"
- ❌ "404 Not Found"
- ❌ "Failed to connect to the API"

### Check Network Tab
1. Go to Network tab in DevTools
2. Reload page (F5)
3. Click "Products"
4. Look for the API request:

| Column | Expected Value |
|--------|-----------------|
| Request | api/productlist |
| Status | 200 |
| Type | xhr (XMLHttpRequest) |
| Size | ~300 bytes |
| Time | < 100ms |

**Response Tab Content**:
```json
[
  {"id":1,"name":"Laptop","price":1200.5,"stock":25},
  ...
]
```

---

## Debugging Failed Scenarios

### Scenario 1: "Products Not Displaying"

**Steps**:
1. Open DevTools (F12)
2. Console tab - look for errors
3. Check Network tab - is API request made?
4. Is API response valid JSON?

**Quick Checks**:
- [ ] Is ServerApp running? (Check terminal)
- [ ] Is API URL correct? (Should be `/api/productlist`, not `/api/products`)
- [ ] Is port 5028 correct? (Check `ServerApp/Properties/launchSettings.json`)

### Scenario 2: "CORS Error in Console"

**Error Message**:
```
Access to XMLHttpRequest has been blocked by CORS policy
```

**Fix**:
1. Check `ServerApp/Program.cs`
2. Verify `app.UseCors("AllowBlazor")` is present
3. Verify CORS policy is added to services
4. Restart ServerApp after changes

### Scenario 3: "API Returns 404 Error"

**Steps**:
1. Check Network tab in DevTools
2. Look at the failed request URL
3. Expected: `/api/productlist`
4. If different, update FetchProducts.razor

**Common Mistakes**:
- ❌ URL is `/api/products` (old endpoint)
- ❌ URL is `/product` (missing /api)
- ❌ URL has typo like `/api/productist`

### Scenario 4: "JSON Parse Error"

**Error Message**:
```
Failed to parse the API response. The server returned invalid JSON.
```

**Debugging Steps**:
1. Check browser console - should show "Raw JSON response"
2. Copy the JSON from console
3. Validate JSON at: https://jsonlint.com/
4. If invalid, API response structure needs fixing

---

## Performance Testing

### Measure Load Time
1. Open DevTools (F12)
2. Go to Performance or Network tab
3. Click Products
4. Check "Time" column for API request
5. **Expected**: < 100ms

### Test with Slow Connection
1. In Network tab, set throttle to "Slow 3G"
2. Click Products
3. Loading spinner should appear
4. After ~5 seconds, products should load
5. Test timeout handling (set timeout < 5 seconds in code if needed)

---

## Testing Error Handling

### Test 1: Server Shutdown (Timeout)
1. Products loading and displaying
2. In ServerApp terminal, press `Ctrl + C` to stop
3. In ClientApp, click "Refresh Products"
4. **Expected**: Error message appears after 10 seconds

### Test 2: Invalid Port
1. In FetchProducts.razor, change port to `5999`
2. Click "Products"
3. **Expected**: Connection error message

### Test 3: Invalid Endpoint
1. In FetchProducts.razor, change URL to `/api/invalidproducts`
2. Click "Products"
3. **Expected**: 404 error message

---

## Verification Checklist

Use this checklist to verify everything works:

### API Endpoint
- [ ] ServerApp running on port 5028
- [ ] Direct browser access to `/api/productlist` returns JSON
- [ ] JSON contains 5 products

### Blazor Client
- [ ] ClientApp running on port 5273+
- [ ] Client page loads without errors
- [ ] "Products" link visible in navigation

### Integration
- [ ] Clicking "Products" loads product table
- [ ] All 5 products displayed
- [ ] No CORS errors in browser console
- [ ] No JavaScript errors in console

### Error Handling
- [ ] Console shows success logs
- [ ] Refresh button works
- [ ] Error messages appear when server is down

### Styling
- [ ] Table displays with Bootstrap styling
- [ ] Stock badges colored correctly
- [ ] Prices formatted to 2 decimals
- [ ] Responsive design works on mobile

---

## FAQ

### Q: Why does the page say "Loading..." forever?
**A**: Check if ServerApp is running. Open DevTools console for error messages.

### Q: Port 5028 is already in use
**A**: Either stop the other application using that port or change it in `launchSettings.json` and update the URL in FetchProducts.razor.

### Q: Changes to code don't appear to take effect
**A**: 
- ServerApp: Stop and restart `dotnet run`
- ClientApp: Should auto-refresh with `dotnet watch run`, otherwise hard-refresh (Ctrl+Shift+R)

### Q: How do I know which port the ClientApp is using?
**A**: Look at terminal output when ClientApp starts. It will say "Now listening on: http://localhost:XXXX"

### Q: How do I test CORS locally?
**A**: The current setup allows any origin for testing. In production, specify exact origins in CORS policy.

---

## Summary

If all tests pass, your integration is working correctly! The application successfully:
1. ✅ Fetches data from the Minimal API
2. ✅ Handles CORS correctly
3. ✅ Deserializes JSON properly
4. ✅ Displays data in the UI
5. ✅ Provides error feedback to users

You're now ready for Activity 3: Advanced JSON Structures and Filtering.
