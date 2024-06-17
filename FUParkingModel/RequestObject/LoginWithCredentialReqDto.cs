using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class LoginWithCredentialReqDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Must be email format")]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
