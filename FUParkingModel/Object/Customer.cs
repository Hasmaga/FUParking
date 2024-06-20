using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("Customer", Schema = "dbo")]
    public class Customer : Common
    {
        [Column("CustomerTypeId")]
        [JsonIgnore]
        public Guid CustomerTypeId { get; set; }
        public CustomerType? CustomerType { get; set; }

        [Column("FullName")]
        public string? FullName { get; set; }

        [Column("Email")]
        public string? Email { get; set; }        

        [Column("StatusCustomer")]
        public required string StatusCustomer { get; set; }

        [Column("Avarta")]
        public string? Avarta { get; set; }

        [JsonIgnore]
        public ICollection<Feedback>? Feedbacks { get; set; }
        [JsonIgnore]
        public ICollection<Vehicle>? Vehicles { get; set; }
        [JsonIgnore]
        public ICollection<Wallet>? Wallets { get; set; }
        [JsonIgnore]
        public ICollection<Deposit>? Deposits { get; set; }
    }
}
