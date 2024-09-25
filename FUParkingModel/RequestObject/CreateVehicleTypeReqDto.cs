using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class CreateVehicleTypeReqDto
    {
        [Required(ErrorMessage = "Must have vehicle type's name")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Must have BlockPricing")]
        public int BlockPricing { get; set; }

        [Required(ErrorMessage = "Must have MaxPrice")]
        public int MaxPrice { get; set; }

        [Required(ErrorMessage = "Must have MinPrice")]
        public int MinPrice { get; set; }
    }
}
