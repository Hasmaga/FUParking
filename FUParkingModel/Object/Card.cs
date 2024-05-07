using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Card", Schema = "dbo")]
    public class Card : Common
    {
        [Column("PlateNumber")]
        public string? PlateNumber { get; set; }

        public ICollection<Session>? Sessions { get; set; }
    }
}
