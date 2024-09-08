using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Vehicle
{
    public class UpdateStatusInactiveAndActiveCustomerVehicleByUserReqDto
    {
        [FromBody]
        [Required(ErrorMessage = "Must have VehicleId")]
        public Guid VehicleId { get; set; }

        [FromBody]
        [Required(ErrorMessage = "Must have IsActive")]
        public bool IsActive { get; set; }
    }
}
