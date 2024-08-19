using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class UpdateCustomerAccountReqDto
    {
        [Required(ErrorMessage = "Name Must have")]
        public required string FullName { get; set; }
    }
}
