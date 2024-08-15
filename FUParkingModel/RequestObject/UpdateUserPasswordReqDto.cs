using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class UpdateUserPasswordReqDto
    {

        [Required]
        [MinLength(8, ErrorMessage = "Password has to be 8 or more character")]
        public string Password { get; set; } = null!;

        [Required]
        [Compare("Password", ErrorMessage = "Password and Confirm Password must be the same")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
