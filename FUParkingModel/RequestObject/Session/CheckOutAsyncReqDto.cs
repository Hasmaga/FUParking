using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Session
{
    public class CheckOutAsyncReqDto
    {
        [FromForm]
        [Required(ErrorMessage = "Must have CardNumber")]
        public string CardNumber { get; set; } = null!;

        [FromForm]
        [Required(ErrorMessage = "Must have GateOutId")]
        public Guid GateOutId { get; set; }

        [FromForm]
        [Required(ErrorMessage = "Must have TimeOut")]
        public DateTime TimeOut { get; set; }

        [FromForm]
        [Required(ErrorMessage = "Must have ImageOut")]
        public IFormFile ImageOut { get; set; } = null!;

        [FromForm]
        [Required(ErrorMessage = "")]
        public string PlateNumber { get; set; } = null!;
    }
}
