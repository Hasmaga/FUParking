using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingService.MailObject
{
    public class MailRequest
    {
        public required string ToEmail { get; set; }
        public required string ToUsername { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
    }
}
