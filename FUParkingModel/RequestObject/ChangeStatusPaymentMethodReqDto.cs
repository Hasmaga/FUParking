using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject
{
    public class ChangeStatusPaymentMethodReqDto
    {
        [Required(ErrorMessage = "PaymentMethodId must not be null")]
        public Guid PaymentMethodId { get; set; }

        [Required(ErrorMessage = "IsActive must not be null")]
        public bool IsActive { get; set; }
    }
}
