namespace FUParkingModel.RequestObject.Zalo
{
    public class ZaloResDto
    {
        public int Return_code { get; set; }
        public string Return_message { get; set; } = null!;
        public int Sub_return_code { get; set; }
        public string Sub_return_message { get; set; } = null!;
        public string Order_id { get; set; } = null!;
        public string Zp_trans_token { get; set; } = null!;
        public string Order_token { get; set; } = null!;
        public string Qr_code { get; set; } = null!;
    }
}
