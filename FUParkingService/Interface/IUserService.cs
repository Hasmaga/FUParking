﻿using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.User;
using FUParkingModel.ResponseObject.User;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IUserService
    {
        Task<Return<bool>> CreateStaffAsync(FUParkingModel.RequestObject.CreateUserReqDto req);
        Task<Return<bool>> CreateSupervisorAsync(FUParkingModel.RequestObject.CreateUserReqDto req);
        Task<Return<bool>> CreateManagerAsync(FUParkingModel.RequestObject.CreateUserReqDto req);
        Task<Return<IEnumerable<GetUserResDto>>> GetListUserAsync(GetListObjectWithFiller req);
        Task<Return<bool>> ChangeUserStatusAsync(Guid userId);
        Task<Return<bool>> ResetWrongPasswordCountAsync(Guid userId);
        Task<Return<bool>> UpdateUserAsync(UpdateUserReqDto req);
        Task<Return<bool>> UpdatePasswordAsync(UpdateUserPasswordReqDto req);
        Task<Return<bool>> DeleteUserByIdAsync(Guid id);
        Task<Return<dynamic>> CreateListUserAsync(CreateListUserReqDto req);
    }
}
