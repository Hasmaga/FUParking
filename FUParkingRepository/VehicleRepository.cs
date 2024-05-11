using FUParkingModel.DatabaseContext;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public VehicleRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public Task<Return<Vehicle>> CreateVehicleAsync(Vehicle vehicle)
        {
            throw new NotImplementedException();
        }
    }
}
