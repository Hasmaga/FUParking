using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Gate", Schema = "dbo")]
    public class Gate : Common
    {
        [Column("ParkingAreaId")]
        public Guid ParkingAreaId { get; set; }
        public ParkingArea? ParkingArea { get; set; }

        [Column("Name")]
        public required string Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Column("GateTypeId")]
        public Guid GateTypeId { get; set; }
        public GateType? GateType { get; set; }

        [Column("StatusGate")]
        public required string StatusGate { get; set; }

        [Column("CreateById")]
        public Guid? CreatedById { get; set; }
        public User? CreateBy { get; set; }

        [Column("LastModifyById")]
        public Guid? LastModifyById { get; set; }
        public User? LastModifyBy { get; set; }

        [Column("LastModifyDate")]
        public DateTime? LastModifyDate { get; set; }

        public ICollection<Session>? SessionGateIns { get; set; }

        public ICollection<Session>? SessionGateOuts { get; set; }
    }
}
