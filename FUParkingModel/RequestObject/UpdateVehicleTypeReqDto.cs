using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class UpdateVehicleTypeReqDto
    {
        [Required(ErrorMessage = "Must have vehicle type's id")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Must have vehicle type's name")]
        public required String Name { get; set; }

        public String? Description { get; set; }
    }
}
