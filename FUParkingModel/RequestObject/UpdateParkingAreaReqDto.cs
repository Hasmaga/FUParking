using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class UpdateParkingAreaReqDto
    {
        //[Required(ErrorMessage = "Parking area Id must not be null")]
        public Guid ParkingAreaId { get; set; }

        //[Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Mode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max capacity must be greater than 0")]
        public int? MaxCapacity { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Block must be greater than 0")]
        public int? Block { get; set; }
    }
}
