using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Customer", Schema = "dbo")]
    public class Customer : Common
    {
        [Column("CustomerTypeId")]
        public Guid CustomerTypeId { get; set; }
        public CustomerType? CustomerType { get; set; }

        [Column("FullName")]
        public string? FullName { get; set; }

        [Column("Phone")]
        public string? Phone { get; set; }

        [Column("Email")]
        public string? Email { get; set; }

        [Column("PasswordHash")]
        public string? PasswordHash { get; set; }

        [Column("StatusCustomer")]
        public string? StatusCustomer { get; set; }

        public ICollection<Feedback>? Feedbacks { get; set; }
        public ICollection<Vehicle>? Vehicles { get; set; }
        public ICollection<Wallet>? Wallets { get; set; }
    }
}
