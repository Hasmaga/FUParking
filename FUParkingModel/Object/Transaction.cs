using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Transaction", Schema = "dbo")]
    public class Transaction : Common
    {
        [Column("WalletId")]
        public Guid? WalletId { get; set; }
        public Wallet? Wallet { get; set; }

        [Column("PaymentId")]
        public Guid? PaymentId { get; set; }
        public Payment? Payment { get; set; }

        [Column("DepositId")]
        public Guid? DepositId { get; set; }
        public Deposit? Deposit { get; set; }

        [Column("Amount")]
        public int Amount { get; set; }

        [Column("TransactionDescription")]
        public required string TransactionDescription { get; set; }

        [Column("TransactionStatus")]
        public required string TransactionStatus { get; set; }
    }
}
