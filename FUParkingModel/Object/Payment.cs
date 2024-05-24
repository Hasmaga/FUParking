using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("Payment", Schema = "dbo")]
    public class Payment : Common
    {
        [Column("PaymentMethodId")]
        [JsonIgnore]
        public Guid PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        [Column("SessionId")]
        [JsonIgnore]
        public Guid SessionId { get; set; }
        public Session? Session { get; set; }

        [Column("TotalPrice")]
        public int TotalPrice { get; set; }

        [Column("StatusPayment")]
        public string? StatusPayment { get; set; }
        [JsonIgnore]
        public ICollection<Transaction>? Transactions { get; set; }
    }
}