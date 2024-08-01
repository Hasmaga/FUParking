using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Session
{
    public class CheckInForGuestReqDto
    {
        [FromForm]
        [Required(ErrorMessage = "Must have PlateNumber")]
        public string PlateNumber { get; set; } = null!;

        [FromForm]
        [Required(ErrorMessage = "Must have CardId")]
        public string CardNumber { get; set; } = null!;

        [FromForm]
        [Required(ErrorMessage = "Must have GateInId")]
        public Guid GateInId { get; set; }

        [FromForm]
        [Required(ErrorMessage = "Must have ImageIn")]
        public IFormFile ImageIn { get; set; } = null!;

        [FromForm]
        [Required(ErrorMessage = "Must have VehicleTypeId")]
        public Guid VehicleTypeId { get; set; }
    }
}
