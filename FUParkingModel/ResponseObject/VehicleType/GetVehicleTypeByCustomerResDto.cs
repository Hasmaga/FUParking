namespace FUParkingModel.ResponseObject.VehicleType
{
    public class GetVehicleTypeByCustomerResDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
