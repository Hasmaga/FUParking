using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("VehicleType", Schema = "dbo")]
    public class VehicleType : Common
    {
        [Column("Name")]
        public required string Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [JsonIgnore]
        public ICollection<Vehicle>? Vehicles { get; set; }

        [JsonIgnore]
        public ICollection<PriceTable>? PriceTables { get; set; }
    }
}
