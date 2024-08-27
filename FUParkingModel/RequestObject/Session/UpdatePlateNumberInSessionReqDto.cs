using FUParkingModel.ReturnCommon;
using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Session
{
    public class UpdatePlateNumberInSessionReqDto
    {
        [Required(ErrorMessage = "PlateNumber is required")]
        [RegularExpression("^[0-9]{2}[A-ZĐ]{1,2}[0-9]{4,6}$", ErrorMessage = "Wrong Format PlateNumber")]
        public string PlateNumber { get; set; } = null!;

        [Required(ErrorMessage = "SessionId is required")]
        public Guid SessionId { get; set; }
    }
}
