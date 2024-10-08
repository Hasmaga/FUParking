﻿using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class CustomerReqDto
    {
        [Required(ErrorMessage = "Name is missing")]
        public required string Name { get; set; }
        [EmailAddress(ErrorMessage = "Not a valid Email")]
        [Required(ErrorMessage = "Email is missing")]
        public required string Email { get; set; }

        [RegularExpression(@"0\d{9}", ErrorMessage = "Not a valid phone number")]
        public string? Phone { get; set; }
    }
}
