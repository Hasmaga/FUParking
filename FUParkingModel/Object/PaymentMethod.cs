using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("PaymentMethod", Schema = "dbo")]
    public class PaymentMethod : Common
    {
        [Column("Name")]
        public string? Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        public ICollection<Payment>? Payments { get; set; }
    }
}
