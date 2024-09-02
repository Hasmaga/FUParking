namespace FUParkingModel.ResponseObject.Session
{
    public class GetCustomerTypeByPlateNumberResDto
    {
        public string? CustomerType { get; set; }
        public PreviousSessionInfo? PreviousSessionInfo { get; set; }
    }

    public class PreviousSessionInfo
    {
        public Guid Id { get; set; }
        public string GateIn { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public string ImageInUrl { get; set; } = null!;
        public string ImageInBodyUrl { get; set; } = null!;
        public DateTime TimeIn { get; set; }
        public string VehicleyType { get; set; } = null!;
        public string? CustomerEmail { get; set; }
        public string CustomerType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string CardOrPlateNumber { get; set; } = null!;
    }
}
