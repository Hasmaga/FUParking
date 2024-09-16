namespace FUParkingModel.ResponseObject.Card
{
    public class GetCardByCardNumberResDto
    {
        public required Guid CardId { get; set; }
        public required string CardNumber { get; set; }       
        public required string Status { get; set; }
        public Guid? SessionId { get; set; }
        public string? SessionPlateNumber { get; set; }
        public string? SessionVehicleType { get; set; }
        public DateTime SessionTimeIn { get; set; }
        public string? SessionGateIn { get; set; }
        public DateTime? SessionTimeOut { get; set; }
        public string? SessionGateOut { get; set; }
        public string? ImageInUrl { get; set; }
        public string? ImageInBodyUrl { get; set; }
        public string? SessionStatus { get; set; } 
    }
}