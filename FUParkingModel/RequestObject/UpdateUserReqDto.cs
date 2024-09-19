using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class UpdateUserReqDto
    {
        [Required]
        public Guid Id { get; set; }
        
        public string? FullName { get; set; }
        
        [EmailAddress(ErrorMessage = "Must be email format")]
        public string? Email { get; set; }

        public string? Password { get; set; }

        public Guid? RoleId { get; set; }
    }
}
