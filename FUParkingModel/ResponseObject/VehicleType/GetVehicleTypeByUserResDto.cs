namespace FUParkingModel.ResponseObject.VehicleType
{
    public class GetVehicleTypeByUserResDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string CreateByEmail { get; set; } = null!;
        public string? LastModifyByEmail { get; set; }
        public DateTime CreateDatetime { get; set; }
        public DateTime? LastModifyDatetime { get; set; }
    }
}
