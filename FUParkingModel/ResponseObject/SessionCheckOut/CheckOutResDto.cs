namespace FUParkingModel.ResponseObject.SessionCheckOut
{
    public class CheckOutResDto
    {
        public string Message { get; set; } = null!;
        public int? Amount { get; set; }
        public string ImageIn { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
    }
}
