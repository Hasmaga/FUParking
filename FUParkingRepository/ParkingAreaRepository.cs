using FUParkingModel.DatabaseContext;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    public class ParkingAreaRepository : IParkingAreaRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public ParkingAreaRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public Task<Return<ParkingArea>> CreateParkingAreaAsync(ParkingArea parkingArea)
        {
            throw new NotImplementedException();
        }
    }
}
