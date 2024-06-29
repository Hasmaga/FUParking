using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("GateType", Schema = "dbo")]
    public class GateType : Common
    {
        [Column("Name")]
        public required string Name { get; set; }

        [Column("Descriptipn")]
        public required string Descriptipn { get; set; }

        [Column("CreateById")]
        public Guid? CreatedById { get; set; }
        public User? CreateBy { get; set; }

        [Column("LastModifyById")]
        public Guid? LastModifyById { get; set; }
        public User? LastModifyBy { get; set; }

        [Column("LastModifyDate")]
        public DateTime? LastModifyDate { get; set; }

        public ICollection<Gate>? Gates { get; set; }
    }
}
