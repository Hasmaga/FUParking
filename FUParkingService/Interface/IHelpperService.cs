using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IHelpperService
    {
        bool CheckBearerTokenIsValidAndNotExpired(string token);
        Guid GetAccIdFromLogged();
        bool IsTokenValid();
        Task<string> CreatePassHashAndPassSaltAsync(string pass, out byte[] passwordSalt);
        Task<Return<Customer>> ValidateCustomerAsync();
        Task<Return<User>> ValidateUserAsync(string actor);
    }
}
