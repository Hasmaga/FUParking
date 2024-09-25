namespace FUParkingModel.ResponseObject.Vehicle
{
    public class GetCustomerVehicleByCustomerResDto
    {
        public Guid Id { get; set; }
        public string VehicleTypeName { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public string PlateImage { get; set; } = null!;
        public string StatusVehicle { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public Guid VehicleTypeId { get; set; }
    }
}
