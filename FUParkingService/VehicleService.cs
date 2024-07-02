using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Vehicle;
using FUParkingModel.ResponseObject.Vehicle;
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
            }
            catch (Exception ex)
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
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<IEnumerable<Vehicle>>
                    {
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
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<GetCustomerVehicleByCustomerResDto>>> GetCustomerVehicleByCustomerIdAsync()
        {
            Return<IEnumerable<GetCustomerVehicleByCustomerResDto>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                Return<Customer> customerLogin = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!customerLogin.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customerLogin.Data == null)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }

                if (customerLogin.Data.StatusCustomer.ToLower().Equals(StatusCustomerEnum.INACTIVE.ToLower()))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }
                var result = await _vehicleRepository.GetAllCustomerVehicleByCustomerIdAsync(customerLogin.Data.Id);
                if (!result.IsSuccess)
                {
                    res.InternalErrorMessage = result.InternalErrorMessage;
                    return res;
                }
                res.Data = result.Data?.Select(x => new GetCustomerVehicleByCustomerResDto
                {
                    Id = x.Id,
                    PlateNumber = x.PlateNumber,
                    VehicleTypeName = x.VehicleType?.Name ?? "",
                    PlateImage = x.PlateImage,
                    StatusVehicle = x.StatusVehicle,
                    CreateDate = x.CreatedDate
                });
                res.TotalRecord = result.TotalRecord;
                res.IsSuccess = true;
                res.Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
                return res;
            }
        }

        public async Task<Return<dynamic>> DeleteVehicleTypeAsync(Guid id)
        {
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                var userLogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());

                if (!userLogged.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = userLogged.InternalErrorMessage };

                if (userLogged.Data == null || !userLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                if (!Auth.AuthSupervisor.Contains(userLogged.Data.Role?.Name ?? ""))
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(id);

                if (!vehicleType.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };

                if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST };

                vehicleType.Data.DeletedDate = DateTime.Now;

                var result = await _vehicleRepository.UpdateVehicleTypeAsync(vehicleType.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY };
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    InternalErrorMessage = ex,
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
                    PlateImage = "https://miniofile.khangbpa.com/" + BucketMinioEnum.BUCKET_IMAGE_VEHICLE + "/" + imagePlateNumber.ObjName,
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

        public async Task<Return<dynamic>> DeleteVehicleByCustomerAsync(Guid vehicleId)
        {
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                var userLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!userLogged.IsSuccess) 
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = userLogged.InternalErrorMessage };

                if (userLogged.Data == null || !userLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                var vehicle = await _vehicleRepository.GetVehicleByIdAsync(vehicleId);
                if (!vehicle.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicle.InternalErrorMessage };

                if (vehicle.Data == null || !vehicle.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_NOT_EXIST };

                if (vehicle.Data.CustomerId != userLogged.Data.Id)
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                if (!vehicle.Data.StatusVehicle.Equals(StatusVehicleEnum.PENDING))
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                vehicle.Data.DeletedDate = DateTime.Now;

                var result = await _vehicleRepository.UpdateVehicleAsync(vehicle.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY };
            } 
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
