using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject.ParkingArea
{
    public class UpdateStatusParkingAreaReqDto
    {
        [Required(ErrorMessage = "Must have parkingId")]
        public Guid ParkingId { get; set; }

        [Required(ErrorMessage = "Must have isActive")]
        public bool IsActive { get; set; }
    }
}
