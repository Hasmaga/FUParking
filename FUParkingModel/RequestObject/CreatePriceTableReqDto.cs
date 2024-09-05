using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class CreatePriceTableReqDto : IValidatableObject
    {
        [Required(ErrorMessage = "Must have vehicle type")]
        public Guid VehicleTypeId { get; set; }

        [Required(ErrorMessage = "Must have Priority")]
        [Range(2, 5, ErrorMessage = "Priority is from 2 to 5")]
        public int Priority { get; set; }

        [Required(ErrorMessage = "Must have Name")]
        public string Name { get; set; } = null!;
        
        public DateTime? ApplyFromDate { get; set; }       
        
        public DateTime? ApplyToDate { get; set; }

        [Required(ErrorMessage = "Must have price per block default")]
        public int PricePerBlock { get; set; }

        [Required(ErrorMessage = "Must have max price default")]
        public int MaxPrice { get; set; }

        [Required(ErrorMessage = "Must have min price default")]
        public int MinPrice { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ApplyFromDate is not null && ApplyToDate is not null)
            {
                if (ApplyFromDate > ApplyToDate)
                {
                    yield return new ValidationResult("ApplyFromDate must be less than ApplyToDate", [nameof(ApplyFromDate), nameof(ApplyToDate)]);
                }
            }
        }
    }    
}
