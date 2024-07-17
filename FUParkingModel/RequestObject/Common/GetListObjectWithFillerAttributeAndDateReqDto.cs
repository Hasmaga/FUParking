using FUParkingModel.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject.Common
{
    public class GetListObjectWithFillerAttributeAndDateReqDto
    {
        public int PageSize { get; set; } = Pagination.PAGE_SIZE;
        public int PageIndex { get; set; } = Pagination.PAGE_INDEX;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchInput { get; set; }
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
