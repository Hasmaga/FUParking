using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Wallet", Schema = "dbo")]
    public class Wallet : Common
    {
        [Column("Balance")]
        public int Balance { get; set; }

        [Column("CustomerId")]
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Column("WalletStatus")]
        public string? WalletStatus { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }
    }
}
