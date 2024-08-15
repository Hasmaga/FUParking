using Microsoft.EntityFrameworkCore.Query;

namespace FUParkingModel.ResponseObject.Price
{
    public class GetPriceItemResDto
    {
        public Guid Id { get; set; }        
        public int? ApplyFromHour { get; set; }
        public int? ApplyToHour { get; set; }
        public int MaxPrice { get; set; }
        public int BlockPricing { get; set; }
        public int MinPrice { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string LastModifyBy { get; set; } = null!;
        public DateTime? LastModifyDate { get; set; }
    }
}
