using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class CustomerReqDto
    {
        [Required(ErrorMessage ="Name is missing")]
        public required string Name {  get; set; }
        [EmailAddress(ErrorMessage ="Not a valid Email")]
        [Required(ErrorMessage = "Email is missing")]
        public required string Email { get; set; }

        [RegularExpression(@"0\d{9}", ErrorMessage = "Not a valid phone number")]
        public string? Phone { get; set; }
        [Required(ErrorMessage ="Password is missing")]
        [MinLength(5, ErrorMessage ="Password must be 5 or more characters")]
        public required string Password { get; set; }
    }
}
