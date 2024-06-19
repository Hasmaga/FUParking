using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace FUParkingService.Interface
{
    public interface IAuthService
    {
        Task<Return<LoginResDto>> LoginWithGoogleAsync(GoogleReturnAuthenticationResDto login);
        Task<Return<LoginResDto>> LoginWithCredentialAsync(LoginWithCredentialReqDto req);
        Task<Return<LoginResDto>> CheckRoleByTokenAsync();
        Task<Return<Payload>> LoginWithGoogleMobileAsync(string one_time_code);
    }
}
