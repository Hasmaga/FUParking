using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ReturnObject
{
    public class GoogleReturnAuthenticationResDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string GivenName { get; set; } = null!;
        public bool IsAuthentication { get; set; }
    }
}
