using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Statistic
{
    public class StatisticPaymentByCustomerResDto
    {
        public int TotalPaymentInThisMonth { get; set; }
        public int TotalTimePakedInThisMonth { get; set; }
    }
}
