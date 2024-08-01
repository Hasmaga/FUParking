using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Payment", Schema = "dbo")]
    public class Payment : Common
    {
        [Column("PaymentMethodId")]
        public Guid PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        [Column("SessionId")]
        public Guid SessionId { get; set; }
        public Session? Session { get; set; }

        [Column("TotalPrice")]
        public required int TotalPrice { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }

    }
}