using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class CreatePriceItemReqDto : IValidatableObject
    {        
        [Range(0, 24, ErrorMessage = "Range is from 0 to 24")]
        public int From { get; set; } // Hour

        [Range(0, 24, ErrorMessage = "Range is from 0 to 24")]
        public int To { get; set; } // Hour

        [Range(0, int.MaxValue, ErrorMessage = "MaxPrice must be greater than or equal to 0")]
        public int MaxPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MinPrice must be greater than or equal to 0")]
        public int MinPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "BlockPricing must be greater than or equal to 0")]
        public int BlockPricing { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (From >= To)
            {
                yield return new ValidationResult(
                    "The 'From' hour must be less than the 'To' hour.",
                    [nameof(From), nameof(To)]);
            }
        }
    }
}
