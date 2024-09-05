using FUParkingModel.RequestObject.Customer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject.Vehicle
{
    public class CreateListVehicleForCustomerByUserReqDto
    {
        [Required(ErrorMessage = "Must have 1 vehicle")]
        public CreateVehiclesNonPriceResDto[] Vehicles { get; set; } = null!;

        [Required(ErrorMessage = "CustomerId is required")]
        public Guid CustomerId { get; set; }
    }
}
