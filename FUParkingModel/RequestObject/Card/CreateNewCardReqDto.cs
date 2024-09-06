using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Card
{
    public class CreateNewCardReqDto
    {
        [Required(ErrorMessage = "Must have card Number")]
        public string[] CardNumbers { get; set; } = null!;
    }    
}
