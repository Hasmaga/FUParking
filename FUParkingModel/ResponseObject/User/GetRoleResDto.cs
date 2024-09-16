using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.ResponseObject.User
{
    public class GetRoleResDto
    {
        public Guid RoleId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
