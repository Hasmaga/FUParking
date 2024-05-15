﻿using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IUserRepository
    {
        Task<Return<User>> GetUserByEmailAsync(string email);
        Task<Return<User>> GetUserByIdAsync(Guid id);
        Task<Return<User>> CreateUserAsync(User user);
        Task<Return<User>> UpdateUserAsync(User user);
        Task<Return<IEnumerable<User>>> GetAllUsersAsync();
    }
}