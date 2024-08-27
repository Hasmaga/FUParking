using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Card
{
    public class CreateNewCardReqDto
    {
        [Required(ErrorMessage = "Must have card Number")]
        public string CardNumber { get; set; } = null!;

        [RegularExpression("^[0-9]{2}[A-ZĐ]{1,2}[0-9]{4,6}$", ErrorMessage = "Wrong Format PlateNumber")]
        public string? PlateNumber { get; set; }
    }
}
