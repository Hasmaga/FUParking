using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("Package", Schema = "dbo")]
    public class Package : Common
    {
        [Column("Name")]
        public required string Name { get; set; }

        [Column("CoinAmount")]
        public int CoinAmount { get; set; }

        [Column("Price")]
        public decimal Price { get; set; }

        [Column("PackageStatus")]
        public required string PackageStatus { get; set; }

        [JsonIgnore]
        public ICollection<Deposit>? Deposits { get; set; }
    }
}
