namespace FUParkingModel.RequestObject.Zalo
{
    public class ZaloCallback
    {
        public int? Amount { get; set; }
        public int? AppId { get; set; }
        public string? AppTransId { get; set; }
        public string? BankCode { get; set; }
        public string? Checksum { get; set; }
        public int? DiscountAmount { get; set; }
        public int? PmcId { get; set; }
        public int? Status { get; set; }
    }
}
