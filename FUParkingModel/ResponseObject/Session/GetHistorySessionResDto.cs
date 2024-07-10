using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Session
{
    public class GetHistorySessionResDto
    {
        public DateTime TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public string Status { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public int? Amount { get; set; }
    }
}
