using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class BuyPackageReqDto
    {
        [Required(ErrorMessage = "Package not valid")]
        public string packageId { get; set; }

    }
}
