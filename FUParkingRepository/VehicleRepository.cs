using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public VehicleRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<IEnumerable<Vehicle>>> GetAllCustomerVehicleByCustomerIdAsync(Guid customerGuid)
        {
            Return<IEnumerable<Vehicle>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                IEnumerable<Vehicle> vehicles = await _db.Vehicles.Where(v => v.CustomerId.Equals(customerGuid)).ToListAsync();
                res.Data = vehicles;
                res.IsSuccess = true;
                res.TotalRecord = vehicles.Count();
                res.Message = vehicles.Any() ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT;
                return res;
            }
            catch (Exception e)
            {
                res.InternalErrorMessage = e;
                return res;
            }
        }

        public async Task<Return<Vehicle>> CreateVehicleAsync(Vehicle vehicle)
        {
            try
            {
                await _db.Vehicles.AddAsync(vehicle);
                await _db.SaveChangesAsync();
                return new Return<Vehicle>
                {
                    Data = vehicle,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Vehicle>
                {                    
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<VehicleType>> CreateVehicleTypeAsync(VehicleType vehicleType)
        {
            try
            {
                await _db.VehicleTypes.AddAsync(vehicleType);
                await _db.SaveChangesAsync();
                return new Return<VehicleType>()
                {
                    Data = vehicleType,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<VehicleType>()
                {                    
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<VehicleType>>> GetAllVehicleTypeAsync(GetListObjectWithFiller? req = null)
        {
            try
            {
                var query = _db.VehicleTypes.Where(e => e.DeletedDate == null).AsQueryable();
                if (req == null)
                {
                    var resul = await query.Where(e => e.DeletedDate == null).ToListAsync();
                    return new Return<IEnumerable<VehicleType>>()
                    {
                        Data = resul,
                        IsSuccess = true,
                        TotalRecord = resul.Count,
                        Message = resul.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                    };
                }
                else if (!string.IsNullOrEmpty(req.Attribute) && !string.IsNullOrEmpty(req.SearchInput))
                {
                    switch (req.Attribute.ToLower())
                    {
                        case "name":
                            query = query.Where(e => e.Name.Contains(req.SearchInput));
                            break;
                        case "description":
                            query = query.Where(e => (e.Description ?? "").Contains(req.SearchInput));
                            break;
                    }
                }
                var result = await query
                                .OrderByDescending(t => t.CreatedDate)
                                .Skip((req.PageIndex - 1) * req.PageSize)
                                .Take(req.PageSize)
                                .ToListAsync();
                return new Return<IEnumerable<VehicleType>>()
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = result.Count,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<VehicleType>>()
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<VehicleType>> GetVehicleTypeByIdAsync(Guid vehicleTypeId)
        {
            try
            {
                var result = await _db.VehicleTypes.Where(e => e.DeletedDate == null).FirstOrDefaultAsync(e => e.Id == vehicleTypeId);
                return new Return<VehicleType>()
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<VehicleType>()
                {                    
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR
                };
            }
        }

        public async Task<Return<VehicleType>> UpdateVehicleTypeAsync(VehicleType vehicleType)
        {
            try
            {
                _db.VehicleTypes.Update(vehicleType);
                await _db.SaveChangesAsync();
                return new Return<VehicleType>()
                {
                    Data = vehicleType,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<VehicleType>()
                {                    
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<Vehicle>>> GetVehiclesAsync()
        {
            try
            {
                var vehicles = await _db.Vehicles.Include(v => v.Customer).ToListAsync();
                return new Return<IEnumerable<Vehicle>>()
                {
                    Data = vehicles,
                    IsSuccess = true,
                    TotalRecord = vehicles.Count,
                    Message = vehicles.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Vehicle>>()
                {                    
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<Vehicle>>> GetVehiclesByVehicleTypeId(Guid id)
        {
            try
            {
                var result = await _db.Vehicles.Where(v => v.VehicleTypeId == id).ToListAsync();
                return new Return<IEnumerable<Vehicle>>()
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = result.Count,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Vehicle>>()
                {                    
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<VehicleType>> GetVehicleTypeByName(string VehicleTypeName)
        {
            try
            {
                var result = await _db.VehicleTypes.Where(a => a.DeletedDate == null).FirstOrDefaultAsync(vt => vt.Name.Equals(VehicleTypeName, StringComparison.OrdinalIgnoreCase));
                return new Return<VehicleType>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<VehicleType>
                {                    
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
