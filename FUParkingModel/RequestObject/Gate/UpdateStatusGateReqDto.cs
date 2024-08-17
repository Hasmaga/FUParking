using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Gate
{
    public class UpdateStatusGateReqDto
    {
        [Required(ErrorMessage = "Must have gateId")]
        public Guid GateId { get; set; }

        [Required(ErrorMessage = "Must have isActive")]
        public bool IsActive { get; set; }
    }
}
