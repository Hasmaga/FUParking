using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("VehicleType", Schema = "dbo")]
    public class VehicleType : Common
    {
        [Column("Name")]
        public required string Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        public ICollection<Vehicle>? Vehicles { get; set; }
        public ICollection<PriceTable>? PriceTables { get; set; }
    }
}
