namespace FUParkingModel.ResponseObject.Card
{
    public class GetCardByCardNumberResDto
    {
        public required string CardNumber { get; set; }
        public string? PlateNumber { get; set; }
        public required string Status { get; set; }
        public Guid? SessionId { get; set; }
        public string? SessionPlateNumber { get; set; }
        public string? SessionVehicleType { get; set; }
        public DateTime SessionTimeIn { get; set; }
        public string? SessionGateIn { get; set; }
        public string? SessionCustomerName { get; set; }
        public string? SessionCustomerEmail { get; set; }
        public string? imageInUrl { get; set; }
        public string? imageInBodyUrl { get; set; }
        public string? sessionStatus { get; set; } 
    }
}
