using FUParkingModel.ReturnCommon;
using Microsoft.AspNetCore.Http;

namespace FUParkingService.Interface
{
    public interface IHelpperService
    {
        bool CheckBearerTokenIsValidAndNotExpired(string token);
        Guid GetAccIdFromLogged();
        bool IsTokenValid();
        Task<string> CreatePassHashAndPassSaltAsync(string pass, out byte[] passwordSalt);        
    }
}
