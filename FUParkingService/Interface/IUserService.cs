using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.User;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IUserService
    {
        Task<Return<bool>> CreateStaffAsync(CreateUserReqDto req);
        Task<Return<bool>> CreateSupervisorAsync(CreateUserReqDto req);
        Task<Return<bool>> CreateManagerAsync(CreateUserReqDto req);
        Task<Return<IEnumerable<GetUserResDto>>> GetListUserAsync(GetListObjectWithFiller req);
        Task<Return<bool>> DeleteUserByIdAsync(Guid id);
    }
}
