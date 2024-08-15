namespace FUParkingModel.ResponseObject
{
    public class GetPriceTableResDto
    {        
        public Guid Id { get; set; }
        public string VehicleType { get; set; } = null!;
        public int Priority { get; set; }
        public string Name { get; set; } = null!;
        public DateTime? ApplyFromDate { get; set; }
        public DateTime? ApplyToDate { get; set; }
        public string StatusPriceTable { get; set; } = null!;
    }
}
