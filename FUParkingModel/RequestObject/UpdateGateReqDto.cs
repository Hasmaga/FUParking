using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class UpdateGateReqDto
    {
        [Required(ErrorMessage = "Gate Type cannot null")]
        public Guid GateTypeId { get; set; }

        public string? Description { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Parking Area cannot null")]
        public Guid ParkingAreaId { get; set; }
    }
}