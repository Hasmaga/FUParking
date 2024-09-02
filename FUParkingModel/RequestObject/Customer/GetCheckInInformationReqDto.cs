using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject.Customer
{
    public class GetCheckInInformationReqDto
    {
        public string PlateNumber { get; set; } = null!;
        public string CardNumber { get; set; } = null!;
    }
}
