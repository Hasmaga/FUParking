using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class ChangeStatusParkingAreaReqDto
    {
        [Required(ErrorMessage = "Parking area Id must not be null")]
        public Guid ParkingAreaId { get; set; }

        [Required(ErrorMessage = "IsActive must not be null")]
        public bool IsActive { get; set; }
    }
}
