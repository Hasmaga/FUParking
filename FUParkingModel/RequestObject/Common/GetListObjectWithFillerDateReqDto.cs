using FUParkingModel.Enum;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Common
{
    public class GetListObjectWithFillerDateReqDto : IValidatableObject
    {
        [FromQuery]
        [Range(0, int.MaxValue, ErrorMessage = "PageSize must be a non-negative value.")]
        public int PageSize { get; set; } = Pagination.PAGE_SIZE;

        [FromQuery]
        [Range(0, int.MaxValue, ErrorMessage = "PageSize must be a non-negative value.")]
        public int PageIndex { get; set; } = Pagination.PAGE_INDEX;

        [FromQuery]
        [Range(0, int.MaxValue, ErrorMessage = "PageSize must be a non-negative value.")]
        public DateTime? StartDate { get; set; }

        [FromQuery]
        [Range(0, int.MaxValue, ErrorMessage = "PageSize must be a non-negative value.")]
        public DateTime? EndDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
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
