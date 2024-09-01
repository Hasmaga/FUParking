using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.Card
{
    public class GetCardOptionsResDto
    {
        public Guid Id { get; set; }
        public string CardNumber { get; set; } = null!;
    }
}
