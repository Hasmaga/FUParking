﻿using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class UpdateParkingAreaReqDto
    {
        [Required(ErrorMessage = "Must have parking Area")]
        public Guid ParkingAreaId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        [Range(1, 4, ErrorMessage = "Range 1 to 4")]
        public int? Mode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max capacity must be greater than 0")]
        public int? MaxCapacity { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Block must be greater than 0")]
        public int? Block { get; set; }
    }
}
