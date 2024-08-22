using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Card
{
    public class GetCardByCardNumberResDto
    {
        public required string cardNumber { get; set; }
        public string? plateNumber { get; set; }
        public required string status { get; set; }
        public Guid? sessionId { get; set; }
        public string? sessionPlateNumber { get; set; }
        public string? sessionVehicleType { get; set; }
        public string? sessionTimeIn { get; set; }
        public string? sessionGateIn { get; set; }
        public string? sessionCustomerName { get; set; }
        public string? sessionCustomerEmail { get; set; }
    }
}
