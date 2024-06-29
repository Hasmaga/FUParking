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
        public required int Priority { get; set; }

        [Column("Name")]
        public required string Name { get; set; } = null!;

        [Column("ApplyFromDate")]
        public DateTime ApplyFromDate { get; set; }

        [Column("ApplyToDate")]
        public DateTime ApplyToDate { get; set; }

        [Column("StatusPriceTable")]
        public required string StatusPriceTable { get; set; }

        [Column("CreateById")]
        public Guid? CreatedById { get; set; }
        public User? CreateBy { get; set; }

        [Column("LastModifyById")]
        public Guid? LastModifyById { get; set; }
        public User? LastModifyBy { get; set; }

        [Column("LastModifyDate")]
        public DateTime? LastModifyDate { get; set; }

        public ICollection<PriceItem>? PriceItems { get; set; }
    }
}
