using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Camera", Schema = "dbo")]
    public class Camera : Common
    {
        [Column("Name")]
        public string? Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Column("GateId")]
        public Guid GateId { get; set; }
        public Gate? Gate { get; set; }
    }
}
