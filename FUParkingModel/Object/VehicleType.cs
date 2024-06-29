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

        [Column("CreateById")]
        public Guid? CreatedById { get; set; }
        public User? CreateBy { get; set; }

        [Column("LastModifyById")]
        public Guid? LastModifyById { get; set; }
        public User? LastModifyBy { get; set; }

        [Column("LastModifyDate")]
        public DateTime? LastModifyDate { get; set; }

        public ICollection<Vehicle>? Vehicles { get; set; }        
        public ICollection<PriceTable>? PriceTables { get; set; }
    }
}
