using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class CreateVehicleTypeReqDto
    {
        [Required(ErrorMessage = "Must have vehicle type's name")]
        public required String Name { get; set; }

        public String? Description { get; set; }
    }
}
