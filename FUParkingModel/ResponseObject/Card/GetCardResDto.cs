﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Card
{
    public class GetCardResDto
    {
        public Guid Id { get; set; }
        public string CardNumber { get; set; } = null!;
        public string? PlateNumber { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}