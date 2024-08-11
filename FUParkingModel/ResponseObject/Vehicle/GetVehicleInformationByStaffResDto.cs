namespace FUParkingModel.ResponseObject.Vehicle
{
    public class GetVehicleInformationByStaffResDto
    {
        public string PlateNumber { get; set; } = null!;
        public Guid VehicleType { get; set; }
        public string PlateImage { get; set; } = null!;
        public string StatusVehicle { get; set; } = null!;
        public DateTime CreateDate { get; set; }
    }
}
