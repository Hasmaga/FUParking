using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class CreateGateReqDto
    {
        [Required]
        public required Guid ParkingAreaId { get; set; }

        [Required]
        public required string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public required Guid GateTypeId { get; set; }
    }
}
