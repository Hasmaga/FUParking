﻿using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class CreateUserReqDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Must be email format")]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8, ErrorMessage = "Password has to be 8 or more character")]
        public string Password { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;
    }
}
