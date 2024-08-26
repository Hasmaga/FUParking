using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Session
{
    public class GetSessionByPlateNumberResDto
    {        
        public string GateIn { get; set; } = null!;        
        public string ImageInUrl { get; set; } = null!;
        public string ImageInBodyUrl { get; set; } = null!;
        public DateTime TimeIn { get; set; }
        public string VehicleType { get; set; } = null!;
        public string? CustomerEmail { get; set; } = null!;
        public string StaffCheckInEmail { get; set; } = null!;
        public int Amount { get; set; }
    }
}
