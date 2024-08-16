using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Vehicle
{
    public class UpdateCustomerVehicleByUserReqDto
    {
        [Required(ErrorMessage = "Must have vehicle Id")]
        public Guid VehicleId { get; set; }

        [Required(ErrorMessage = "Must have vehicle type Id")]
        public Guid VehicleTypeId { get; set; }
    }
}
