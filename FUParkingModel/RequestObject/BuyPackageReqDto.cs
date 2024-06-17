using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class BuyPackageReqDto
    {
        [Required(ErrorMessage = "Missing Package")]
        public required string PackageId { get; set; }

    }
}
