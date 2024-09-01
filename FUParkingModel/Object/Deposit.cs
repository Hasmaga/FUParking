using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Deposit", Schema = "dbo")]
    public class Deposit : Common
    {
        [Column("PaymentMethodId")]
        public Guid PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        [Column("PackageId")]
        public Guid PackageId { get; set; }
        public Package? Package { get; set; }

        [Column("CustomerId")]
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Column("Amount")]
        public int Amount { get; set; }

        [Column("AppTranId")]
        public string? AppTranId { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }
    }
}
