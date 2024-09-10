using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Session
{
    public class CheckOutSessionByPlateNumberReqDto
    {
        [Required(ErrorMessage = "Required PlateNumber")]
        public string PlateNumber { get; set; } = null!;

        [Required(ErrorMessage = "Required CheckOutTime")]
        public DateTime CheckOutTime { get; set; }
        
        public IFormFile? ImagePlate { get; set; }
        
        public IFormFile? ImageBody { get; set; }
        
        public Guid GateId { get; set; }
    }
}
