using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("PaymentMethod", Schema = "dbo")]
    public class PaymentMethod : Common
    {
        [Column("Name")]
        public string? Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [JsonIgnore]
        public ICollection<Payment>? Payments { get; set; }
    }
}
