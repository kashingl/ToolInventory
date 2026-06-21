# ToolInventory

ToolInventory is a full-stack tool management system with:

1. **Backend API** in `src/ToolInventory.API` (.NET 10, EF Core, Identity, JWT).
2. **Frontend** in `frontend/tool-inventory-app` (Angular 21, Angular Material).
3. **Mobile app** in `mobile/ToolInventory.MAUI` (.NET MAUI).

## Repository structure

| Path | Purpose |
| --- | --- |
| `src/` | Backend projects (`API`, `Core`, `Infrastructure`, `Shared`) |
| `frontend/tool-inventory-app/` | Angular web app |
| `mobile/ToolInventory.MAUI/` | MAUI mobile app |
| `tests/` | API tests and E2E tests |

## Recent architecture improvements

### Backend

1. Controllers are thinner; checkout/tool/maintenance business rules moved to service layer.
2. Startup is split into extension methods (`ServiceCollectionExtensions`, `ApplicationBuilderExtensions`).
3. Repository/UoW support cancellation tokens and filtered paging.
4. Inputs are normalized centrally and API errors are consistently returned as `ProblemDetails`.
5. Added optimized scanner check-in endpoint: `PUT /api/checkouts/tool/{toolId}/checkin`.

### Frontend

1. `confirm()` prompts replaced by shared Angular Material confirm dialog.
2. Scanner check-in now uses backend endpoint by tool id (no full checkout list fetch).
3. Shared status-color mapping and reduced duplicated dialog refresh logic.
4. Better component lifecycle usage (`ngOnInit` for data-loading side effects).
5. Tools page redesigned to an operations dashboard with quick checkout/check-in panel, metrics, and alerts.
6. Input and action icons migrated to custom SVG icons (mail, user, search, QR/barcode, plus, reset, edit, trash, box, eye, calendar).

### Mobile

1. API calls now return explicit `ApiResult`/`ApiResult<T>` with status and error details.
2. ViewModels use consistent dialog/navigation abstraction via `IUserDialogService`.
3. Route strings centralized in `Navigation/AppRoutes.cs`.
4. Scanner flow includes request guarding to avoid duplicate/racing actions.
5. Mobile scanner check-in now uses `PUT /api/checkouts/tool/{toolId}/checkin`.

## Local development quick start

### Backend API

```powershell
cd src/ToolInventory.API
dotnet run
```

### Frontend

```powershell
cd frontend/tool-inventory-app
npm ci
ng serve
```

### Mobile (Windows target)

```powershell
dotnet build mobile/ToolInventory.MAUI/ToolInventory.MAUI.csproj -f net10.0-windows10.0.19041.0
```

## Security baseline

1. Vulnerability reporting process: see [SECURITY.md](SECURITY.md).
2. Automated checks in CI: CodeQL, secret scanning (Gitleaks), dependency review, and SBOM generation.
3. Dependency updates: Dependabot for GitHub Actions.
4. Secret hygiene: `.gitignore` blocks common local secret files.