using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject.Firebase
{
    public class FirebaseReqDto
    {
        public List<string> ClientTokens { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
