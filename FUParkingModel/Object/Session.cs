using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Session", Schema = "dbo")]
    public class Session : Common
    {
        [Column("CardId")]        
        public required Guid CardId { get; set; }
        public Card? Card { get; set; }

        [Column("GateInId")]
        public required Guid GateInId { get; set; }
        public Gate? GateIn { get; set; }

        [Column("GateOutId")]
        public Guid? GateOutId { get; set; }
        public Gate? GateOut { get; set; }

        [Column("PlateNumber")]
        public string? PlateNumber { get; set; }

        [Column("ImageInUrl")]
        public required string ImageInUrl { get; set; }

        [Column("ImageOutUrl")]
        public required string ImageOutUrl { get; set; }

        [Column("TimeIn")]
        public required DateTime TimeIn { get; set; }

        [Column("TimeOut")]
        public DateTime TimeOut { get; set; }

        [Column("Mode")]
        public required string Mode { get; set; } = null!;

        [Column("CustomerId")]
        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        
        public ICollection<Payment>? Payments { get; set; }        
        public ICollection<Feedback>? Feedbacks { get; set; }
    }
}
