using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Feedback", Schema = "dbo")]
    public class Feedback : Common
    {
        [Column("CustomerId")]
        //[JsonIgnore]
        public Guid CustomerId { get; set; }
        //[JsonIgnore]
        public Customer? Customer { get; set; }

        [Column("ParkingAreaId")]
        //[JsonIgnore]
        public Guid ParkingAreaId { get; set; }
        //[JsonIgnore]
        public ParkingArea? ParkingArea { get; set; }

        [Column("Title")]
        public string? Title { get; set; }

        [Column("Description")]
        public string? Description { get; set; }
    }
}
