using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("Transaction", Schema = "dbo")]
    public class Transaction : Common
    {
        [Column("WalletId")]
        [JsonIgnore]
        public Guid? WalletId { get; set; }

        [JsonIgnore]
        public Wallet? Wallet { get; set; }

        [Column("PaymentId")]
        [JsonIgnore]
        public Guid? PaymentId { get; set; }

        [JsonIgnore]
        public Payment? Payment { get; set; }

        [Column("DepositId")]
        [JsonIgnore]
        public Guid? DepositId { get; set; }
        public Deposit? Deposit { get; set; }

        [Column("Amount")]
        public int Amount { get; set; }

        [Column("TransactionDescription")]
        public string? TransactionDescription { get; set; }

        [Column("TransactionStatus")]
        public string? TransactionStatus { get; set; }
    }
}
