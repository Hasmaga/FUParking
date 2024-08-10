using FUParkingModel.Enum;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Common
{
    public class GetListObjectWithPageReqDto
    {
        [FromQuery]
        [Range(0, int.MaxValue, ErrorMessage = "PageSize must be a non-negative value.")]
        public int PageSize { get; set; } = Pagination.PAGE_SIZE;

        [FromQuery]
        [Range(0, int.MaxValue, ErrorMessage = "PageSize must be a non-negative value.")]
        public int PageIndex { get; set; } = Pagination.PAGE_INDEX;
    }
}
