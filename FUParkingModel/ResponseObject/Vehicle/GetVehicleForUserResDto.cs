namespace FUParkingModel.ResponseObject.Vehicle
{
    public class GetVehicleForUserResDto
    {
        public Guid Id { get; set; }
        public string PlateNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public Guid VehicleTypeId { get; set; } 
        public string PlateImage { get; set; } = null!;
        public string StatusVehicle { get; set; } = null!;
        public string StaffApproval { get; set; } = null!;
        public string? LastModifyBy { get; set; }
        public DateTime? LastModifyDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
