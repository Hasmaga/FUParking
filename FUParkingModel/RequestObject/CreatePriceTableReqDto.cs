using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class CreatePriceTableReqDto
    {
        [Required(ErrorMessage = "Must have vehicle type")]
        public Guid VehicleTypeId { get; set; }

        [Required(ErrorMessage = "Must have Priority")]
        [Range(2, 5, ErrorMessage = "Priority is from 2 to 5")]
        public int Priority { get; set; }

        [Required(ErrorMessage = "Must have Name")]
        public string Name { get; set; } = null!;

        public DateTime? ApplyFromDate { get; set; }

        [Compare("ApplyFromDate", ErrorMessage = "Apply To Date must be greater than Apply From Date")]
        public DateTime? ApplyToDate { get; set; }

        [Required(ErrorMessage = "Must have price per block default")]
        public int PricePerBlock { get; set; }

        [Required(ErrorMessage = "Must have max price default")]
        public int MaxPrice { get; set; }

        [Required(ErrorMessage = "Must have min price default")]
        public int MinPrice { get; set; }
    }
}
