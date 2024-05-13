using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingModel.ResponseObject;

namespace FUParkingService.Interface
{
    public interface IAuthService
    {
        Task<Return<LoginResDto>> LoginWithGoogleAsync(GoogleReturnAuthenticationResDto login);        
        Task<Return<LoginResDto>> LoginWithCredentialAsync(LoginWithCredentialReqDto req);
    }
}
