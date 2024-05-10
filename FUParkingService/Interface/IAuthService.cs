using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingModel.ReturnObject;

namespace FUParkingService.Interface
{
    public interface IAuthService
    {
        Task<Return<LoginResDto>> LoginWithGoogleAsync(GoogleReturnAuthenticationResDto login);
        Task<Return<CreateUserReqDto>> CreateUserAsync(CreateUserReqDto user);
    }
}
