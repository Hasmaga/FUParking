using FUParkingModel.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Statistic
{
    public class StatisticRevenueOfParkingAreaSystemDetailResDto
    {
        public required string PaymentMethod { get; set; }
        public List<GateDetailDto>? Gates { get; set; }
        public int total { get; set; }
    }

    public class GateDetailDto
    {
        public required string Name { get; set; }
        public int Revenue { get; set; } = 0;
    }
}
