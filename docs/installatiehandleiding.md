# Installatiehandleiding – ToolInventory

## 1. Vereisten

### Backend API

| Vereiste | Minimale versie |
|---|---|
| .NET SDK | 10.0 |
| SQL Server | 2019 of LocalDB (ontwikkeling) |
| Docker *(optioneel)* | 24+ |

### Webfrontend

| Vereiste | Minimale versie |
|---|---|
| Node.js | 20 LTS |
| npm | 10+ |
| Angular CLI | 17+ (`npm install -g @angular/cli`) |

### Mobiele app (.NET MAUI)

| Vereiste | Minimale versie |
|---|---|
| .NET SDK | 10.0 |
| Visual Studio 2022 | 17.10+ met MAUI-workload |
| Windows | 10 versie 19041+ (Windows-target) |
| Android SDK | API 21+ (Android-target) |

---

## 2. Repository klonen

```powershell
git clone https://github.com/kashingl/ToolInventory.git
cd ToolInventory
```

---

## 3. Backend API installeren en starten

### 3.1 Configuratie

Maak in `src/ToolInventory.API/` een bestand `appsettings.Development.json` aan (of pas het bestaande aan) met de volgende inhoud:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ToolInventoryDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "UW_GEHEIME_SLEUTEL_MINIMAAL_32_TEKENS",
    "Issuer": "ToolInventory.API",
    "Audience": "ToolInventory.Client",
    "ExpiryMinutes": 480
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200"
    ]
  }
}
```

> **Belangrijk:** De JWT-sleutel (`Jwt:Key`) mag **nooit** in versiebeheer staan. Gebruik voor productie een omgevingsvariabele:
>
> ```powershell
> $env:TOOLINVENTORY_JWT_KEY = "UW_GEHEIME_SLEUTEL_MINIMAAL_32_TEKENS"
> ```

### 3.2 Database aanmaken

De API past automatisch migraties toe bij opstart. Zorg dat SQL Server of LocalDB bereikbaar is.

Handmatig migraties toepassen:

```powershell
cd src/ToolInventory.API
dotnet ef database update
```

### 3.3 API starten

```powershell
cd src/ToolInventory.API
dotnet run
```

De API is standaard bereikbaar op:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:7000`
- **Swagger UI**: `https://localhost:7000/swagger`

---

## 4. Webfrontend installeren en starten

### 4.1 Afhankelijkheden installeren

```powershell
cd frontend/tool-inventory-app
npm ci
```

### 4.2 Proxy-configuratie

Het bestand `proxy.conf.json` stuurt API-aanroepen door naar de lokale backend. Controleer of de URL overeenkomt met uw backend-adres (standaard `http://localhost:5000`).

### 4.3 Development server starten

```powershell
ng serve
```

De webapplicatie is bereikbaar op `http://localhost:4200`.

### 4.4 Productie-build

```powershell
ng build --configuration production
```

De statische bestanden worden gegenereerd in `dist/tool-inventory-app/`.

---

## 5. Mobiele app installeren en starten

### 5.1 MAUI-workload installeren

```powershell
dotnet workload install maui
```

### 5.2 API-adres instellen

Open `mobile/ToolInventory.MAUI/Services/ApiConfiguration.cs` (of de bijbehorende `appsettings`) en stel de basis-URL in van uw backend:

```csharp
public string BaseUrl => "https://uw-api-adres.nl";
```

Voor lokaal testen op Android-emulator gebruikt u `http://10.0.2.2:5000`.

### 5.3 Windows-target bouwen en uitvoeren

```powershell
cd mobile/ToolInventory.MAUI
dotnet build ToolInventory.MAUI.csproj -f net10.0-windows10.0.19041.0
dotnet run -f net10.0-windows10.0.19041.0
```

### 5.4 Android-target bouwen

```powershell
dotnet build ToolInventory.MAUI.csproj -f net10.0-android
```

---

## 6. Installatie via Docker

### 6.1 Backend API

```powershell
cd src/ToolInventory.API
docker build -t toolinventory-api .
docker run -d -p 5000:8080 `
  -e ConnectionStrings__DefaultConnection="Server=db;Database=ToolInventoryDb;User=sa;Password=Uw_Wachtwoord!" `
  -e TOOLINVENTORY_JWT_KEY="UW_GEHEIME_SLEUTEL_MINIMAAL_32_TEKENS" `
  --name toolinventory-api `
  toolinventory-api
```

### 6.2 Webfrontend

```powershell
cd frontend/tool-inventory-app
docker build -t toolinventory-frontend .
docker run -d -p 80:80 --name toolinventory-frontend toolinventory-frontend
```

### 6.3 Docker Compose (aanbevolen voor ontwikkeling)

Maak een `docker-compose.yml` aan in de root van de repository:

```yaml
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "Uw_Wachtwoord!"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"

  api:
    build: ./src/ToolInventory.API
    ports:
      - "5000:8080"
    environment:
      ConnectionStrings__DefaultConnection: "Server=db;Database=ToolInventoryDb;User=sa;Password=Uw_Wachtwoord!"
      TOOLINVENTORY_JWT_KEY: "UW_GEHEIME_SLEUTEL_MINIMAAL_32_TEKENS"
      Jwt__Issuer: "ToolInventory.API"
      Jwt__Audience: "ToolInventory.Client"
      Cors__AllowedOrigins__0: "http://localhost:80"
    depends_on:
      - db

  frontend:
    build: ./frontend/tool-inventory-app
    ports:
      - "80:80"
    depends_on:
      - api
```

Starten:

```powershell
docker compose up -d
```

---

## 7. Omgevingsvariabelen (productie)

| Variabele | Beschrijving |
|---|---|
| `TOOLINVENTORY_JWT_KEY` | JWT-ondertekeningssleutel (minimaal 32 tekens) |
| `ConnectionStrings__DefaultConnection` | SQL Server verbindingsstring |
| `Jwt__Issuer` | JWT-issuer (standaard: `ToolInventory.API`) |
| `Jwt__Audience` | JWT-audience (standaard: `ToolInventory.Client`) |
| `Jwt__ExpiryMinutes` | Geldigheidsduur token in minuten (standaard: `480`) |
| `Cors__AllowedOrigins__0` | Toegestane CORS-origin (bijv. `https://app.uwdomein.nl`) |

---

## 8. Gezondheidscheck

De API exposeert een health-check-endpoint:

```
GET /healthz
```

Verwachte respons: `200 OK` met body `Healthy`.

---

## 9. Probleemoplossing

| Probleem | Oplossing |
|---|---|
| `JWT signing key is not configured` | Controleer `Jwt:Key` in `appsettings.json` of stel de omgevingsvariabele `TOOLINVENTORY_JWT_KEY` in. |
| Database-verbindingsfout | Controleer of SQL Server/LocalDB actief is en de verbindingsstring klopt. |
| CORS-fout in browser | Voeg het frontend-adres toe aan `Cors:AllowedOrigins` in de API-configuratie. |
| `dotnet ef` niet gevonden | Installeer met `dotnet tool install --global dotnet-ef`. |
| Angular app laadt niet | Controleer of `npm ci` succesvol is uitgevoerd en of de proxy-configuratie naar de juiste API-URL verwijst. |
| MAUI-app bouwt niet | Controleer of de MAUI-workload is geïnstalleerd: `dotnet workload list`. |
