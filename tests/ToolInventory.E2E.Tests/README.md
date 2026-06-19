# E2E Tests (Playwright)

These tests use `Microsoft.Playwright` directly with plain xUnit and require both the Angular app and the .NET API to be running.

## Prerequisites

Build the E2E project and install Playwright browsers:

```powershell
dotnet build tests/ToolInventory.E2E.Tests/ToolInventory.E2E.Tests.csproj
pwsh tests/ToolInventory.E2E.Tests/bin/Debug/net10.0/playwright.ps1 install chromium
```

## Running the apps

```powershell
# Terminal 1 - API
cd src/ToolInventory.API
dotnet run

# Terminal 2 - Angular
cd frontend/tool-inventory-app
ng serve
```

## Running E2E tests

The tests are intentionally marked with `Skip` because they require the apps to be running locally.

Remove the `Skip` attribute from the tests you want to execute, then run:

```powershell
dotnet test tests/ToolInventory.E2E.Tests --filter "FullyQualifiedName~E2E"
```
