# 💊 Pharmacy Management System

A **complete, offline-first** pharmacy management application that runs locally on your Windows computer.
No internet connection, no cloud accounts, and no technical setup needed after the first installation.

---

## 🖥️ For the Client / End User

### Option A — Run as a Single `.exe` (Recommended)

> **This is the easiest option. No programming knowledge required.**

#### Step 1 — Install the .NET 8 Runtime (one-time only)

1. Go to this link on any internet-connected computer:
   **https://dotnet.microsoft.com/en-us/download/dotnet/8.0**
2. Under **".NET Runtime"** (not SDK), click **"Download x64"** for Windows.
3. Run the downloaded installer and follow the on-screen steps.
4. Restart your computer.

> ✅ You only need to do this **once**. The runtime does not need to be updated manually.

#### Step 2 — Download the Application

1. Go to: **https://github.com/Khaled-Abdelfattah/Pharmacy**
2. Click the green **"Code"** button → **"Download ZIP"**

   ![Download ZIP](.docs/download-zip.png)

3. Extract the ZIP file to any folder (e.g., `C:\PharmacyApp\`)

#### Step 3 — Publish the Single `.exe`

Open **Command Prompt** (press `Win + R`, type `cmd`, press Enter), then paste this command:

```cmd
cd C:\PharmacyApp\Pharmacy
dotnet publish Pharmacy.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o C:\PharmacyApp\Output
```

> This creates a single file: `C:\PharmacyApp\Output\PharmacySystem.exe`

#### Step 4 — Create a Desktop Shortcut

1. Go to `C:\PharmacyApp\Output\`
2. Right-click `PharmacySystem.exe` → **Send to** → **Desktop (create shortcut)**
3. Done! Double-click the shortcut anytime to launch the pharmacy system.

> The first time you run it, Windows may show a security prompt — click **"Run anyway"**.
> Your browser will open automatically to **http://localhost:5000**.

---

### Option B — Run Directly (for Developers / IT Staff)

#### Prerequisites

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Git | Any | https://git-scm.com/download/win |

#### Step 1 — Clone the Repository

Open **Command Prompt** or **PowerShell** and run:

```bash
git clone https://github.com/Khaled-Abdelfattah/Pharmacy.git
cd Pharmacy
```

#### Step 2 — Run the Application

```bash
dotnet run --project Pharmacy.csproj
```

Your default browser will open automatically at **http://localhost:5000**.

> On first run, the database is created automatically in:
> `C:\Users\<YourName>\AppData\Local\PharmacySystem\pharmacy.db`
> Sample products are seeded automatically.

#### Step 3 — Publish as a Single `.exe` (optional)

```powershell
dotnet publish Pharmacy.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

Output: `./publish/PharmacySystem.exe` — a single portable file, no installation needed.

---

## ✨ Features

| Module | What it does |
|--------|-------------|
| **Sales / POS** | Scan barcodes or search by name. Add to cart, complete sale. Keyboard-friendly. |
| **Shift Report** | Filter sales by exact time range for cash settlement with employees. Presets: Today, Yesterday. |
| **Inventory** | Add/edit/delete products with barcode, category, stock, price, expiry. Low-stock & out-of-stock alerts. |
| **Daily Invoice** | Summary of all items sold on a selected day. Export to **PDF** or **Excel** — fully offline. |
| **Orders** | Record supplier orders. "Needs Reorder" list auto-fills when stock hits 0. Mark received → stock updated. |
| **Auto-backup** | Database backed up automatically every day at 02:00 AM. Last 7 days of backups kept. |

---

## 🗄️ Where is My Data Stored?

| Item | Location |
|------|----------|
| Database | `%LOCALAPPDATA%\PharmacySystem\pharmacy.db` |
| Daily backups | `%LOCALAPPDATA%\PharmacySystem\backups\` |

> To open that folder: press `Win + R`, type `%LOCALAPPDATA%\PharmacySystem`, press Enter.

**To back up your data manually**, simply copy `pharmacy.db` to a USB drive or any safe location.

**To restore**, copy it back and restart the application.

---

## 🎨 Theme Colors

The app uses a **white + sky-blue + green** color scheme.
All colors are CSS variables in `wwwroot/css/site.css` and can be changed easily.

---

## 🌍 Future: Arabic / RTL Support

The app is structured to support Arabic with minimal changes:
1. Change `<html dir="ltr">` → `<html dir="rtl">` in `Views/Shared/_Layout.cshtml`
2. Add Arabic resource files under `Resources/`

---

## 🧪 Running Tests (Developers)

```bash
dotnet test tests/PharmacySystem.Tests/PharmacySystem.Tests.csproj
```

22 tests covering: stock deduction, over-sell prevention, shift filtering, reorder triggers, daily invoice aggregation, and more.

---

## 📋 Project Structure

```
Pharmacy/
├── Controllers/        ← HTTP routing only (no business logic)
├── Services/           ← All business logic (SalesService, OrderService, etc.)
├── Data/               ← AppDbContext, EF Migrations, Seeder
├── Models/             ← Entities + ViewModels
├── Views/              ← Razor views (Sales, Inventory, Invoices, Orders)
├── wwwroot/            ← CSS, JS, local libraries (jsPDF, SheetJS)
├── tests/              ← xUnit tests
└── README.md
```

---

## ❓ Troubleshooting

| Problem | Solution |
|---------|----------|
| Browser doesn't open automatically | Manually go to **http://localhost:5000** |
| "Access denied" or write error | Make sure you're running from `AppData`, not `Program Files` |
| Port 5000 already in use | Close other apps using that port or change the port in `Program.cs` |
| Database is missing / corrupt | Delete `%LOCALAPPDATA%\PharmacySystem\pharmacy.db` and restart — it will be recreated with sample data |
| Windows blocked the `.exe` | Right-click the `.exe` → Properties → check "Unblock" → OK |

---

*Built with ASP.NET Core 8 · Entity Framework Core · SQLite · Bootstrap 5*
