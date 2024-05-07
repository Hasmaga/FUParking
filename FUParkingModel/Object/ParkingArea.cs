using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("ParkingArea", Schema = "dbo")]
    public class ParkingArea : Common
    {
        [Column("Name")]
        public string? Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Column("MaxCapacity")]
        public int MaxCapacity { get; set; }

        [Column("Block")]
        public int Block { get; set; }

        [Column("Mode")]
        public string? Mode { get; set; }

        [Column("StatusParkingArea")]
        public string? StatusParkingArea { get; set; }

        public ICollection<Feedback>? Feedbacks { get; set; }
        public ICollection<Gate>? Gates { get; set; }
    }
}
