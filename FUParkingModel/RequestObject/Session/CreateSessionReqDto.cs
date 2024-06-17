using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Session
{
    public class CreateSessionReqDto
    {
        [Required(ErrorMessage = "Must have CardId")]
        public Guid CardId { get; set; }

        [Required(ErrorMessage = "Must have GateInId")]
        public Guid GateInId { get; set; }

        [Required(ErrorMessage = "Must have PlateNumber")]
        public string PlateNumber { get; set; } = null!;

        [Required(ErrorMessage = "Must have ImageInUrl")]
        public IFormFile ImageIn { get; set; } = null!;
    }
}
