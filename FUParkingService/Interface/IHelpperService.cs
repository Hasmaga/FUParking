namespace FUParkingService.Interface
{
    public interface IHelpperService
    {
        bool CheckBearerTokenIsValidAndNotExpired(string token);
        Guid GetAccIdFromLogged();
        bool IsTokenValid();
        Guid GetAccIdFromLoogedNotThrow();
    }
}
