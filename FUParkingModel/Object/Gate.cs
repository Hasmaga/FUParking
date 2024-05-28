using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("Gate", Schema = "dbo")]
    public class Gate : Common
    {
        [Column("ParkingAreaId")]
        public Guid ParkingAreaId { get; set; }
        
        [JsonIgnore]
        public ParkingArea? ParkingArea { get; set; }

        [Column("WPFCode")]
        public Guid WPFCode { get; set; } = Guid.NewGuid();

        [Column("Name")]
        public required string Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Column("GateTypeId")]
        public Guid GateTypeId { get; set; }

        [JsonIgnore]
        public GateType? GateType { get; set; }

        [Column("StatusGate")]
        public string? StatusGate { get; set; }

        [JsonIgnore]
        public ICollection<Camera>? Cameras { get; set; }

        [JsonIgnore]
        public ICollection<Session>? SessionGateIns { get; set; }

        [JsonIgnore]
        public ICollection<Session>? SessionGateOuts { get; set; }
    }
}
