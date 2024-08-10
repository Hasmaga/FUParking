namespace FUParkingModel.ResponseObject.Transaction
{
    public class GetInfoWalletTransResDto
    {
        public Guid Id { get; set; }
        public int Amount { get; set; }
        public string TransactionDescription { get; set; } = null!;
        public string TransactionStatus { get; set; } = null!;
        public DateTime Date { get; set; }
        public string TransactionType { get; set; } = null!;
    }
}
