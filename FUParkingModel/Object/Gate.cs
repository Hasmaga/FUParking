using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Gate", Schema = "dbo")]
    public class Gate : Common
    {
        [Column("ParkingAreaId")]
        public Guid ParkingAreaId { get; set; }
        public ParkingArea? ParkingArea { get; set; }

        [Column("WPFCode")]
        public Guid WPFCode { get; set; } = Guid.NewGuid();

        [Column("Name")]
        public string? Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Column("GateTypeId")]
        public Guid GateTypeId { get; set; }
        public GateType? GateType { get; set; }

        [Column("StatusGate")]
        public string? StatusGate { get; set; }

        public ICollection<Camera>? Cameras { get; set; }
        public ICollection<Session>? SessionGateIns { get; set; }
        public ICollection<Session>? SessionGateOuts { get; set; }
    }
}
