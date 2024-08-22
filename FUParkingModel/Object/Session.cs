using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Session", Schema = "dbo")]
    public class Session : Common
    {
        [Column("CardId")]
        public Guid CardId { get; set; }
        public Card? Card { get; set; }

        [Column("GateInId")]
        public Guid GateInId { get; set; }
        public Gate? GateIn { get; set; }

        [Column("GateOutId")]
        public Guid? GateOutId { get; set; }
        public Gate? GateOut { get; set; }

        [Column("PlateNumber")]
        public required string PlateNumber { get; set; }

        [Column("ImageInUrl")]
        public required string ImageInUrl { get; set; }

        [Column("ImageInBodyUrl")]
        public string ImageInBodyUrl { get; set; } = null!;

        [Column("ImageOutUrl")]
        public string? ImageOutUrl { get; set; }

        [Column("ImageOutBodyUrl")]
        public string? ImageOutBodyUrl { get; set; }

        [Column("TimeIn")]
        public required DateTime TimeIn { get; set; }

        [Column("TimeOut")]
        public DateTime? TimeOut { get; set; }

        [Column("Mode")]
        public required string Mode { get; set; } = null!;

        [Column("Block")]
        public required int Block { get; set; }

        [Column("VehicleTypeId")]
        public Guid VehicleTypeId { get; set; }
        public VehicleType? VehicleType { get; set; }

        [Column("PaymentMethodId")]
        public Guid? PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        [Column("CustomerId")]
        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Column("Status")]
        public required string Status { get; set; } = null!;

        [Column("CreateById")]
        public Guid? CreatedById { get; set; }
        public User? CreateBy { get; set; }

        [Column("LastModifyById")]
        public Guid? LastModifyById { get; set; }
        public User? LastModifyBy { get; set; }

        [Column("LastModifyDate")]
        public DateTime? LastModifyDate { get; set; }

        public ICollection<Payment>? Payments { get; set; }
        public ICollection<Feedback>? Feedbacks { get; set; }
    }
}
