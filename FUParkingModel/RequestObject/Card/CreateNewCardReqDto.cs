using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Card
{
    public class CreateNewCardReqDto
    {
        [Required(ErrorMessage ="Must have card Number")]
        public string CardNumber { get; set; } = null!;

        public string? PlateNumber { get; set; }
    }
}
