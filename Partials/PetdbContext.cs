using Microsoft.EntityFrameworkCore;

namespace PawsPort.Models
{
    public partial class PetDbContext : DbContext
    {
        public PetDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddEnvironmentVariables()
                    .AddUserSecrets<PetDbContext>(optional: true)
                    .Build();

                // 檢查是否為 LOCAL 環境
                string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                string? connectionString;

                if (environment == "LOCAL")
                {
                    // 本機開發環境
                    connectionString = config.GetConnectionString("LocalConnection")
                        ?? "Server=localhost;Database=PetDB;Trusted_Connection=True;TrustServerCertificate=True;";
                }
                else
                {
                    // 其他環境 - User Secrets > 環境變數 > appsettings.json
                    connectionString = config["PetDB"]
                        ?? Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION_STRING")
                        ?? config.GetConnectionString("DefaultConnection");
                }

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("找不到資料庫連接字串。請設定 User Secrets、環境變數或 appsettings.json。");
                }

                // 配置 SQL Server 提供者並啟用重試機制
                optionsBuilder.UseSqlServer(connectionString, sqlServerOptions =>
                {
                    // 啟用暫時性錯誤重試機制
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);

                    // 設定命令逾時時間（秒）
                    sqlServerOptions.CommandTimeout(60);
                });
            }
        }
    }
}
