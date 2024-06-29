using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Feedback", Schema = "dbo")]
    public class Feedback : Common
    {
        [Column("CustomerId")]        
        public Guid CustomerId { get; set; }        
        public Customer? Customer { get; set; }

        [Column("ParkingAreaId")]        
        public Guid ParkingAreaId { get; set; }        
        public ParkingArea? ParkingArea { get; set; }

        [Column("SessionId")]
        public Guid SessionId { get; set; }
        public Session? Session { get; set; }

        [Column("Title")]
        public required string? Title { get; set; }

        [Column("Description")]
        public required string Description { get; set; }        
    }
}
