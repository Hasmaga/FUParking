using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Common
{
    public class GetListObjectWithFillerDateAndSearchInputResDto
    {
        [FromQuery]
        public DateTime? StartDate { get; set; }

        [FromQuery]
        public DateTime? EndDate { get; set; }

        [FromQuery]
        public string? SearchInput { get; set; }

        [FromQuery]
        public string? Attribute { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext _)
        {
            if (string.IsNullOrWhiteSpace(SearchInput) && string.IsNullOrEmpty(Attribute))
            {
                yield return new ValidationResult("Attribute is required when SearchInput is present.", [nameof(Attribute)]);
            }
            if (StartDate.HasValue && EndDate.HasValue)
            {
                if (StartDate.Value > EndDate.Value)
                {
                    yield return new ValidationResult("StartDate must be less than EndDate.", [nameof(StartDate), nameof(EndDate)]);
                }
            }
        }
    }
}
