using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Price
{
    public class UpdatePriceTableReqDto : IValidatableObject
    {
        public Guid PriceTableId { get; set; }
        public string? Name { get; set; }    
        public DateTime? ApplyFromDate { get; set; }
        public DateTime? ApplyToDate { get; set; }

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
