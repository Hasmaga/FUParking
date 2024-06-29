using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class FeedbackReqDto
    {
        [Required(ErrorMessage = "SessionId must have")]
        public Guid SessionId { get; set; }

        [Required(ErrorMessage = "Missing Description")]
        [MaxLength(100, ErrorMessage = "Description too long")]
        public string Description { get; set; } = null!;

        [MaxLength(50, ErrorMessage = "Title too long")]
        public string Title { get; set; } = null!;
    }
}
