using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
