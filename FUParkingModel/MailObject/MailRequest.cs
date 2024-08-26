using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingService.MailObject
{
    public class MailRequest
    {
        public required string toEmail { get; set; }
        public required string toUsername { get; set; }
        public required string subject { get; set; }
        public required string body { get; set; }
    }
}
