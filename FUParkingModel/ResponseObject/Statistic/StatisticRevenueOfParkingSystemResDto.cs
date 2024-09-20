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

        public int TotalRevenue { get; set; }

        public int WalletRevenue { get; set; } 

        public int OtherRevenue { get; set; }

        public int AverageRevenue { get; set; }
    }
}
