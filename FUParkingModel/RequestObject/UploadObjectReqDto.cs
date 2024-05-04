using Microsoft.AspNetCore.Http;

namespace FUParkingModel.RequestObject
{
    public class UploadObjectReqDto
    {
        public string BucketName { get; set; } = null!;
        public string ObjName { get; set; } = null!;
        public IFormFile ObjFile { get; set; } = null!;
    }
}
