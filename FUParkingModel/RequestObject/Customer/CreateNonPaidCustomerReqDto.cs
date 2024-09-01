using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject.Customer
{
    public class CreateNonPaidCustomerReqDto : IValidatableObject
    {
        [FromBody]
        [Required(ErrorMessage = "Must have name")]
        public string Name { get; set; } = null!;

        [FromBody]
        [Required(ErrorMessage = "Must have email")]
        public string Email { get; set; } = null!;

        [FromBody]
        public string? PlateNumber { get; set; } = null!;

        [FromBody]
        public Guid? VehicleTypeId { get; set; }

        [FromBody]
        public Guid? CardId { get; set; }

        [FromBody]
        public string? CardNumber { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(PlateNumber) && !VehicleTypeId.HasValue)
            {
                yield return new ValidationResult(
                    "VehicleTypeId must be provided if PlateNumber is provided.",
                    [nameof(VehicleTypeId)]);
            }

            if (VehicleTypeId.HasValue && string.IsNullOrEmpty(PlateNumber))
            {
                yield return new ValidationResult(
                    "PlateNumber must be provided if VehicleTypeId is provided.",
                    [nameof(PlateNumber)]);
            }
        }
    }
}
