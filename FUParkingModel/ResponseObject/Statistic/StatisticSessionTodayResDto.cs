using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Statistic
{
    public class StatisticSessionTodayResDto
    {
        public int TotalCheckInToday { get; set; }
        public int TotalCheckOutToday { get; set; }

        public int TotalVehicleParked { get; set; }
    }
}
