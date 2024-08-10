using FUParkingModel.Enum;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Common
{
    public class GetListObjectWithFiller : IValidatableObject
    {
        [FromQuery]
        [Range(0, int.MaxValue, ErrorMessage = "PageSize must be a non-negative value.")]
        public int PageSize { get; set; } = Pagination.PAGE_SIZE;

        [FromQuery]
        [Range(0, int.MaxValue, ErrorMessage = "PageSize must be a non-negative value.")]
        public int PageIndex { get; set; } = Pagination.PAGE_INDEX;

        [FromQuery]
        public string? SearchInput { get; set; }

        [FromQuery]
        public string? Attribute { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(SearchInput) && string.IsNullOrEmpty(Attribute))
            {
                yield return new ValidationResult("Attribute is required when SearchInput is present.", [nameof(Attribute)]);
            }
        }
    }
}
