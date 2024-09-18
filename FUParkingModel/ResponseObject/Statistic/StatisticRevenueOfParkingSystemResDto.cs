using FUParkingModel.ResponseObject.ParkingArea;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Statistic
{
    public class StatisticRevenueOfParkingSystemResDto
    {
        public GetParkingAreaOptionResDto ParkingArea { get; set; } = null!;

        public int totalRevenue { get; set; }

        public int walletRevenue { get; set; } 

        public int otherRevenue { get; set; }

        public int averageRevenue { get; set; }
    }
}
