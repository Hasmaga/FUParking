using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject
{
    public class GetListFeedbacksResDto
    {
        public required string CustomerName { get; set; }
        public required string ParkingAreaName { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string createdDate { get; set; }
    }
}
