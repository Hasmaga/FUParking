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

        public async Task<Return<List<Vehicle>>> GetAllCustomerVehicleByCustomerIdAsync(Guid customerGuid)
        {
            Return<List<Vehicle>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                List<Vehicle> vehicles = await _db.Vehicles.Where(v => v.CustomerId.Equals(customerGuid)).ToListAsync();
                res.Data = vehicles;
                res.IsSuccess = true;
                res.Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY;
                return res;
            }
            catch
            {
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
                    IsSuccess = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
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
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<VehicleType>>> GetAllVehicleTypeAsync(GetListObjectWithFiller req)
        {
            try
            {
                var query = _db.VehicleTypes.Where(e => e.DeletedDate == null).AsQueryable();
                if (!string.IsNullOrEmpty(req.Attribute) && !string.IsNullOrEmpty(req.SearchInput))
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
                return new Return<IEnumerable<VehicleType>>()
                {
                    Data = await query
                                .OrderByDescending(t => t.CreatedDate)
                                .Skip((req.PageIndex - 1) * req.PageSize)
                                .Take(req.PageSize)
                                .ToListAsync(),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<VehicleType>>()
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR
                };
            }
        }

        public async Task<Return<VehicleType>> GetVehicleTypeByIdAsync(Guid vehicleTypeId)
        {
            try
            {
                return new Return<VehicleType>()
                {
                    Data = await _db.VehicleTypes.Where(e => e.DeletedDate == null).FirstOrDefaultAsync(e => e.Id == vehicleTypeId),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<VehicleType>()
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
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
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR
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
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Vehicle>>()
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<Vehicle>>> GetVehiclesByVehicleTypeId(Guid id)
        {
            try
            {
                return new Return<IEnumerable<Vehicle>>()
                {
                    Data = await _db.Vehicles.Where(v => v.VehicleTypeId == id).ToListAsync(),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Vehicle>>()
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR
                };
            }
        }

        public async Task<Return<VehicleType>> GetVehicleTypeByName(string VehicleTypeName)
        {
            try
            {
                return new Return<VehicleType>
                {
                    Data = await _db.VehicleTypes.Where(a => a.DeletedDate == null).FirstOrDefaultAsync(vt => vt.Name.Equals(VehicleTypeName, StringComparison.OrdinalIgnoreCase)),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<VehicleType>
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR
                };
            }
        }
    }
}
