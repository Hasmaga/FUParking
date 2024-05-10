using FUParkingModel.ReturnCommon;
using FUParkingModel.ReturnObject;

namespace FUParkingService.Interface
{
    public interface IAuthService
    {
        Task<Return<string>> LoginWithGoogleAsync(GoogleReturnAuthenticationResDto login);
    }
}
