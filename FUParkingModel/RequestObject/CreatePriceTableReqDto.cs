using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class CreatePriceTableReqDto
    {
        [Required(ErrorMessage = "Must have vehicle type")]
        public Guid VehicleTypeId { get; set; }

        [Required(ErrorMessage = "Must have Priority")]
        public int Priority { get; set; }

        [Required(ErrorMessage = "Must have Name")]
        public string Name { get; set; } = null!;
                
        public DateTime? ApplyFromDate { get; set; }

        public DateTime? ApplyToDate { get; set; }
    }
}
