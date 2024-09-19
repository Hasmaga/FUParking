using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.User
{
    public class UpdateStatusUserReqDto
    {
        [Required(ErrorMessage = "Must have userId")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "IsActive must be true or false")]
        public bool IsActive { get; set; }
    }
}
