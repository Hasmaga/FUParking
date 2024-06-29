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

        [Column("ExtraCoin")]
        public int? ExtraCoin { get; set; }

        [Column("EXPPackage")]
        public int? EXPPackage { get; set; }

        [Column("Price")]
        public int Price { get; set; }

        [Column("PackageStatus")]
        public required string PackageStatus { get; set; }

        [Column("CreateById")]
        public Guid? CreatedById { get; set; }
        public User? CreateBy { get; set; }

        [Column("LastModifyById")]
        public Guid? LastModifyById { get; set; }
        public User? LastModifyBy { get; set; }

        [Column("LastModifyDate")]
        public DateTime? LastModifyDate { get; set; }
        
        public ICollection<Deposit>? Deposits { get; set; }
    }
}
