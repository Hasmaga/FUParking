using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class ChangeStatusPriceTableResDto
    {
        [Required(ErrorMessage = "Price Table Id must not be null")]
        public Guid PriceTableId { get; set; }

        [Required(ErrorMessage = "IsActive must not be null")]
        public bool IsActive { get; set; }
    }
}
