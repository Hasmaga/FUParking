namespace FUParkingModel.ResponseObject.Session
{
    public class GetSessionByCardNumberResDto
    {
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public string GateIn { get; set; } = null!;        
        public string PlateNumber { get; set; } = null!;
        public string ImageInUrl { get; set; } = null!;
        public string ImageInBodyUrl { get; set; } = null!;        
        public DateTime TimeIn { get; set; }        
        public string VehicleType { get; set; } = null!;        
        public int Amount { get; set; }
        public string customerType { get; set; } = null!;
    }
}
