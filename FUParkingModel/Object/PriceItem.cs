using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("PriceItem", Schema = "dbo")]
    public class PriceItem : Common
    {
        [Column("PriceTableId")]
        public required Guid PriceTableId { get; set; }        
        public PriceTable? PriceTable { get; set; }

        [Column("ApplyFromHour")]
        public TimeOnly ApplyFromHour { get; set; }

        [Column("ApplyToHour")]
        public TimeOnly ApplyToHour { get; set; }

        [Column("MaxPrice")]
        public int? MaxPrice { get; set; }

        [Column("MinPrice")]
        public int? MinPrice { get; set; }

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
