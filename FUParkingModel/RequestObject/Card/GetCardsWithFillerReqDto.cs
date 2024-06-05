using FUParkingModel.Enum;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject.Card
{
    public class GetCardsWithFillerReqDto : IValidatableObject
    {
        [FromQuery]
        public int PageSize { get; set; } = Pagination.PAGE_SIZE;

        [FromQuery]
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
