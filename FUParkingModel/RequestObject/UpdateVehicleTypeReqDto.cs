using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class UpdateVehicleTypeReqDto
    {
        [Required(ErrorMessage = "Must have id")]
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
