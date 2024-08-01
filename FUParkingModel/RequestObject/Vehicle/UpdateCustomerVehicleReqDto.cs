using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Vehicle
{
    public class UpdateCustomerVehicleReqDto
    {
        [Required]
        public Guid VehicleId { get; set; }

        [FromForm]
        public string? PlateNumber { get; set; }

        [FromForm]
        public Guid? VehicleTypeId { get; set; }
    }
}
