﻿using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ResponseObject.Customer;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IAuthService
    {
        Task<Return<LoginResDto>> LoginWithGoogleAsync(GoogleReturnAuthenticationResDto login);
        Task<Return<LoginResDto>> LoginWithCredentialAsync(LoginWithCredentialReqDto req);
        Task<Return<LoginResDto>> CheckRoleByTokenAsync();
        Task<Return<LoginWithGoogleMoblieResDto>> LoginWithGoogleMobileAsync(string one_time_code);
    }
}
