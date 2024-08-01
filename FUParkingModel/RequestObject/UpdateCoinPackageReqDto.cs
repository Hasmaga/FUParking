using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class UpdateCoinPackageReqDto
    {
        [Required(ErrorMessage = "CoinPackage Must have")]
        public Guid PackageId { get; set; }

        public string? Name { get; set; }

        public bool? IsActive { get; set; }
    }
}
