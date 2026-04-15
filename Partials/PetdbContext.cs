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
                    .AddUserSecrets<PetDbContext>(optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                string connectionString = config["PetDB"];

                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
