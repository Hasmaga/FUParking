namespace FUParkingModel.ResponseObject.Session
{
    public class GetSessionByUserResDto
    {
        public Guid Id { get; set; }
        public string CardNumber { get; set; } = null!;
        public string GateInName { get; set; } = null!;
        public string GateOutName { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public string ImageInUrl { get; set; } = null!;
        public string ImageInBodyUrl { get; set; } = null!;
        public string ImageOutUrl { get; set; } = null!;
        public string ImageOutBodyUrl { get; set; } = null!;
        public DateTime TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public string Mode { get; set; } = null!;
        public int Block { get; set; }
        public string VehicleTypeName { get; set; } = null!;
        public string PaymentMethodName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string CheckInStaff { get; set; } = null!;
        public string CheckOutStaff { get; set; } = null!;
        public string ParkingArea { get; set; } = null!;
    }
}
