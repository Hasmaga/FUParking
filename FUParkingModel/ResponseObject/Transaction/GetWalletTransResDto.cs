namespace FUParkingModel.ResponseObject.Transaction
{
    public class GetWalletTransResDto
    {
        public int Balance { get; set; }
        public IEnumerable<GetInfoWalletTransResDto>? Transactions { get; set; }
    }
}
