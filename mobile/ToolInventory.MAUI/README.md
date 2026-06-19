# ToolInventory Mobile (.NET MAUI)

Cross-platform mobile client for ToolInventory.

## Build (Windows target)

```powershell
dotnet build mobile/ToolInventory.MAUI/ToolInventory.MAUI.csproj -f net10.0-windows10.0.19041.0
```

## API base URL

Configured through `IApiConfiguration` using preferences key:

- Key: `api_base_url`
- Default:
  - Android emulator: `http://10.0.2.2:5177`
  - Others: `http://localhost:5177`

## Implemented clean-code improvements

1. **Structured API result model**
   - Added `Services/ApiResult.cs`.
   - API calls now return `ApiResult` / `ApiResult<T>` (with status + error message), not only `bool`/`null`.
2. **Consistent UI abstraction**
   - ViewModels now consistently use `IUserDialogService` for alerts/prompts/confirm/navigation.
3. **Centralized route constants**
   - Added `Navigation/AppRoutes.cs`.
4. **Improved loading safety**
   - ViewModels use `try/finally` to prevent stuck loading state.
5. **Scanner flow hardening**
   - Added duplicate/race guards in scanner viewmodel.
6. **Optimized scanner check-in**
   - Mobile scanner uses `PUT /api/checkouts/tool/{toolId}/checkin`.
7. **Auth usability improvement**
   - Auth service exposes `CurrentUserId` (from JWT `sub`) and checkout prompts can prefill this value.
8. **Runtime API base URL sync**
   - Services ensure `HttpClient.BaseAddress` follows current configuration.

## Important files

| Path | Purpose |
| --- | --- |
| `Services/ToolApiService.cs` | Backend API client implementation |
| `Services/AuthService.cs` | Login/register/token persistence |
| `Services/ApiResult.cs` | Standard API call result contract |
| `ViewModels/` | Screen-specific app logic |
| `Navigation/AppRoutes.cs` | Route constants |
