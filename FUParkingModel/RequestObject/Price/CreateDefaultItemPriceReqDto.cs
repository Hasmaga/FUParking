﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject.Price
{
    public class CreateDefaultItemPriceReqDto
    {
        [Required(ErrorMessage = "Must have vehicle Type")]
        public Guid VehicleTypeId { get; set; }       

        [Required(ErrorMessage = "Must have Max Price")]
        [Range(1, int.MaxValue, ErrorMessage = "")]
        public int MaxPrice { get; set; }

        [Required(ErrorMessage = "Must have Min Price")]
        [Range(1, int.MaxValue, ErrorMessage = "")]
        public int MinPrice { get; set; }

        [Required(ErrorMessage = "Must have BlockPricing")]
        [Range(1, int.MaxValue, ErrorMessage = "")]
        public int BlockPricing { get; set; }
    }
}