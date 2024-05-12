using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingModel.ReturnObject;

namespace FUParkingService.Interface
{
    public interface IAuthService
    {
        Task<Return<LoginResDto>> LoginWithGoogleAsync(GoogleReturnAuthenticationResDto login);
        Task<Return<bool>> CreateStaffAsync(CreateUserReqDto req);
        Task<Return<bool>> CreateSupervisorAsync(CreateUserReqDto req);
        Task<Return<bool>> CreateManagerAsync(CreateUserReqDto req);
        Task<Return<LoginResDto>> LoginWithCredentialAsync(LoginWithCredentialReqDto req);
    }
}
