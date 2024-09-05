namespace FUParkingModel.ResponseObject.Card
{
    public class GetCardResDto
    {
        public Guid Id { get; set; }
        public string CardNumber { get; set; } = null!;        
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = null!;
        public GetSessionWithCard Session { get; set; } = null!;
        public bool IsInUse { get; set; }
    }

    public class GetSessionWithCard
    {
        public Guid SessionId { get; set; }
        public string GateIn { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public string ImageInUrl { get; set; } = null!;
        public string ImageInBodyUrl { get; set; } = null!;
        public DateTime TimeIn { get; set; }
        public string VehicleType { get; set; } = null!;
        public string? CustomerEmail { get; set; } = null!;
        public string StaffCheckInEmail { get; set; } = null!;
    }
}
