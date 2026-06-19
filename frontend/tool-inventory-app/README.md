# Tool Inventory Frontend (Angular)

Angular 21 + Angular Material frontend for tools, checkouts, and maintenance.

## Run locally

```powershell
cd frontend/tool-inventory-app
npm ci
ng serve
```

The app runs on `http://localhost:4200`.

## Build and test

```powershell
ng build
ng test --watch=false
```

## Runtime API configuration

API base URL is loaded from:

`src/assets/config.json`

Example:

```json
{
  "apiBaseUrl": "http://localhost:5177"
}
```

## Implemented clean-code improvements

1. **Shared confirm dialog** replaced native `confirm()` usage:
   - `src/app/shared/confirm-dialog/*`
2. **Scanner check-in optimization** now uses backend endpoint by tool id:
   - `PUT /api/checkouts/tool/{toolId}/checkin`
3. **Shared tool status mapping**:
   - `src/app/shared/tool-status.util.ts`
4. **Lifecycle cleanup**:
   - data-loading side effects moved to `ngOnInit` in list/dialog components.
5. **Form safety improvements**:
   - stricter value guards and reduced unsafe non-null assertions.
6. **Routing fallback**:
   - wildcard route redirects unknown paths to `tools`.

## Key folders

| Path | Purpose |
| --- | --- |
| `src/app/features/` | Feature modules (auth, tools, checkouts, maintenance) |
| `src/app/services/` | API/data services |
| `src/app/interceptors/` | Auth and error interceptors |
| `src/app/shared/` | Shared dialogs/utilities/components |
