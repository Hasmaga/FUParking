using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IInitializeDataService
    {
        Task<Return<bool>> InitializeDatabase();
    }
}
