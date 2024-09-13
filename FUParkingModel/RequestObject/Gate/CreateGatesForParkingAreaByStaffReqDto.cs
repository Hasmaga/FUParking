using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Gate
{
    public class CreateGatesForParkingAreaByStaffReqDto
    {
        [Required(ErrorMessage = "Must have ParkingAreaId")]
        public Guid ParkingAreaId { get; set; }

        [Required(ErrorMessage = "Must have 1 Gates")]
        public ListGateRegister[] Gates { get; set; } = null!;
    }    

    public class ListGateRegister
    {
        [Required(ErrorMessage = "Must have Name")]
        public required string Name { get; set; }

        public string? Description { get; set; }
    }
}
