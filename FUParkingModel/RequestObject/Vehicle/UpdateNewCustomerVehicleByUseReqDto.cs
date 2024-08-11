using Microsoft.AspNetCore.Mvc;
namespace FUParkingModel.RequestObject.Vehicle
{
    public class UpdateNewCustomerVehicleByUseReqDto
    {
        [FromBody]
        public Guid? VehicleType { get; set; }

        [FromBody]
        public string PlateNumber { get; set; } = null!;

        [FromBody]
        public bool IsAccept { get; set; }
    }
}
