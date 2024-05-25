using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IHelpperService _helpperService;
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;

        public VehicleService(IVehicleRepository vehicleRepository, IHelpperService helpperService, IUserRepository userRepository,ICustomerRepository customerRepository)
        {
            _vehicleRepository = vehicleRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
            _customerRepository = customerRepository;
        }

        public async Task<Return<IEnumerable<VehicleType>>> GetVehicleTypesAsync()
        {
            try
            {
                // Validate token
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<IEnumerable<VehicleType>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<IEnumerable<VehicleType>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthSupervisor.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<VehicleType>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                return await _vehicleRepository.GetAllVehicleTypeAsync();
            }
            catch (Exception)
            {
                return new Return<IEnumerable<VehicleType>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<bool>> CreateVehicleTypeAsync(CreateVehicleTypeReqDto reqDto)
        {
            try
            {
                // check token valid
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                // Check role = Manager, Supervisor
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                if (!Auth.AuthSupervisor.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check if the vehicle type name already exists
                var vehicleTypes = await _vehicleRepository.GetAllVehicleTypeAsync();
                if (vehicleTypes.Data != null && vehicleTypes.IsSuccess)
                {
                    bool isVehicleTypeExist = vehicleTypes.Data.Any(v => v.Name.Equals(reqDto.Name, StringComparison.OrdinalIgnoreCase));

                    if (isVehicleTypeExist)
                    {
                        return new Return<bool>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.OBJECT_EXISTED
                        };
                    }
                }

                var vehicleType = new VehicleType
                {
                    Name = reqDto.Name,
                    Description = reqDto.Description
                };

                var result = await _vehicleRepository.CreateVehicleTypeAsync(vehicleType);
                if (result.IsSuccess)
                {
                    return new Return<bool>
                    {
                        IsSuccess = true,
                        Data = true,
                        Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                    };
                }
                else
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
            } catch (Exception)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<bool>> UpdateVehicleTypeAsync(UpdateVehicleTypeReqDto reqDto)
        {
            try
            {
                // check token valid
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                // Check role = Manager, Supervisor
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                if (!Auth.AuthSupervisor.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check the vehicle type id exists
                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(reqDto.Id);
                if (vehicleType.Data == null || vehicleType.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }   

                // Check for duplicate vehicle type name with other vehicle types (except the current vehicle type)
                var vehicleTypes = await _vehicleRepository.GetAllVehicleTypeAsync();

                if (vehicleTypes.Data == null || vehicleTypes.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }

                bool isVehicleTypeExist = vehicleTypes.Data.Any(v => v.Name.Equals(reqDto.Name, StringComparison.OrdinalIgnoreCase) && !v.Id.Equals(reqDto.Id));

                if (isVehicleTypeExist)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.OBJECT_EXISTED
                    };
                }

                // Update from here
                vehicleType.Data.Name = reqDto.Name;
                vehicleType.Data.Description = reqDto.Description;

                var result = await _vehicleRepository.UpdateVehicleTypeAsync(vehicleType.Data);
                if (result.IsSuccess)
                {
                    return new Return<bool>
                    {
                        IsSuccess = true,
                        Data = true,
                        Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                    };
                }
                else
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR
                    };
                }
            }
            catch (Exception)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
        public async Task<Return<IEnumerable<Vehicle>>> GetVehiclesAsync()
        {
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<IEnumerable<Vehicle>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<IEnumerable<Vehicle>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthSupervisor.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<Vehicle>> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                return await _vehicleRepository.GetVehiclesAsync();
            }
            catch
            {
                return new Return<IEnumerable<Vehicle>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
             }
        }

        public async Task<Return<List<Vehicle>>> GetCustomerVehicleByCustomerIdAsync(Guid customerGuid)
        {
            Return<List<Vehicle>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                Return<Customer> customerRes = await _customerRepository.GetCustomerByIdAsync(customerGuid);
                if(customerRes.Data == null)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }

                if (customerRes.Data.StatusCustomer.ToLower().Equals(StatusCustomerEnum.INACTIVE.ToLower()))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }

                res = await _vehicleRepository.GetAllCustomerVehicleByCustomerIdAsync(customerGuid);
                return res;
            }
            catch
            {
                throw;
            }
        }
    }
}
