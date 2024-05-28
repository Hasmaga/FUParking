using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class CreateGateReqDto
    {
        [Required]
        public required Guid ParkingAreaId { get; set; }

        [Required]
        public required string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public required Guid GateTypeId { get; set; }
    }
}
