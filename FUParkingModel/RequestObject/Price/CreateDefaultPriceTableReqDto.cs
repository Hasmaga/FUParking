using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Price
{
    public class CreateDefaultPriceTableReqDto
    {
        [Required(ErrorMessage = "Must have VehicleTypeId")]
        public Guid VehicleTypeId { get; set; }
    }
}
