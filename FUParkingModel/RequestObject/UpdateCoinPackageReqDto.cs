using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class UpdateCoinPackageReqDto
    {
        [Required(ErrorMessage = "Package name is required")]
        public required string Name { get; set; }
        public bool isActive { get; set; }
    }
}
