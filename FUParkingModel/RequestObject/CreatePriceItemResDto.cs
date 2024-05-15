using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class CreatePriceItemResDto
    {
        [Required(ErrorMessage = "PriceTable must not be null")]
        public Guid PriceTableId { get; set; }

        public TimeOnly? ApplyFromHour { get; set; }

        public TimeOnly? ApplyToHour { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxPrice must be greater than or equal to 0")]
        public int MaxPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MinPrice must be greater than or equal to 0")]
        public int MinPrice { get; set; }
    }
}
