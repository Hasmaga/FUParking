using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FUParkingModel.RequestObject
{
    public class UpdateCoinPackageReqDto
    {
        [JsonIgnore]
        public Guid PackageId { get; set; }

        [Required(ErrorMessage = "Package name is required")]
        public required string Name { get; set; }

        public bool IsActive { get; set; }
    }
}
