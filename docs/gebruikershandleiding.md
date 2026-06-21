# Gebruikershandleiding – ToolInventory

## 1. Inleiding

ToolInventory helpt u om gereedschappen bij te houden: wie heeft wat, waar ligt het, wanneer is het onderhouden en is het nog beschikbaar? U kunt de webapplicatie gebruiken via een browser of de mobiele app op uw telefoon of tablet.

---

## 2. Aanmelden en inloggen

### Registreren (nieuw account)

1. Ga naar de webapplicatie (bijv. `http://localhost:4200`).
2. Klik op **Registreren**.
3. Vul uw e-mailadres, weergavenaam en wachtwoord in.
   - Wachtwoordvereisten: minimaal 8 tekens, minstens één hoofdletter, één cijfer en één speciaal teken.
4. Klik op **Account aanmaken**.
5. U bent direct ingelogd.

### Inloggen

1. Ga naar de loginpagina.
2. Vul uw e-mailadres en wachtwoord in.
3. Klik op **Inloggen**.
4. Uw sessie is 8 uur geldig; daarna wordt u automatisch uitgelogd.

### Uitloggen

Klik op uw gebruikersnaam of het uitlog-icoon rechtsboven en kies **Uitloggen**.

---

## 3. Gereedschappen

### Gereedschappen inzien

1. Klik op **Gereedschappen** in het navigatiemenu.
2. U ziet een overzichtslijst met naam, categorie, locatie en status.
3. Gebruik het **statusfilter** (bijv. "Beschikbaar") om de lijst te verfijnen.
4. Klik op een gereedschap om de detailweergave te openen.

### Gereedschap zoeken via barcode

1. Gebruik de **scannerfunctie** (zie sectie 7) of vul handmatig de barcode in het zoekveldin op de gereedschappenpagina.
2. Het bijbehorende gereedschap wordt direct getoond.

### Gereedschap aanmaken

1. Klik op **Gereedschappen** → **+ Nieuw gereedschap**.
2. Vul de verplichte velden in:
   - **Naam** (verplicht)
3. Vul optioneel in: beschrijving, barcode, locatie, systainer, afbeelding-URL en categorie.
4. Klik op **Opslaan**.

### Gereedschap bewerken

1. Open het gereedschap via de lijstweergave.
2. Klik op **Bewerken**.
3. Pas de gewenste velden aan.
4. Klik op **Opslaan**.

> **Let op:** Niet alle statuswijzigingen zijn toegestaan. U kunt een `Afgeschreven` gereedschap niet meer wijzigen.

### Gereedschap verwijderen

1. Open het gereedschap.
2. Klik op **Verwijderen**.
3. Bevestig in het dialoogvenster.

---

## 4. Categorieën

Categorieën helpen gereedschappen te groeperen (bijv. Elektrisch, Handgereedschap).

1. Ga naar **Categorieën** in het menu.
2. Gebruik **+ Nieuwe categorie** om een categorie toe te voegen.
3. Klik op een categorie om deze te bewerken of te verwijderen.

---

## 5. Uitgifte van gereedschappen

### Gereedschap uitgifte registreren

1. Open het gewenste gereedschap (status moet **Beschikbaar** zijn).
2. Klik op **Uitgifte registreren**.
3. Selecteer de medewerker die het gereedschap meeneemt.
4. Voer optioneel een **verwachte retourendatum** in (moet in de toekomst liggen).
5. Voeg eventueel notities toe.
6. Klik op **Bevestigen**.
7. De status van het gereedschap wijzigt naar **Uitgegeven**.

### Gereedschap innemen

#### Via de uitgiftelijst

1. Ga naar **Uitgiften** in het menu.
2. Zoek de actieve uitgifte.
3. Klik op **Innemen** en bevestig.

#### Via barcode-scanner (snel innemen)

1. Open de **Scanner** (zie sectie 7).
2. Scan de barcode van het gereedschap.
3. Het gereedschap wordt direct ingenomen als er een actieve uitgifte is.

---

## 6. Onderhoud

### Onderhoud registreren

1. Open het gereedschap (status moet **Beschikbaar** zijn; het mag niet uitgegeven zijn).
2. Klik op **Onderhoud starten**.
3. Vul de datum, beschrijving en optioneel de uitvoerder, kosten en volgende onderhoudsdatum in.
4. Klik op **Opslaan**.
5. De status wijzigt naar **In onderhoud**.

### Onderhoud voltooien

1. Ga naar **Onderhoud** of open het gereedschap.
2. Zoek het actieve onderhoudsrecord.
3. Klik op **Voltooien** en bevestig.
4. De status wijzigt terug naar **Beschikbaar**.

### Onderhoudsgeschiedenis inzien

1. Open het gewenste gereedschap.
2. Bekijk het tabblad of de sectie **Onderhoudshistorie** voor een overzicht van alle eerdere onderhoudsrecords.

---

## 7. Scanner (mobiele app)

De mobiele app biedt een ingebouwde barcode-scanner voor snel innemen van gereedschappen.

1. Open de **ToolInventory**-app op uw mobiel.
2. Log in met uw account.
3. Tik op **Scanner** in het navigatiemenu.
4. Richt de camera op de barcode van het gereedschap.
5. De app zoekt het gereedschap op en neemt het automatisch in als er een actieve uitgifte bestaat.
6. U ontvangt een bevestiging of een foutmelding (bijv. als het gereedschap niet uitgegeven is).

> **Tip:** De scanner stuurt maar één verzoek per scan om dubbele acties te voorkomen.

---

## 8. Statusoverzicht

| Status | Icoon/kleur | Betekenis |
|---|---|---|
| Beschikbaar | 🟢 Groen | Gereedschap is beschikbaar voor uitgifte |
| Uitgegeven | 🟡 Oranje | Gereedschap is bij een medewerker |
| In onderhoud | 🔵 Blauw | Gereedschap wordt onderhouden |
| Afgeschreven | 🔴 Rood | Gereedschap is buiten gebruik gesteld |

---

## 9. Veelgestelde vragen

**Ik kan een gereedschap niet uitgifte geven – waarom?**
Het gereedschap heeft waarschijnlijk niet de status `Beschikbaar`. Controleer de huidige status. Als het `Afgeschreven` is, kan het niet meer worden uitgegeven.

**De barcode-scan werkt niet.**
Controleer of de barcode in het systeem staat geregistreerd bij het gereedschap. Controleer ook de cameratoestemming van de app.

**Mijn sessie is verlopen.**
JWT-tokens zijn 8 uur geldig. Log opnieuw in om verder te gaan.

**Ik zie de knop "Onderhoud starten" niet.**
Het gereedschap moet de status `Beschikbaar` hebben. Als het `Uitgegeven` is, neem het dan eerst in.
