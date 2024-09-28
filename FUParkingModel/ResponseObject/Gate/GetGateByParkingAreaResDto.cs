using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Gate
{
    public class GetGateByParkingAreaResDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;     
        public string Status {  get; set; } = null!;
        public Guid ParkingAreaId { get; set; }
    }
}
