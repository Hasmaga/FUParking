using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class CreateParkingAreaReqDto
    {
        [Required(ErrorMessage = "Must have parking area name")]
        public required String Name { get; set; }

        public String? Description { get; set; }

        [Required(ErrorMessage = "Must have parking area max capacity")]
        [Range(1, int.MaxValue, ErrorMessage = "Max capacity must be greater than 0")]
        public int MaxCapacity { get; set; }

        [Required(ErrorMessage = "Must have parking area block")]
        [Range(1, int.MaxValue, ErrorMessage = "Block must be greater than 0")]
        public int Block { get; set; }
    }
}
