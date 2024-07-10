using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject.Session
{
    public class CheckInForGuestReqDto
    {
        [Required]
        public string PlateNumber { get; set; } = null!;

        [Required]
        public Guid CardId { get; set; }

        [Required]
        public Guid GateInId { get; set; }

        [Required]
        public IFormFile ImageIn { get; set; } = null!;

        [Required]
        public Guid VehicleTypeId { get; set; }
    }
}
