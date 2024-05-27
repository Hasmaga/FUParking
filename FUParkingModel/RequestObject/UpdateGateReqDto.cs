using FUParkingModel.Object;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class UpdateGateReqDto
    {
        [Required(ErrorMessage = "Gate Type cannot null")]
        public Guid GateTypeId { get; set; }

        public string? Description { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Parking Area cannot null")]
        public Guid ParkingAreaId { get; set; }
    }
}