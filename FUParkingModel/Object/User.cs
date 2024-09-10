using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("User", Schema = "dbo")]
    public class User : Common
    {
        [Column("RoleId")]
        public Guid RoleId { get; set; }
        public Role? Role { get; set; }

        [Column("FullName")]
        public required string FullName { get; set; }

        [Column("Email")]
        public required string Email { get; set; }

        [Column("PasswordHash")]
        public required string PasswordHash { get; set; }

        [Column("PasswordSalt")]
        public required string PasswordSalt { get; set; }

        [Column("StatusUser")]
        public required string StatusUser { get; set; }

        [Column("CreateById")]
        public Guid? CreatedById { get; set; }  
        public User? CreateBy { get; set; }

        [Column("LastModifyById")]
        public Guid? LastModifyById { get; set; }
        public User? LastModifyBy { get; set; }

        [Column("LastModifyDate")]
        public DateTime? LastModifyDate { get; set; }

        [Column("WrongPassword")]
        public int WrongPassword { get; set; }

        public ICollection<Card>? CardsCreateBy { get; set; }
        public ICollection<Card>? CardsLastModifyBy { get; set; }
        public ICollection<Customer>? CustomersCreateBy { get; set; }
        public ICollection<Customer>? CustomersLastModifyBy { get; set; }
        public ICollection<CustomerType>? CustomerTypeCreateBy { get; set; }
        public ICollection<CustomerType>? CustomerTypeLastModifyBy { get; set; }
        public ICollection<Gate>? GateCreateBy { get; set; }
        public ICollection<Gate>? GateLastModifyBy { get; set; }        
        public ICollection<Package>? PackageCreateBy { get; set; }
        public ICollection<Package>? PackageLastModifyBy { get; set; }
        public ICollection<ParkingArea>? ParkingAreaCreateBy { get; set; }
        public ICollection<ParkingArea>? ParkingAreaLastModifyBy { get; set; }
        public ICollection<PaymentMethod>? PaymentMethodCreateBy { get; set; }
        public ICollection<PaymentMethod>? PaymentMethodLastModifyBy { get; set; }
        public ICollection<PriceItem>? PriceItemCreateBy { get; set; }
        public ICollection<PriceItem>? PriceItemLastModifyBy { get; set; }
        public ICollection<PriceTable>? PriceTableCreateBy { get; set; }
        public ICollection<PriceTable>? PriceTableLastModifyBy { get; set; }
        public ICollection<Role>? RoleCreateBy { get; set; }
        public ICollection<Role>? RoleLastModifyBy { get; set; }
        public ICollection<Vehicle>? VehicleStaffs { get; set; }
        public ICollection<Vehicle>? VehicleLastModifyBy { get; set; }
        public ICollection<VehicleType>? VehicleTypeCreateBy { get; set; }
        public ICollection<VehicleType>? VehicleTypeLastModifyBy { get; set; }
        public ICollection<Session>? SessionCreateBy { get; set; }
        public ICollection<Session>? SessionLastModifyBy { get; set; }
        public ICollection<User>? CreatedBys { get; set; }
        public ICollection<User>? LastModifyBys { get; set; }
    }
}
