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
        public TimeOnly ApplyFromHour { get; set; }

        [Column("ApplyToHour")]
        public TimeOnly ApplyToHour { get; set; }

        [Column("MaxPrice")]
        public int MaxPrice { get; set; }

        [Column("MinPrice")]
        public int MinPrice { get; set; }
    }
}
