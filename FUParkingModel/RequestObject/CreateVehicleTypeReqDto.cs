using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class CreateVehicleTypeReqDto
    {
        [Required(ErrorMessage = "Must have vehicle type's name")]
        public required String Name { get; set; }

        public String? Description { get; set; }
    }
}
