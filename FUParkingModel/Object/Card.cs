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

        [Column("CreateById")]
        public Guid? CreatedById { get; set; }
        public User? CreateBy { get; set; }

        [Column("Status")]
        public string Status { get; set; } = null!;

        [Column("LastModifyById")]
        public Guid? LastModifyById { get; set; }
        public User? LastModifyBy { get; set; }

        [Column("LastModifyDate")]
        public DateTime? LastModifyDate { get; set; }

        public ICollection<Session>? Sessions { get; set; }
    }
}
