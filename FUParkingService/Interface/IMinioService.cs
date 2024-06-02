﻿using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingModel.ResponseObject;

namespace FUParkingService.Interface
{
    public interface IMinioService
    {
        Task<Return<ReturnObjectUrlResDto>> GetObjectUrlByObjectNameAsync(GetObjectReqDto req);
        Task<Return<ReturnObjectUrlResDto>> UploadObjectAsync(UploadObjectReqDto req);
        Task<Return<bool>> DeleteObjectAsync(DeleteObjectReqDto req);
    }
}
