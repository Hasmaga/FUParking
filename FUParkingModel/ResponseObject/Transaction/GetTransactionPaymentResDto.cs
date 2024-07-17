namespace FUParkingModel.ResponseObject.Transaction
{
    public class GetTransactionPaymentResDto
    {
        public string Email { get; set; } = null!;
        public string WalletType { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
        public string PackageName { get; set; } = null!;
        public int Amount { get; set; }
        public string TransactionDescription { get; set; } = null!;
        public string TransactionStatus { get; set; } = null!;
        public DateTime CreateDateTime { get; set; }
    }
}
