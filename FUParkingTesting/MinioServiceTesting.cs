using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingService;

namespace FUParkingTesting
{
    [TestClass]
    public class MinioServiceTesting
    {
        private readonly MinioService _minioService;

        public MinioServiceTesting()
        {
            _minioService = new MinioService();
        }

        [TestMethod]
        public async Task TestGetObjectByName_ShouldReturnObjectUrl()
        {
            var objName = "LogoTesting.jpg";
            var bucketName = "parking";
            var req = new GetObjectReqDto
            {
                ObjName = objName,
                BucketName = bucketName
            };
            var result = await _minioService.GetObjectUrlByObjectNameAsync(req);
            var expected = "https://miniofile.khangbpa.com/" + req.BucketName + "/" + req.ObjName;
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.IsNotNull(result.Data.ObjUrl);
            Assert.AreEqual(expected, result.Data.ObjUrl);
        }

        [TestMethod]
        public async Task TestGetObjectByName_ShouldReturnNotFound()
        {
            var objName = "LogoTestingNotFound.jpg";
            var bucketName = "parking";
            var req = new GetObjectReqDto
            {
                ObjName = objName,
                BucketName = bucketName
            };
            var result = await _minioService.GetObjectUrlByObjectNameAsync(req);
            Assert.AreEqual(ErrorEnumApplication.SERVER_ERROR, result.Message);
            Assert.AreEqual(MinioErrorApplicationDefineEnum.NOT_FOUND, result.InternalErrorMessage);
        }
    }
}