using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Card", Schema = "dbo")]
    public class Card : Common
    {
        [Column("PlateNumber")]
        public string? PlateNumber { get; set; }

        [Column("CardNumber")]
        public string CardNumber { get; set; } = null!;

        public ICollection<Session>? Sessions { get; set; }
    }
}
