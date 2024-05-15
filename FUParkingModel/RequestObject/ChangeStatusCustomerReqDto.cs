using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class ChangeStatusCustomerReqDto
    {
        [Required(ErrorMessage = "IsActive must be true or false")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Customer can't not null")]
        public Guid CustomerId { get; set; }
    }
}
