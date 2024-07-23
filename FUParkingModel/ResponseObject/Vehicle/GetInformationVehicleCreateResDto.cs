namespace FUParkingModel.ResponseObject.Vehicle
{
    public class GetInformationVehicleCreateResDto
    {
        public Guid VehicleId { get; set; }
        public string ImagePlateNumber { get; set; } = null!;
        public string VehicleTypeName { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
    }
}
