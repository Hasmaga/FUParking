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
            var priceItemsWithDefinedTimes = PriceItems
                .Where(item => item.From.HasValue && item.To.HasValue)
                .ToList();

            for (int i = 0; i < priceItemsWithDefinedTimes.Count; i++)
            {
                for (int j = i + 1; j < priceItemsWithDefinedTimes.Count; j++)
                {
                    var first = priceItemsWithDefinedTimes[i];
                    var second = priceItemsWithDefinedTimes[j];

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
