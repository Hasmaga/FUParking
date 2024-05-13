using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IUserService
    {
        Task<Return<bool>> CreateStaffAsync(CreateUserReqDto req);
        Task<Return<bool>> CreateSupervisorAsync(CreateUserReqDto req);
        Task<Return<bool>> CreateManagerAsync(CreateUserReqDto req);
    }
}
