namespace FUParkingModel.ResponseObject.Card
{
    public class GetCardByCardNumberResDto
    {
        public required string CardNumber { get; set; }       
        public required string Status { get; set; }
        public Guid? SessionId { get; set; }
        public string? SessionPlateNumber { get; set; }
        public string? SessionVehicleType { get; set; }
        public DateTime SessionTimeIn { get; set; }
        public string? SessionGateIn { get; set; }
        public string? SessionCustomerName { get; set; }
        public string? SessionCustomerEmail { get; set; }
        public string? ImageInUrl { get; set; }
        public string? ImageInBodyUrl { get; set; }
        public string? SessionStatus { get; set; } 
    }
}
