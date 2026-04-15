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

                string is_local = config["IS_LOCAL"];
                string connectionString;

                if (is_local == "true")
                {
                    connectionString = "Data Source=.;Initial Catalog=PetDB;Integrated Security=True;Encrypt=False";
                }
                else
                {
                    connectionString = config["PetDB"]
                        ?? throw new InvalidOperationException("找不到資料庫連接字串。請設定 User Secrets 或環境變數。");
                }

                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
