using FUParkingModel.ResponseObject.ParkingArea;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Statistic
{
    public class StatisticParkingAreaRevenueResDto
    {
        public GetParkingAreaOptionResDto ParkingArea { get; set; } = null!;
        public int Revenue { get; set; }
    }
}
