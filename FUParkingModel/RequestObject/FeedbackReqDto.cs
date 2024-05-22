using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class FeedbackReqDto
    {
        [Required(ErrorMessage ="Missing Parking Area")]
        public required string ParkingAreaId {  get; set; }

        [Required(ErrorMessage = "Missing Description")]
        [MaxLength(100, ErrorMessage ="Description too long")]
        public string? Description { get; set; }

        [MaxLength(50, ErrorMessage ="Title too long")]
        public string? Title { get; set; }
    }
}
