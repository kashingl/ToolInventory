# Functionele documentatie – ToolInventory

## 1. Systeembeschrijving

ToolInventory is een beheersysteem voor gereedschapsinventarissen. Het stelt organisaties in staat om gereedschappen bij te houden, uit te geven aan medewerkers, onderhoud te registreren en de locatie en status van elk stuk gereedschap in te zien. De applicatie is beschikbaar als webapplicatie en als mobiele app.

---

## 2. Gebruikersrollen

| Rol | Beschrijving |
|---|---|
| **Medewerker** | Kan inloggen, gereedschappen inzien en uitchecken |
| **Beheerder** | Kan gereedschappen, categorieën en onderhoudsrecords aanmaken, bewerken en verwijderen |

> Alle acties vereisen een ingelogd account (JWT-authenticatie).

---

## 3. Functionele modules

### 3.1 Authenticatie

| Actie | Beschrijving |
|---|---|
| **Registreren** | Nieuwe gebruiker aanmaken met e-mailadres, weergavenaam en wachtwoord |
| **Inloggen** | Inloggen met e-mailadres en wachtwoord; ontvangt JWT-token geldig voor 8 uur |

**Validatieregels:**
- E-mailadres moet uniek zijn.
- Wachtwoord: minimaal 8 tekens, minstens één hoofdletter, één cijfer en één speciaal teken.

---

### 3.2 Gereedschapsbeheer

Gereedschappen vormen de kern van het systeem. Elk stuk gereedschap heeft een status die de levenscyclus bijhoudt.

#### Gegevensmodel

| Veld | Type | Verplicht | Beschrijving |
|---|---|---|---|
| Naam | tekst (max 200) | Ja | Naam van het gereedschap |
| Beschrijving | tekst | Nee | Korte omschrijving |
| Barcode | tekst (max 100) | Nee | Unieke barcode; wordt gebruikt voor snel inchecken |
| Locatie | tekst (max 200) | Nee | Opslaglocatie |
| Systainer | tekst (max 100) | Nee | Systeemkoffer of opslagcode |
| Status | enum | Ja | Zie statusmachine hieronder |
| Afbeelding URL | tekst | Nee | Link naar afbeelding van het gereedschap |
| Categorie | verwijzing | Nee | Koppeling aan een categorie |

#### Statussen

| Status | Betekenis |
|---|---|
| `Available` | Beschikbaar voor uitgifte |
| `CheckedOut` | Momenteel uitgegeven aan een medewerker |
| `UnderMaintenance` | In onderhoud |
| `Retired` | Afgeschreven; kan niet meer worden uitgegeven |

#### Toegestane statusovergangen

| Van → Naar | Toegestaan |
|---|---|
| Available → CheckedOut | ✅ |
| Available → UnderMaintenance | ✅ |
| Available → Retired | ✅ |
| CheckedOut → Available | ✅ |
| UnderMaintenance → Available | ✅ |
| UnderMaintenance → Retired | ✅ |
| Retired → (alles) | ❌ |
| CheckedOut → UnderMaintenance | ❌ |

#### Acties

| Actie | Beschrijving |
|---|---|
| Inzien (lijst) | Gepagineerde lijst met optioneel statusfilter |
| Inzien (detail) | Gedetailleerde weergave van één gereedschap |
| Opzoeken via barcode | Gereedschap ophalen op basis van barcode-scan |
| Aanmaken | Nieuw gereedschap registreren |
| Bewerken | Gegevens of status bijwerken |
| Verwijderen | Gereedschap verwijderen uit het systeem |

---

### 3.3 Categorieën

Categorieën worden gebruikt om gereedschappen te groeperen (bijv. Elektrisch, Handgereedschap, Meetgereedschap).

| Actie | Beschrijving |
|---|---|
| Inzien | Alle categorieën opvragen |
| Aanmaken | Nieuwe categorie aanmaken (naam uniek) |
| Bewerken | Naam/beschrijving aanpassen |
| Verwijderen | Categorie verwijderen |

---

### 3.4 Uitgifte (Checkouts)

De uitgifte-module registreert wanneer een gereedschap wordt meegegeven aan een medewerker en wanneer het teruggebracht is.

#### Gegevensmodel

| Veld | Type | Verplicht | Beschrijving |
|---|---|---|---|
| Gereedschap | verwijzing | Ja | Welk gereedschap wordt uitgegeven |
| Medewerker | verwijzing | Ja | Wie het gereedschap meeneemt |
| Uitgifte-datum | datum/tijd | Ja | Automatisch ingesteld op moment van uitgifte |
| Verwachte retour | datum/tijd | Nee | Verwachte terugbrengdatum (moet in de toekomst liggen) |
| Werkelijke retour | datum/tijd | Nee | Ingevuld bij inname |
| Notities | tekst | Nee | Aanvullende opmerkingen |

#### Acties

| Actie | Beschrijving |
|---|---|
| Inzien (lijst) | Gepagineerde lijst van alle uitgiften |
| Inzien (detail) | Detail van één uitgifte |
| Uitgifte aanmaken | Gereedschap uitgifte registreren; status → `CheckedOut` |
| Inname via uitgifte-ID | Gereedschap innemen op basis van het uitgifte-ID; status → `Available` |
| Inname via barcode/tool-ID | Gereedschap innemen via barcode-scan of tool-ID (snelle scanner-flow) |

**Validatieregels bij uitgifte:**
- Gereedschap moet status `Available` hebben.
- Afgeschreven gereedschappen (`Retired`) kunnen niet worden uitgegeven.
- Verwachte retour, indien opgegeven, moet in de toekomst liggen.
- De medewerker moet een bestaand account hebben.

**Validatieregels bij inname:**
- De uitgifte mag nog niet eerder zijn ingenomen.

---

### 3.5 Onderhoud

Onderhoudsrecords documenteren uitgevoerd onderhoud en plannen toekomstige onderhoudsdata.

#### Gegevensmodel

| Veld | Type | Verplicht | Beschrijving |
|---|---|---|---|
| Gereedschap | verwijzing | Ja | Welk gereedschap is onderhouden |
| Datum | datum | Ja | Datum van onderhoud |
| Beschrijving | tekst | Ja | Wat is er gedaan |
| Uitgevoerd door | tekst | Nee | Naam of afdeling |
| Kosten | bedrag | Nee | Onderhoudskosten (≥ 0) |
| Volgende onderhoudsdatum | datum | Nee | Geplande volgende onderhoud (≥ onderhoudsdatum) |

#### Acties

| Actie | Beschrijving |
|---|---|
| Inzien (lijst) | Gepagineerde lijst van alle onderhoudsrecords |
| Inzien (detail) | Detail van één record |
| Per gereedschap opvragen | Alle records voor een specifiek gereedschap |
| Aanmaken | Nieuw onderhoud registreren; tool-status → `UnderMaintenance` |
| Voltooien | Onderhoud afsluiten; tool-status → `Available` |
| Verwijderen | Record verwijderen (als laatste record voor tool: status → `Available`) |

**Validatieregels:**
- Gereedschap mag niet `Retired` of `CheckedOut` zijn bij start van onderhoud.
- Kosten mogen niet negatief zijn.
- Volgende onderhoudsdatum mag niet vóór de onderhoudsdatum liggen.

---

## 4. API-eindpunten (overzicht)

| Methode | Pad | Beschrijving |
|---|---|---|
| POST | `/api/auth/register` | Gebruiker registreren |
| POST | `/api/auth/login` | Inloggen |
| GET | `/api/tools` | Gereedschappen opvragen (gepagineerd) |
| GET | `/api/tools/{id}` | Gereedschap op ID |
| GET | `/api/tools/barcode/{code}` | Gereedschap op barcode |
| POST | `/api/tools` | Gereedschap aanmaken |
| PUT | `/api/tools/{id}` | Gereedschap bijwerken |
| DELETE | `/api/tools/{id}` | Gereedschap verwijderen |
| GET | `/api/categories` | Categorieën opvragen |
| POST | `/api/categories` | Categorie aanmaken |
| PUT | `/api/categories/{id}` | Categorie bijwerken |
| DELETE | `/api/categories/{id}` | Categorie verwijderen |
| GET | `/api/checkouts` | Uitgiften opvragen (gepagineerd) |
| GET | `/api/checkouts/{id}` | Uitgifte op ID |
| POST | `/api/checkouts` | Uitgifte aanmaken |
| PUT | `/api/checkouts/{id}/checkin` | Inname via uitgifte-ID |
| PUT | `/api/checkouts/tool/{toolId}/checkin` | Inname via tool-ID (scanner) |
| GET | `/api/maintenance` | Onderhoudsrecords opvragen |
| GET | `/api/maintenance/{id}` | Onderhoudsrecord op ID |
| GET | `/api/maintenance/tool/{toolId}` | Records per gereedschap |
| POST | `/api/maintenance` | Onderhoudsrecord aanmaken |
| PUT | `/api/maintenance/{id}/complete` | Onderhoud voltooien |
| DELETE | `/api/maintenance/{id}` | Onderhoudsrecord verwijderen |

> Alle eindpunten (behalve `/api/auth/*`) vereisen een geldig JWT Bearer-token.

---

## 5. Foutafhandeling

Alle API-fouten worden teruggegeven als RFC 7807 `ProblemDetails`:

```json
{
  "status": 400,
  "title": "Tool unavailable.",
  "detail": "The tool is not currently available for checkout."
}
```

Veelvoorkomende statuscodes:

| Code | Betekenis |
|---|---|
| 200 | Succes |
| 201 | Aangemaakt |
| 204 | Succes, geen inhoud |
| 400 | Ongeldige invoer of businessregel geschonden |
| 401 | Niet geauthenticeerd |
| 404 | Niet gevonden |
| 409 | Conflict (bijv. dubbele barcode of e-mail) |
| 429 | Te veel verzoeken (rate limit bereikt) |
