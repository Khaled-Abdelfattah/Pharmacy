namespace PharmacySystem.Services;

/// <summary>
/// Background service that:
///   1. Backs up the SQLite database daily at 02:00 AM local time.
///   2. Deletes backups older than 7 days.
/// </summary>
public class BackupService : BackgroundService
{
    private readonly ILogger<BackupService> _logger;
    private readonly IConfiguration _configuration;

    public BackupService(ILogger<BackupService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BackupService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            // Schedule next backup at 02:00 AM today or tomorrow
            var nextRun = DateTime.Today.AddHours(2);
            if (now >= nextRun)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            _logger.LogInformation("Next backup scheduled at {NextRun}", nextRun);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (!stoppingToken.IsCancellationRequested)
                PerformBackup();
        }

        _logger.LogInformation("BackupService stopping.");
    }

    private void PerformBackup()
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbFolder = Path.Combine(appData, "PharmacySystem");
            var dbPath = Path.Combine(dbFolder, "pharmacy.db");
            var backupFolder = Path.Combine(dbFolder, "backups");
            Directory.CreateDirectory(backupFolder);

            if (!File.Exists(dbPath))
            {
                _logger.LogWarning("Database file not found at {DbPath}. Skipping backup.", dbPath);
                return;
            }

            var backupFile = Path.Combine(backupFolder, $"pharmacy_{DateTime.Now:yyyyMMdd_HHmm}.db");
            File.Copy(dbPath, backupFile, overwrite: true);
            _logger.LogInformation("Backup created: {BackupFile}", backupFile);

            // Delete backups older than 7 days
            var cutoff = DateTime.Now.AddDays(-7);
            foreach (var file in Directory.GetFiles(backupFolder, "pharmacy_*.db"))
            {
                var created = File.GetCreationTime(file);
                if (created < cutoff)
                {
                    File.Delete(file);
                    _logger.LogInformation("Deleted old backup: {File}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup failed.");
        }
    }
}
