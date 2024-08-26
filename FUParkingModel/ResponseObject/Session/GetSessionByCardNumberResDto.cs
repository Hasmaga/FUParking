namespace FUParkingModel.ResponseObject.Session
{
    public class GetSessionByCardNumberResDto
    {
        public Guid CardId { get; set; }
        public string GateIn { get; set; } = null!;
        public string GateOut { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public string ImageInUrl { get; set; } = null!;
        public string ImageInBodyUrl { get; set; } = null!;        
        public DateTime TimeIn { get; set; }        
        public string VehicleType { get; set; } = null!;
        public bool? IsEnoughToPay {  get; set; }
        public int Amount { get; set; }
    }
}
