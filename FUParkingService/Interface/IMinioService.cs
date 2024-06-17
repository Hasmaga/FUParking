using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IMinioService
    {
        Task<Return<ReturnObjectUrlResDto>> GetObjectUrlByObjectNameAsync(GetObjectReqDto req);
        Task<Return<ReturnObjectUrlResDto>> UploadObjectAsync(UploadObjectReqDto req);
        Task<Return<bool>> DeleteObjectAsync(DeleteObjectReqDto req);
    }
}
