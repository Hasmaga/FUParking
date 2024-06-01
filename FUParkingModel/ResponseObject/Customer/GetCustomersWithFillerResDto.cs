using System.Data;

namespace FUParkingModel.ResponseObject.Customer
{
    public class GetCustomersWithFillerResDto
    {
        public Guid CustomerId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";         
        public string StatusCustomer { get; set; } = "";
        public string CustomerType { get; set; } = "";       
        public DateOnly CreateDate { get; set; }
    }
}
