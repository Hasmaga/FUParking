using FUParkingModel.ResponseObject.Vehicle;

namespace FUParkingModel.ResponseObject.Session
{
    public class GetCustomerTypeByPlateNumberResDto
    {
        public string? CustomerType { get; set; }
        public PreviousSessionInfo? PreviousSessionInfo { get; set; }
        public GetVehicleInformationByStaffResDto? InformationVehicle {  get; set; }
    }

    public class PreviousSessionInfo
    {        
        public string CardOrPlateNumber { get; set; } = null!;
    }
}
