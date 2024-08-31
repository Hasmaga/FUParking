using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Statistic
{
    public class StatisticRevenueParkingAreasDetailsResDto
    {
        public Guid ParkingAreaId { get; set; }
        public string ParkingAreaName { get; set; } = null!;
        public int RevenueTotal { get; set; }
        public int RevenueApp { get; set; }
        public int RevenueOther { get; set; }
    }
}
