using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("Customer", Schema = "dbo")]
    public class Customer : Common
    {
        [Column("CustomerTypeId")]        
        public required Guid CustomerTypeId { get; set; }
        public CustomerType? CustomerType { get; set; }

        [Column("FullName")]
        public required string FullName { get; set; }

        [Column("Email")]
        public required string Email { get; set; }

        [Column("StatusCustomer")]
        public required string StatusCustomer { get; set; }

        [Column("CreateById")]
        public Guid? CreatedById { get; set; }
        public User? CreateBy { get; set; }

        [Column("LastModifyById")]
        public Guid? LastModifyById { get; set; }
        public User? LastModifyBy { get; set; }

        [Column("LastModifyDate")]
        public DateTime? LastModifyDate { get; set; }

        public ICollection<Vehicle>? Vehicles { get; set; }
        public ICollection<Feedback>? Feedbacks { get; set; }
        public ICollection<Wallet>? Wallets { get; set; }
        public ICollection<Deposit>? Deposits { get; set; }
        public ICollection<Session>? Sessions { get; set; }
    }
}
