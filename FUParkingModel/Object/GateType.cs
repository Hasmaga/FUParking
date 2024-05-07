using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("GateType", Schema = "dbo")]
    public class GateType : Common
    {
        [Column("Name")]
        public string? Name { get; set; }

        [Column("Descriptipn")]
        public string? Descriptipn { get; set; }

        public ICollection<Gate>? Gates { get; set; }
    }
}
