# 💊 Pharmacy Management System

---

## 🚀 How to Install & Run

### Step 1 — Install the .NET 8 Runtime *(one-time only)*

1. Open this link in your browser:
   **https://dotnet.microsoft.com/en-us/download/dotnet/8.0**
2. Under **".NET Runtime"**, click **"Download x64"** for Windows
3. Run the installer and follow the steps
4. Restart your computer

> ✅ You only need to do this **once**.

---

### Step 2 — Download the Application

1. Go to: **https://github.com/Khaled-Abdelfattah/Pharmacy**
2. Click the green **"Code"** button → **"Download ZIP"**
3. Extract the ZIP to any folder — for example: `C:\PharmacyApp\`

---

### Step 3 — Create the `.exe`

1. Press `Win + R`, type `cmd`, press **Enter**
2. Paste the following command and press **Enter**:

```cmd
dotnet publish "C:\PharmacyApp\Pharmacy\Pharmacy.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "C:\PharmacyApp\Output"
```

> ⚠️ If you extracted the ZIP to a different folder, replace `C:\PharmacyApp\Pharmacy\` with your actual path.

This will create a single file: **`C:\PharmacyApp\Output\PharmacySystem.exe`**

---

### Step 4 — Create a Desktop Shortcut

1. Go to `C:\PharmacyApp\Output\`
2. Right-click `PharmacySystem.exe` → **Send to** → **Desktop (create shortcut)**

**Done! ✅** From now on, just double-click the shortcut on your Desktop to launch the system.

> Your browser will open automatically to the pharmacy system. No internet needed.

---

## ✨ Features

| Module | Description |
|--------|-------------|
| **Sales / POS** | Barcode scanning or product search. Add to cart and complete sales. |
| **Shift Report** | Filter sales by time range for cash settlement. |
| **Inventory** | Add, edit, and remove products with stock levels, prices, and expiry dates. |
| **Daily Invoice** | Full summary of daily sales. Export to **PDF** or **Excel**. |
| **Orders** | Record orders from suppliers. Auto-alerts when stock runs out. |
| **Auto-backup** | Database backed up automatically every day at 02:00 AM. |

---

## 🗄️ Where is My Data?

Your data is saved automatically on your computer at:

```text
C:\Users\YourName\AppData\Local\PharmacySystem\
```

- **Database file:** `pharmacy.db`
- **Daily backups:** `backups\` folder (last 7 days kept automatically)

**To back up manually:** Copy `pharmacy.db` to a USB drive or any safe location.
**To restore:** Copy it back and restart the application.

---

## ❓ Troubleshooting

| Problem | Solution |
|---------|----------|
| Browser doesn't open | Go to **http://localhost:5000** manually |
| Windows blocked the `.exe` | Right-click → Properties → check **"Unblock"** → OK |
| "dotnet" is not recognized | Repeat Step 1 (install .NET Runtime) and restart |
| App won't start / error on launch | Delete `pharmacy.db` from the AppData folder — it will be recreated fresh on next launch |
