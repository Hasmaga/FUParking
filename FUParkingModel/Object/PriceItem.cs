using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("PriceItem", Schema = "dbo")]
    public class PriceItem : Common
    {
        [Column("PriceTableId")]
        public Guid PriceTableId { get; set; }
        public PriceTable? PriceTable { get; set; }

        [Column("ApplyFromHour")]
        [Range(0, 24)]
        public int? ApplyFromHour { get; set; }

        [Column("ApplyToHour")]
        [Range(0, 24)]
        public int? ApplyToHour { get; set; }

        [Column("MaxPrice")]
        public int MaxPrice { get; set; }

        [Column("BlockPricing")]
        public int BlockPricing { get; set; }

        [Column("MinPrice")]
        public int MinPrice { get; set; }

        [Column("CreateById")]
        public Guid? CreatedById { get; set; }
        public User? CreateBy { get; set; }

        [Column("LastModifyById")]
        public Guid? LastModifyById { get; set; }
        public User? LastModifyBy { get; set; }

        [Column("LastModifyDate")]
        public DateTime? LastModifyDate { get; set; }
    }
}
