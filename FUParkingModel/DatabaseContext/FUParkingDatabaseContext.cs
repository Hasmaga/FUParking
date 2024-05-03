using FUParkingModel.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FUParkingModel.DatabaseContext
{
    public partial class FUParkingDatabaseContext : DbContext
    {
        public FUParkingDatabaseContext()
        {
        }

        public FUParkingDatabaseContext(DbContextOptions<FUParkingDatabaseContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(GetConnectionString());

        private static string GetConnectionString()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            return config.GetConnectionString("DefaultConnection") ?? throw new Exception(ErrorEnumServer.SERVER_ERROR);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreatingPartial(modelBuilder);
        }
    }
}
