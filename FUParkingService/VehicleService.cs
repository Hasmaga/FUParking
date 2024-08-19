﻿using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Vehicle;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ResponseObject.Vehicle;
using FUParkingModel.ResponseObject.VehicleType;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.Transactions;

namespace FUParkingService
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IHelpperService _helpperService;
        private readonly ISessionRepository _sessionRepository;
        private readonly IMinioService _minioService;

        public VehicleService(IVehicleRepository vehicleRepository, IHelpperService helpperService, ISessionRepository sessionRepository, IMinioService minioService)
        {
            _vehicleRepository = vehicleRepository;
            _helpperService = helpperService;
            _sessionRepository = sessionRepository;
            _minioService = minioService;
        }

        public async Task<Return<IEnumerable<GetVehicleTypeByUserResDto>>> GetVehicleTypesAsync(GetListObjectWithFiller req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetVehicleTypeByUserResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _vehicleRepository.GetAllVehicleTypeAsync(req);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetVehicleTypeByUserResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<IEnumerable<GetVehicleTypeByUserResDto>>
                {
                    Data = result.Data?.Select(x => new GetVehicleTypeByUserResDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        CreateByEmail = x.CreateBy?.Email ?? "",
                        CreateDatetime = x.CreatedDate,
                        LastModifyByEmail = x.LastModifyBy?.Email ?? "",
                        LastModifyDatetime = x.LastModifyDate
                    }),
                    TotalRecord = result.TotalRecord,
                    Message = result.TotalRecord > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = true
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<GetVehicleTypeByUserResDto>>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<GetVehicleTypeByCustomerResDto>>> GetListVehicleTypeByCustomer()
        {
            try
            {
                var result = await _vehicleRepository.GetAllVehicleTypeByCustomer();
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
                    Message = result.Message,
                    IsSuccess = true,
                    TotalRecord = result.TotalRecord,
                    Data = result.Data?.Select(x => new GetVehicleTypeByCustomerResDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description ?? ""
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
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var vehicleTypes = await _vehicleRepository.GetVehicleTypeByName(reqDto.Name);
                if (!vehicleTypes.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = vehicleTypes.InternalErrorMessage,
                        Message = ErrorEnumApplication.OBJECT_EXISTED
                    };
                }

                var vehicleType = new VehicleType
                {
                    Name = reqDto.Name,
                    Description = reqDto.Description,
                    CreatedById = checkAuth.Data.Id,
                    StatusVehicleType = StatusVehicleType.ACTIVE
                };

                var result = await _vehicleRepository.CreateVehicleTypeAsync(vehicleType);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<bool>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<bool>> UpdateVehicleTypeAsync(UpdateVehicleTypeReqDto reqDto)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                // Check the vehicle type id exists
                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(reqDto.Id);
                if (!vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || vehicleType.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = vehicleType.InternalErrorMessage,
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }

                if (reqDto.Name is not null && !reqDto.Name.Trim().Equals(vehicleType.Data.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var isExistVehicleType = await _vehicleRepository.GetVehicleTypeByName(reqDto.Name);
                    if (!isExistVehicleType.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                    {
                        return new Return<bool>
                        {
                            Message = ErrorEnumApplication.OBJECT_EXISTED
                        };
                    }
                    vehicleType.Data.Name = reqDto.Name;
                }                
                vehicleType.Data.Description = reqDto.Description ?? vehicleType.Data.Description;
                vehicleType.Data.LastModifyById = checkAuth.Data.Id;
                vehicleType.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                var result = await _vehicleRepository.UpdateVehicleTypeAsync(vehicleType.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
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

        public async Task<Return<IEnumerable<GetVehicleForUserResDto>>> GetVehiclesAsync(GetListObjectWithFillerAttributeAndDateReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetVehicleForUserResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _vehicleRepository.GetVehiclesAsync(req);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetVehicleForUserResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<IEnumerable<GetVehicleForUserResDto>>
                {
                    Data = result.Data?.Select(x => new GetVehicleForUserResDto
                    {
                        Id = x.Id,
                        PlateNumber = x.PlateNumber,
                        VehicleType = x.VehicleType?.Name ?? "",
                        PlateImage = x.PlateImage,
                        StatusVehicle = x.StatusVehicle,
                        CreatedDate = x.CreatedDate,
                        Email = x.Customer?.Email ?? "",
                        LastModifyBy = x.LastModifyBy?.Email ?? "",
                        LastModifyDate = x.LastModifyDate,
                        StaffApproval = x.Staff?.Email ?? ""
                    }),
                    TotalRecord = result.TotalRecord,
                    Message = result.TotalRecord > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = true
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<GetVehicleForUserResDto>>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<GetCustomerVehicleByCustomerResDto>>> GetListCustomerVehicleByCustomerIdAsync()
        {
            Return<IEnumerable<GetCustomerVehicleByCustomerResDto>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    res.InternalErrorMessage = checkAuth.InternalErrorMessage;
                    res.Message = checkAuth.Message;
                    return res;
                }

                var result = await _vehicleRepository.GetAllCustomerVehicleByCustomerIdAsync(checkAuth.Data.Id);
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
                res.Message = result.TotalRecord > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
                return res;
            }
        }

        public async Task<Return<dynamic>> ChangeStatusVehicleTypeAsync(Guid id, bool isActive)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(id);
                if (!vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || vehicleType.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = vehicleType.InternalErrorMessage,
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }
                if (isActive)
                {
                    // Check vehicle type status is already active
                    if (vehicleType.Data.StatusVehicleType.Equals(StatusVehicleType.ACTIVE))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    vehicleType.Data.StatusVehicleType = StatusVehicleType.ACTIVE;
                }
                else if (!isActive)
                {
                    if (vehicleType.Data.StatusVehicleType.Equals(StatusVehicleType.INACTIVE))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    vehicleType.Data.StatusVehicleType = StatusVehicleType.INACTIVE;
                }
                vehicleType.Data.LastModifyById = checkAuth.Data.Id;
                vehicleType.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var result = await _vehicleRepository.UpdateVehicleTypeAsync(vehicleType.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
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

        public async Task<Return<dynamic>> DeleteVehicleTypeAsync(Guid id)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(id);

                if (!vehicleType.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };

                if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST };

                // Check vehicle type is in any vehicle
                var vehicles = await _vehicleRepository.GetNewestVehicleByVehicleTypeId(id);
                if (!vehicles.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.VEHICLE_TYPE_IS_IN_USE,
                        InternalErrorMessage = vehicles.InternalErrorMessage
                    };
                }

                vehicleType.Data.StatusVehicleType = StatusVehicleType.INACTIVE;
                vehicleType.Data.LastModifyById = checkAuth.Data.Id;
                vehicleType.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
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

        public async Task<Return<GetInformationVehicleCreateResDto>> CreateCustomerVehicleAsync(CreateCustomerVehicleReqDto reqDto)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    scope.Dispose();
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(reqDto.VehicleTypeId);
                if (vehicleType.Data == null || !vehicleType.IsSuccess)
                {
                    scope.Dispose();
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }
                // Check plate number exists
                var vehicle = await _vehicleRepository.GetVehicleByPlateNumberAsync(reqDto.PlateNumber);
                if (!vehicle.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    scope.Dispose();
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        InternalErrorMessage = vehicle.InternalErrorMessage,
                        Message = ErrorEnumApplication.PLATE_NUMBER_IS_EXIST
                    };
                }
                var fileExtensionPlateNumber = Path.GetExtension(reqDto.PlateImage.FileName);
                var objNamePlateNumber = checkAuth.Data.Id + "_" + reqDto.PlateNumber + "_" + TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).Date.ToString("dd-MM-yyyy") + "_plateNumber" + fileExtensionPlateNumber;
                UploadObjectReqDto imageUpload = new()
                {
                    ObjFile = reqDto.PlateImage,
                    ObjName = objNamePlateNumber,
                    BucketName = BucketMinioEnum.BUCKET_IMAGE_VEHICLE
                };
                var resultUploadImagePlateNumber = await _minioService.UploadObjectAsync(imageUpload);
                if (!resultUploadImagePlateNumber.Message.Equals(SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY))
                {
                    scope.Dispose();
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        InternalErrorMessage = resultUploadImagePlateNumber.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR,
                    };
                }
                var newVehicle = new Vehicle
                {
                    PlateNumber = reqDto.PlateNumber,
                    VehicleTypeId = reqDto.VehicleTypeId,
                    CustomerId = checkAuth.Data.Id,
                    PlateImage = "https://miniofile.khangbpa.com/" + BucketMinioEnum.BUCKET_IMAGE_VEHICLE + "/" + objNamePlateNumber,
                    StatusVehicle = StatusVehicleEnum.PENDING
                };
                var result = await _vehicleRepository.CreateVehicleAsync(newVehicle);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || result.Data == null)
                {
                    scope.Dispose();
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                scope.Complete();
                return new Return<GetInformationVehicleCreateResDto>
                {
                    IsSuccess = true,
                    Data = new GetInformationVehicleCreateResDto
                    {
                        VehicleId = result.Data.Id,
                        PlateNumber = result.Data.PlateNumber,
                        VehicleTypeName = result.Data.VehicleType?.Name ?? "",
                        ImagePlateNumber = result.Data.PlateImage
                    },
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<GetInformationVehicleCreateResDto>()
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> UpdateVehicleInformationAsync(UpdateCustomerVehicleReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var vehicle = await _vehicleRepository.GetVehicleByIdAsync(req.VehicleId);
                if (!vehicle.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicle.InternalErrorMessage };

                if (vehicle.Data == null || !vehicle.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_NOT_EXIST };

                if (vehicle.Data.CustomerId != checkAuth.Data.Id)
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                if (!vehicle.Data.StatusVehicle.Equals(StatusVehicleEnum.PENDING) && !vehicle.Data.StatusVehicle.Equals(StatusVehicleEnum.REJECTED))
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                // Check vehicle type id exists
                if (req.VehicleTypeId != null)
                {
                    var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(req.VehicleTypeId.Value);
                    if (!vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || vehicleType.Data is null)
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                        };
                    }
                    vehicle.Data.VehicleTypeId = vehicleType.Data.Id;
                }
                if (req.PlateNumber != null)
                    vehicle.Data.PlateNumber = req.PlateNumber;
                vehicle.Data.StatusVehicle = StatusVehicleEnum.PENDING;
                var result = await _vehicleRepository.UpdateVehicleAsync(vehicle.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
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

        public async Task<Return<dynamic>> DeleteVehicleByCustomerAsync(Guid vehicleId)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var vehicle = await _vehicleRepository.GetVehicleByIdAsync(vehicleId);
                if (!vehicle.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicle.InternalErrorMessage };
                if (vehicle.Data == null || !vehicle.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_NOT_EXIST };
                if (vehicle.Data.CustomerId != checkAuth.Data.Id)
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                // Check vehicle is in any session 
                var newestSession = await _sessionRepository.GetNewestSessionByPlateNumberAsync(vehicle.Data.PlateNumber);
                if (!newestSession.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = newestSession.InternalErrorMessage
                    };
                }
                if (newestSession.Data?.GateOut is not null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.VEHICLE_IS_IN_SESSION
                    };
                }
                vehicle.Data.DeletedDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

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

        public async Task<Return<dynamic>> ChangeStatusVehicleByUserAsync(UpdateNewCustomerVehicleByUseReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var vehicle = await _vehicleRepository.GetVehicleByPlateNumberAsync(req.PlateNumber);
                if (!vehicle.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || vehicle.Data is null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.VEHICLE_NOT_EXIST
                    };
                }
                // Check vehicle is active
                if (vehicle.Data.StatusVehicle.Equals(StatusVehicleEnum.ACTIVE))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.VEHICLE_IS_ACTIVE
                    };
                }
                if (req.VehicleType is not null && req.IsAccept)
                {
                    // Check vehicle type id exists
                    var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(req.VehicleType.Value);
                    if (!vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || vehicleType.Data is null)
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                        };
                    }
                    vehicle.Data.VehicleTypeId = req.VehicleType.Value;
                }
                vehicle.Data.StatusVehicle = req.IsAccept ? StatusVehicleEnum.ACTIVE : StatusVehicleEnum.REJECTED;
                vehicle.Data.LastModifyById = checkAuth.Data.Id;
                vehicle.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var result = await _vehicleRepository.UpdateVehicleAsync(vehicle.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
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

        public async Task<Return<dynamic>> UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync(UpdateStatusInactiveAndActiveCustomerVehicleByUserReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var vehicle = await _vehicleRepository.GetVehicleByIdAsync(req.VehicleId);
                if (!vehicle.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || vehicle.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = vehicle.InternalErrorMessage,
                        Message = ErrorEnumApplication.VEHICLE_NOT_EXIST
                    };
                }

                // Check vehicle is any session
                var newestSession = await _sessionRepository.GetNewestSessionByPlateNumberAsync(vehicle.Data.PlateNumber);
                if (!newestSession.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = newestSession.InternalErrorMessage
                    };
                }
                if (newestSession.Data?.GateOut is not null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.VEHICLE_IS_IN_SESSION
                    };
                }
                if (req.IsActive)
                {
                    // Check vehicle is active
                    if (vehicle.Data.StatusVehicle.Equals(StatusVehicleEnum.ACTIVE))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    vehicle.Data.StatusVehicle = StatusVehicleEnum.ACTIVE;
                }
                else
                {
                    // Check vehicle is inactive
                    if (vehicle.Data.StatusVehicle.Equals(StatusVehicleEnum.INACTIVE))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    vehicle.Data.StatusVehicle = StatusVehicleEnum.INACTIVE;
                }
                vehicle.Data.LastModifyById = checkAuth.Data.Id;
                vehicle.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var result = await _vehicleRepository.UpdateVehicleAsync(vehicle.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
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

        public async Task<Return<GetCustomerVehicleByCustomerResDto>> GetCustomerVehicleByVehicleIdAsync(Guid VehicleId)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<GetCustomerVehicleByCustomerResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var vehicle = await _vehicleRepository.GetVehicleByIdAsync(VehicleId);
                if (!vehicle.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || vehicle.Data is null)
                {
                    return new Return<GetCustomerVehicleByCustomerResDto>
                    {
                        InternalErrorMessage = vehicle.InternalErrorMessage,
                        Message = ErrorEnumApplication.VEHICLE_NOT_EXIST
                    };
                }
                if (vehicle.Data.CustomerId != checkAuth.Data.Id)
                {
                    return new Return<GetCustomerVehicleByCustomerResDto>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                return new Return<GetCustomerVehicleByCustomerResDto>
                {
                    Data = new GetCustomerVehicleByCustomerResDto
                    {
                        Id = vehicle.Data.Id,
                        PlateNumber = vehicle.Data.PlateNumber,
                        VehicleTypeName = vehicle.Data.VehicleType?.Name ?? "",
                        PlateImage = vehicle.Data.PlateImage,
                        StatusVehicle = vehicle.Data.StatusVehicle,
                        CreateDate = vehicle.Data.CreatedDate
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new Return<GetCustomerVehicleByCustomerResDto>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> UpdateVehicleInformationByUserAsync(Guid vehicleId, Guid vehicleTypeId)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var vehicle = await _vehicleRepository.GetVehicleByIdAsync(vehicleId);
                if (!vehicle.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicle.InternalErrorMessage };

                if (vehicle.Data == null || !vehicle.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_NOT_EXIST };

                // Check vehicle is in any session
                var newestSession = await _sessionRepository.GetNewestSessionByPlateNumberAsync(vehicle.Data.PlateNumber);
                if (!newestSession.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = newestSession.InternalErrorMessage
                    };
                }
                if (newestSession.Data?.GateOut is not null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.VEHICLE_IS_IN_SESSION
                    };
                }

                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(vehicleTypeId);
                if (!vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || vehicleType.Data is null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }
                vehicle.Data.VehicleTypeId = vehicleType.Data.Id;
                vehicle.Data.StatusVehicle = StatusVehicleEnum.ACTIVE;
                var result = await _vehicleRepository.UpdateVehicleAsync(vehicle.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
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

        public async Task<Return<StatisticVehicleResDto>> GetStatisticVehicleAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<StatisticVehicleResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _vehicleRepository.GetStatisticVehicleAsync();
                if (!result.IsSuccess)
                {
                    return new Return<StatisticVehicleResDto>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<StatisticVehicleResDto>
                {
                    Data = result.Data,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new Return<StatisticVehicleResDto>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
