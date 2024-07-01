using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Vehicle;
using FUParkingModel.ResponseObject.VehicleType;
using FUParkingModel.ReturnCommon;
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
        private readonly IMinioService _minioService;

        public VehicleService(IVehicleRepository vehicleRepository, IHelpperService helpperService, IUserRepository userRepository, ICustomerRepository customerRepository, IMinioService minioService)
        {
            _vehicleRepository = vehicleRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
            _customerRepository = customerRepository;
            _minioService = minioService;
        }

        public async Task<Return<IEnumerable<VehicleType>>> GetVehicleTypesAsync(GetListObjectWithFiller req)
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
                return await _vehicleRepository.GetAllVehicleTypeAsync(req);
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

        public async Task<Return<IEnumerable<GetVehicleTypeByCustomerResDto>>> GetListVehicleTypeByCustomer()
        {
            try
            {
                var result = await _vehicleRepository.GetAllVehicleTypeAsync();
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetVehicleTypeByCustomerResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR,                        
                    };                    
                }
                return new Return<IEnumerable<GetVehicleTypeByCustomerResDto>>
                {
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    IsSuccess = true,
                    TotalRecord = result.TotalRecord,
                    Data = result.Data?.Select(x => new GetVehicleTypeByCustomerResDto
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
                };
            } catch (Exception ex)
            {
                return new Return<IEnumerable<GetVehicleTypeByCustomerResDto>>
                {
                    InternalErrorMessage = ex,
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

                var vehicleTypes = await _vehicleRepository.GetVehicleTypeByName(reqDto.Name);

                if (vehicleTypes.Data != null && vehicleTypes.IsSuccess)
                {
                    if (vehicleTypes.Data.Name != null)
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
                return new Return<bool>
                {
                    IsSuccess = result.IsSuccess,
                    Data = result.IsSuccess,
                    Message = result.IsSuccess ? SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY : ErrorEnumApplication.SERVER_ERROR
                };
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

        public async Task<Return<bool>> UpdateVehicleTypeAsync(Guid Id, UpdateVehicleTypeReqDto reqDto)
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
                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(Id);
                if (vehicleType.Data == null || vehicleType.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }

                // Check for duplicate vehicle type name with other vehicle types (except the current vehicle type)
                var vehicleTypes = await _vehicleRepository.GetVehicleTypeByName(reqDto.Name ?? "");
                if (vehicleTypes.Data != null)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.OBJECT_EXISTED
                    };
                }

                // Update from here
                if (reqDto.Name != null && reqDto.Name != "")
                {
                    vehicleType.Data.Name = reqDto.Name;
                }
                if (reqDto.Description != null && reqDto.Description != "")
                {
                    vehicleType.Data.Description = reqDto.Description;
                }

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
                if (customerRes.Data == null)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }

                if (customerRes.Data.StatusCustomer.ToLower().Equals(StatusCustomerEnum.INACTIVE.ToLower()))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }
                res.Data = (await _vehicleRepository.GetAllCustomerVehicleByCustomerIdAsync(customerGuid)).Data.ToList();
                return res;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Return<bool>> DeleteVehicleTypeAsync(Guid id)
        {
            try
            {
                // check token validity
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role = Manager
                var userLogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userLogged.Data == null || !userLogged.IsSuccess)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                if (!Auth.AuthManager.Contains(userLogged.Data.Role?.Name ?? ""))
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                // Check if the vehicle type id exists
                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(id);
                if (vehicleType.Data == null || !vehicleType.IsSuccess)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }

                // Check if any vehicle is using the vehicle type
                var vehiclesUsingType = await _vehicleRepository.GetVehiclesByVehicleTypeId(id);
                if (vehiclesUsingType.Data != null && vehiclesUsingType.Data.Any())
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.IN_USE
                    };
                }

                // Update DeletedDate to delete the vehicle type
                vehicleType.Data.DeletedDate = DateTime.Now;
                var result = await _vehicleRepository.UpdateVehicleTypeAsync(vehicleType.Data);
                if (result.IsSuccess)
                {
                    return new Return<bool>
                    {
                        IsSuccess = true,
                        Data = true,
                        Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
                    };
                }
                else
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.DELETE_OBJECT_ERROR
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

        public async Task<Return<dynamic>> CreateCustomerVehicleAsync(CreateCustomerVehicleReqDto reqDto)
        {
            try
            {
                // Check token is valid
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                var userLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userLogged.Data == null || !userLogged.IsSuccess)
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check plate number is existed
                var vehicle = await _vehicleRepository.GetVehicleByPlateNumberAsync(reqDto.PlateNumber);
                if (!vehicle.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    if (vehicle.Message.Equals(ErrorEnumApplication.SERVER_ERROR))
                    {
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = vehicle.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.PLATE_NUMBER_IS_EXIST
                    };
                }
                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(reqDto.VehicleTypeId);
                if (vehicleType.Data == null || !vehicleType.IsSuccess)
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }
                // Upload Image to minio 
                var fileExtensionPlateNumber = Path.GetExtension(reqDto.PlateImage.FileName);
                var objNamePlateNumber = userLogged.Data.Id + "_" + reqDto.PlateNumber + "_" + DateTime.Now.Date.ToString("dd-MM-yyyy") + "_plateNumber" + fileExtensionPlateNumber;
                UploadObjectReqDto imagePlateNumber = new()
                {
                    ObjFile = reqDto.PlateImage,
                    ObjName = objNamePlateNumber,
                    BucketName = BucketMinioEnum.BUCKET_IMAGE_VEHICLE
                };                
                var resultUploadImagePlateNumber = await _minioService.UploadObjectAsync(imagePlateNumber);
                if (!resultUploadImagePlateNumber.Message.Equals(SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {                        
                        InternalErrorMessage = resultUploadImagePlateNumber.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR,
                    };
                }                
                var newVehicle = new Vehicle
                {
                    PlateNumber = reqDto.PlateNumber,
                    VehicleTypeId = reqDto.VehicleTypeId,
                    CustomerId = userLogged.Data.Id,
                    PlateImage = imagePlateNumber.ObjName,                    
                    StatusVehicle = StatusVehicleEnum.PENDING
                };
                var result = await _vehicleRepository.CreateVehicleAsync(newVehicle);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };                
            }
            catch (Exception ex)
            {
                return new Return<dynamic>()
                {                    
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
