using FUParkingModel.Object;
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
            return config.GetConnectionString("DefaultConnection") ?? "Data Source=127.0.0.1,9210;Initial Catalog=FUParking;User ID=sa;Password=pWV083Z7kb;TrustServerCertificate=True";
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>()
                .HasMany(e => e.Sessions)
                .WithOne(e => e.Card)
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Customer>()
                .HasMany(e => e.Feedbacks)
                .WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Customer>()
                .HasMany(e => e.Vehicles)
                .WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Customer>()
                .HasMany(e => e.Wallets)
                .WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CustomerType>()
                .HasMany(e => e.Customers)
                .WithOne(e => e.CustomerType)
                .HasForeignKey(e => e.CustomerTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Gate>()
                .HasMany(e => e.SessionGateIns)
                .WithOne(e => e.GateIn)
                .HasForeignKey(e => e.GateInId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Gate>()
                .HasMany(e => e.SessionGateOuts)
                .WithOne(e => e.GateOut)
                .HasForeignKey(e => e.GateOutId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<GateType>()
                .HasMany(e => e.Gates)
                .WithOne(e => e.GateType)
                .HasForeignKey(e => e.GateTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ParkingArea>()
                .HasMany(e => e.Gates)
                .WithOne(e => e.ParkingArea)
                .HasForeignKey(e => e.ParkingAreaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ParkingArea>()
                .HasMany(e => e.Feedbacks)
                .WithOne(e => e.ParkingArea)
                .HasForeignKey(e => e.ParkingAreaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasMany(e => e.Transactions)
                .WithOne(e => e.Payment)
                .HasForeignKey(e => e.PaymentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaymentMethod>()
                .HasMany(e => e.Payments)
                .WithOne(e => e.PaymentMethod)
                .HasForeignKey(e => e.PaymentMethodId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PriceTable>()
                .HasMany(e => e.PriceItems)
                .WithOne(e => e.PriceTable)
                .HasForeignKey(e => e.PriceTableId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Role>()
                .HasMany(e => e.Users)
                .WithOne(e => e.Role)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Session>()
                .HasMany(e => e.Payments)
                .WithOne(e => e.Session)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<VehicleType>()
                .HasMany(e => e.Vehicles)
                .WithOne(e => e.VehicleType)
                .HasForeignKey(e => e.VehicleTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Wallet>()
                .HasMany(e => e.Transactions)
                .WithOne(e => e.Wallet)
                .HasForeignKey(e => e.WalletId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Deposit>()
                .HasMany(e => e.Transactions)
                .WithOne(e => e.Deposit)
                .HasForeignKey(e => e.DepositId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Customer>()
                .HasMany(e => e.Deposits)
                .WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaymentMethod>()
                .HasMany(e => e.Deposits)
                .WithOne(e => e.PaymentMethod)
                .HasForeignKey(e => e.PaymentMethodId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Package>()
                .HasMany(e => e.Deposits)
                .WithOne(e => e.Package)
                .HasForeignKey(e => e.PackageId)
                .OnDelete(DeleteBehavior.NoAction);

            OnModelCreatingPartial(modelBuilder);
        }

        public virtual DbSet<Card> Cards { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<CustomerType> CustomerTypes { get; set; }
        public virtual DbSet<Deposit> Deposits { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }
        public virtual DbSet<Gate> Gates { get; set; }
        public virtual DbSet<GateType> GateTypes { get; set; }
        public virtual DbSet<Package> Packages { get; set; }
        public virtual DbSet<ParkingArea> ParkingAreas { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
        public virtual DbSet<PriceItem> PriceItems { get; set; }
        public virtual DbSet<PriceTable> PriceTables { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Session> Sessions { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Vehicle> Vehicles { get; set; }
        public virtual DbSet<VehicleType> VehicleTypes { get; set; }
        public virtual DbSet<Wallet> Wallets { get; set; }
    }
}
