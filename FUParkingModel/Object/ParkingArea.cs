using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("ParkingArea", Schema = "dbo")]
    public class ParkingArea : Common
    {
        [Column("Name")]
        public required string Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Column("MaxCapacity")]
        public int MaxCapacity { get; set; }

        [Column("Block")]
        public required int Block { get; set; }

        [Column("Mode")]
        public required string Mode { get; set; }

        [Column("StatusParkingArea")]
        public required string StatusParkingArea { get; set; }

        [Column("CreateById")]
        public Guid? CreatedById { get; set; }
        public User? CreateBy { get; set; }

        [Column("LastModifyById")]
        public Guid? LastModifyById { get; set; }
        public User? LastModifyBy { get; set; }

        [Column("LastModifyDate")]
        public DateTime? LastModifyDate { get; set; }

        public ICollection<Feedback>? Feedbacks { get; set; }
        public ICollection<Gate>? Gates { get; set; }
    }
}
