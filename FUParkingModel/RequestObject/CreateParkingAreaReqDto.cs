using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class CreateParkingAreaReqDto
    {
        [Required(ErrorMessage = "Must have parking area name")]
        public required string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(1, 4, ErrorMessage = "Mode only 1 to 4")]
        public required int Mode { get; set; }

        [Required(ErrorMessage = "Must have parking area max capacity")]
        [Range(1, int.MaxValue, ErrorMessage = "Max capacity must be greater than 0")]
        public int MaxCapacity { get; set; }

        [Required(ErrorMessage = "Must have parking area block")]
        [Range(1, int.MaxValue, ErrorMessage = "Block must be greater than 0")]
        public int Block { get; set; }
    }
}
