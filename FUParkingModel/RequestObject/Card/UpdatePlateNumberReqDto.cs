using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Card
{
    public class UpdatePlateNumberReqDto
    {
        [Required(ErrorMessage = "Must have PlateNumber")]
        public string PlateNumber { get; set; } = string.Empty;
    }
}
