using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Price
{
    public class CreateListPriceItemReqDto : IValidatableObject
    {
        [Required]
        public Guid PriceTableId { get; set; }

        [Required]
        public List<CreatePriceItemReqDto> PriceItems { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            for (int i = 0; i < PriceItems.Count; i++)
            {
                for (int j = i + 1; j < PriceItems.Count; j++)
                {
                    var first = PriceItems[i];
                    var second = PriceItems[j];

                    if (first.From < second.To && second.From < first.To)
                    {
                        yield return new ValidationResult(
                            $"The price item from {first.From} to {first.To} overlaps with another item from {second.From} to {second.To}.",
                            [nameof(PriceItems)]);
                    }
                }
            }
        }
    }
}
