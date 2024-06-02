using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class DeleteObjectReqDto
    {
        public string BucketName { get; set; } = null!;
        public string ObjName { get; set; } = null!;
    }
}
