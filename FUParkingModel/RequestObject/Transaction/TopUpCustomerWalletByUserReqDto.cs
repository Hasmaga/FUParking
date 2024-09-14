using System.ComponentModel.DataAnnotations;

namespace FUParkingModel.RequestObject.Transaction
{
    public class TopUpCustomerWalletByUserReqDto
    {
        [Required(ErrorMessage = "Must have customerId")]
        public Guid CustomerId { get; set; }

        [Required(ErrorMessage = "Must have amount")]
        public int Amount { get; set; }
    }
}
