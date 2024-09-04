using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject.Customer
{
    public class CreateNonPaidCustomerReqDto 
    {
        [FromBody]
        [Required(ErrorMessage = "Must have name")]
        public string Name { get; set; } = null!;

        [FromBody]
        [Required(ErrorMessage = "Must have email")]
        public string Email { get; set; } = null!;

        [FromBody]
        public CreateVehiclesNonPriceResDto[]? Vehicles { get; set; }
    }

    public class CreateVehiclesNonPriceResDto
    {
        [FromBody]
        [Required(ErrorMessage = "Must have PlateNumber")]
        public string PlateNumber { get; set; } = null!;

        [FromBody]
        [Required(ErrorMessage = "Must have VehicleTypeId")]
        public Guid VehicleTypeId { get; set; }        
    }
}
