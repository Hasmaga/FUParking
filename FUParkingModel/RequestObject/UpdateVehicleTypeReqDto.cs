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
        public String? Name { get; set; }
        public String? Description { get; set; }
    }
}
