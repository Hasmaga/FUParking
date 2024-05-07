using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("PriceTable", Schema = "dbo")]
    public class PriceTable : Common
    {
        [Column("VehicleTypeId")]
        public Guid VehicleTypeId { get; set; }
        public VehicleType? VehicleType { get; set; }

        [Column("Priority")]
        public int Priority { get; set; }

        [Column("Name")]
        public string Name { get; set; } = null!;

        [Column("ApplyFromDate")]
        public DateTime ApplyFromDate { get; set; }

        [Column("ApplyToDate")]
        public DateTime ApplyToDate { get; set; }

        [Column("StatusPriceTable")]
        public string? StatusPriceTable { get; set; }

        public ICollection<PriceItem>? PriceItems { get; set; }
    }
}
