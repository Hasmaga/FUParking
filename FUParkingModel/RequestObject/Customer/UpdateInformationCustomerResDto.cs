using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject.Customer
{
    public class UpdateInformationCustomerResDto
    {
        [Required(ErrorMessage = "Must have CustomerId")]
        public Guid CustomerId { get; set; }
        
        public Guid? CustomerTypeId { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }
    }
}
