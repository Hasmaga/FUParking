using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Vehicle
{
    public class UpdateCustomerVehicleByUserReqDto : IValidatableObject
    {
        [Required(ErrorMessage = "Must have vehicle Id")]
        public Guid VehicleId { get; set; }
        
        public Guid? VehicleTypeId { get; set; }

        public string? PlateNumber { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (VehicleTypeId == null && PlateNumber == null)
            {
                yield return new ValidationResult("VehicleTypeId or PlateNumber must be present.", [nameof(VehicleTypeId), nameof(PlateNumber)]);
            }
        }
    }
}
