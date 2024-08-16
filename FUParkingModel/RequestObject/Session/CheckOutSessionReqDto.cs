using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Session
{
    public class CheckOutSessionReqDto
    {
        [Required(ErrorMessage = "Must have plate number")]
        [RegularExpression("^[0-9]{2}[A-ZĐ]{1,2}[0-9]{4,6}$", ErrorMessage = "Wrong Format PlateNumber")]
        public string PlateNumber { get; set; } = null!;

        [Required(ErrorMessage = "Must have Time Out")]
        public DateTime TimeOut { get; set; }
    }
}
