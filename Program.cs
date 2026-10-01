using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PharmacySystem.Data;
using PharmacySystem.Services;
using PharmacySystem.Services.Interfaces;

namespace PharmacySystem;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── Database path: %LOCALAPPDATA%\PharmacySystem\pharmacy.db ──────────
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dbFolder = Path.Combine(appData, "PharmacySystem");
        Directory.CreateDirectory(dbFolder);
        var dbPath = Path.Combine(dbFolder, "pharmacy.db");

        // ── Services ──────────────────────────────────────────────────────────
        builder.Services.AddControllersWithViews()
            .AddRazorRuntimeCompilation();

        builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

        builder.Services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ISalesService, SalesService>();
        builder.Services.AddScoped<IOrderService, OrderService>();

        builder.Services.AddHostedService<BackupService>();

        // ── Kestrel: HTTP only on localhost:5000 ──────────────────────────────
        builder.WebHost.UseUrls("http://localhost:5000");

        var app = builder.Build();

        // ── Apply EF migrations + seed data at startup ────────────────────────
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
            await Seeder.SeedAsync(db);
        }

        // ── Middleware pipeline ───────────────────────────────────────────────
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Sales}/{action=Index}/{id?}");

        // ── Open browser after server starts (no console window in production) ─
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            try
            {
                Process.Start(new ProcessStartInfo("http://localhost:5000")
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // Non-fatal — server still runs fine
                Console.Error.WriteLine($"Could not open browser: {ex.Message}");
            }
        });

        await app.RunAsync();
    }
}
