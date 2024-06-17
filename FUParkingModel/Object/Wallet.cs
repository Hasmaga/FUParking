using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("Wallet", Schema = "dbo")]
    public class Wallet : Common
    {
        [Column("Balance")]
        public int Balance { get; set; } = 0;

        [Column("WalletType")]
        public required string WalletType { get; set; }

        [JsonIgnore]
        [Column("CustomerId")]
        public required Guid CustomerId { get; set; }

        [JsonIgnore]
        public Customer? Customer { get; set; }

        [Column("WalletStatus")]
        [JsonIgnore]
        public string? WalletStatus { get; set; }

        [Column("EXPDate")]
        public DateTime? EXPDate { get; set; }

        [JsonIgnore]
        public ICollection<Transaction>? Transactions { get; set; }
    }
}
