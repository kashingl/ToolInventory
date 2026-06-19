# ToolInventory Backend API

.NET 10 Web API with EF Core, Identity, JWT auth, and a service-based application layer.

## Run locally

```powershell
cd src/ToolInventory.API
dotnet run
```

## Test

```powershell
dotnet test tests/ToolInventory.API.Tests/ToolInventory.API.Tests.csproj
```

## Implemented clean-code improvements

1. **Thin controllers + service layer**
   - Business logic moved into:
     - `Services/ToolService.cs`
     - `Services/CheckoutService.cs`
     - `Services/MaintenanceService.cs`
2. **Startup split for clarity**
   - Service registration: `Extensions/ServiceCollectionExtensions.cs`
   - HTTP pipeline: `Extensions/ApplicationBuilderExtensions.cs`
3. **Consistent API errors**
   - Structured `ProblemDetails` usage through shared patterns.
4. **Input normalization**
   - Centralized normalization helper in `Common/InputNormalizer.cs`.
5. **Repository/query improvements**
   - Added cancellation token support and filtered paging.
6. **Optimized scanner check-in API**
   - `PUT /api/checkouts/tool/{toolId}/checkin`
7. **Type-safe status updates**
   - `UpdateToolDto.Status` uses enum (`ToolStatusValue`) instead of raw string.

## API endpoints (core)

| Area | Endpoints |
| --- | --- |
| Auth | `POST /api/auth/login`, `POST /api/auth/register` |
| Tools | `GET/POST /api/tools`, `GET/PUT/DELETE /api/tools/{id}`, `GET /api/tools/barcode/{code}` |
| Checkouts | `GET/POST /api/checkouts`, `PUT /api/checkouts/{id}/checkin`, `PUT /api/checkouts/tool/{toolId}/checkin` |
| Maintenance | `GET/POST /api/maintenance`, `GET /api/maintenance/tool/{toolId}`, `PUT /api/maintenance/{id}/complete`, `DELETE /api/maintenance/{id}` |
| Categories | `GET/POST /api/categories`, `GET/PUT/DELETE /api/categories/{id}` |
