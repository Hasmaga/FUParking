using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class CreatePaymentMethodReqDto
    {
        [Required(ErrorMessage = "Must have Name")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Must have Description")]
        public string Description { get; set; } = null!;        
    }
}
