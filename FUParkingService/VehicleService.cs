using FirebaseService;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Firebase;
using FUParkingModel.RequestObject.Vehicle;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ResponseObject.Vehicle;
using FUParkingModel.ResponseObject.VehicleType;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Helper;
using FUParkingService.Interface;
using FUParkingService.MailObject;
using FUParkingService.MailService;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Transactions;

namespace FUParkingService
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IHelpperService _helpperService;
        private readonly ISessionRepository _sessionRepository;
        private readonly IMinioService _minioService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IFirebaseService _firebaseService;
        private readonly IMailService _mailService;
        private readonly ILogger<VehicleService> _logger;
        private readonly IPriceRepository _priceRepository;

        public VehicleService(IVehicleRepository vehicleRepository, IHelpperService helpperService, ISessionRepository sessionRepository, IMinioService minioService, ICustomerRepository customerRepository, IFirebaseService firebaseService, ILogger<VehicleService> logger, IPriceRepository priceRepository, IMailService mailService)
        {
            _vehicleRepository = vehicleRepository;
            _helpperService = helpperService;
            _sessionRepository = sessionRepository;
            _minioService = minioService;
            _customerRepository = customerRepository;
            _firebaseService = firebaseService;
            _mailService = mailService;
            _logger = logger;
            _priceRepository = priceRepository;
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
                        Description = x.Description ?? "",
                        Status = x.StatusVehicleType,
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
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
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
                if (!result.IsSuccess || result.Data is null)
                {
                    scope.Dispose();
                    return new Return<bool>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }

                // Create default price table
                var priceTable = new PriceTable
                {
                    Name = "Default",
                    Priority = 1,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    VehicleTypeId = result.Data.Id
                };

                var resultPriceTable = await _priceRepository.CreatePriceTableAsync(priceTable);
                if (!resultPriceTable.IsSuccess || resultPriceTable.Data is null)
                {
                    scope.Dispose();
                    return new Return<bool>
                    {
                        InternalErrorMessage = resultPriceTable.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }

                // Create price item for default price table
                var priceItem = new PriceItem
                {
                    BlockPricing = reqDto.BlockPricing,
                    MaxPrice = reqDto.MaxPrice,
                    MinPrice = reqDto.MinPrice,
                    CreatedById = checkAuth.Data.Id,
                    PriceTableId = resultPriceTable.Data.Id,
                };

                var resultPriceItem = await _priceRepository.CreatePriceItemAsync(priceItem);
                if (!resultPriceItem.IsSuccess || resultPriceItem.Data is null)
                {
                    scope.Dispose();
                    return new Return<bool>
                    {
                        InternalErrorMessage = resultPriceItem.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                scope.Complete();
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
                        PlateImage = x.PlateImage ?? "",
                        StatusVehicle = x.StatusVehicle,
                        CreatedDate = x.CreatedDate,
                        Email = x.Customer?.Email ?? "",
                        LastModifyBy = x.LastModifyBy?.Email ?? "",
                        LastModifyDate = x.LastModifyDate,
                        StaffApproval = x.Staff?.Email ?? "",
                        VehicleTypeId = x.VehicleTypeId
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
                    PlateImage = x.PlateImage ?? "",
                    StatusVehicle = x.StatusVehicle,
                    CreateDate = x.CreatedDate,
                    VehicleTypeId = x.VehicleTypeId
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

                vehicleType.Data.DeletedDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
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
                if (string.IsNullOrEmpty(reqDto.PlateNumber) && reqDto.PlateNumber == null)
                {
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        Message = ErrorEnumApplication.NOT_A_PLATE_NUMBER,
                    };
                }
                // Check Plate Number is valid
                reqDto.PlateNumber = reqDto.PlateNumber.Trim().Replace("-", "").Replace(".", "").Replace(" ", "");
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                Regex regex = new(@"^[0-9]{2}[A-ZĐ]{1,2}[0-9]{4,6}$");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                if (!regex.IsMatch(reqDto.PlateNumber))
                {
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        Message = ErrorEnumApplication.NOT_A_PLATE_NUMBER
                    };
                }
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

                // Firebase send notification
                if (!string.IsNullOrEmpty(checkAuth.Data.FCMToken))
                {
                    var plateNumber = Utilities.FormatPlateNumber(reqDto.PlateNumber);

                    var firebaseReq = new FirebaseReqDto
                    {
                        ClientTokens = new List<string> { checkAuth.Data.FCMToken },
                        Title = "Vehicle Registration Complete!",
                        Body = $"Your vehicle with the plate number {plateNumber} has been successfully registered. To activate your registration, please park your vehicle at our facility for the first time."
                    };
                    var notificationResult = await _firebaseService.SendNotificationAsync(firebaseReq);

                    if (notificationResult.IsSuccess == false)
                    {
                        _logger.LogError("Failed to send notification to customer Id {CustomerId}. Error: {Error}", checkAuth.Data.Id, notificationResult.InternalErrorMessage);
                    }
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
                        ImagePlateNumber = result.Data.PlateImage ?? ""
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
                {
                    if (string.IsNullOrEmpty(req.PlateNumber) && req.PlateNumber == null)
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.NOT_A_PLATE_NUMBER,
                        };
                    }
                    // Check Plate Number is valid
                    req.PlateNumber = req.PlateNumber.Trim().Replace("-", "").Replace(".", "").Replace(" ", "");
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                    Regex regex = new(@"^[0-9]{2}[A-ZĐ]{1,2}[0-9]{4,6}$");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                    if (!regex.IsMatch(req.PlateNumber))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.NOT_A_PLATE_NUMBER
                        };
                    }
                    // Check PlateNumber is exist
                    if (!req.PlateNumber.Equals(vehicle.Data.PlateNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        var vehicleByPlateNumber = await _vehicleRepository.GetVehicleByPlateNumberAsync(req.PlateNumber);
                        if (!vehicleByPlateNumber.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                        {
                            return new Return<dynamic>
                            {
                                InternalErrorMessage = vehicleByPlateNumber.InternalErrorMessage,
                                Message = ErrorEnumApplication.PLATE_NUMBER_IS_EXIST
                            };
                        }
                    }
                    vehicle.Data.PlateNumber = req.PlateNumber;
                }
                vehicle.Data.StatusVehicle = StatusVehicleEnum.PENDING;
                var result = await _vehicleRepository.UpdateVehicleAsync(vehicle.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };

                // Firebase send notification
                if (!string.IsNullOrEmpty(checkAuth.Data.FCMToken))
                {
                    var plateNumber = Utilities.FormatPlateNumber(vehicle.Data.PlateNumber);

                    var firebaseReq = new FirebaseReqDto
                    {
                        ClientTokens = new List<string> { checkAuth.Data.FCMToken },
                        Title = "Vehicle Information Updated!",
                        Body = $"The information for your vehicle with the plate number {plateNumber} has been successfully updated."
                    };
                    var notificationResult = await _firebaseService.SendNotificationAsync(firebaseReq);

                    if (notificationResult.IsSuccess == false)
                    {
                        _logger.LogError("Failed to send notification to customer Id {CustomerId}. Error: {Error}", checkAuth.Data.Id, notificationResult.InternalErrorMessage);
                    }
                }

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
                if (newestSession.Data?.Status == (SessionEnum.PARKED))
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

                // Firebase
                if (!string.IsNullOrEmpty(checkAuth.Data.FCMToken))
                {
                    var firebaseReq = new FirebaseReqDto
                    {
                        ClientTokens = [checkAuth.Data.FCMToken],
                        Title = "Vehicle Information Deleteted",
                        Body = $"Your vehicle information with plate number {vehicle.Data.PlateNumber} has been deleteted successfully."
                    };
                    var notificationResult = await _firebaseService.SendNotificationAsync(firebaseReq);
                }

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

                // send notification to customer
                var customer = await _customerRepository.GetCustomerByIdAsync(vehicle.Data.CustomerId ?? Guid.NewGuid());
                if (customer.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    if (!string.IsNullOrEmpty(customer.Data?.FCMToken))
                    {
                        var firebaseReq = new FirebaseReqDto
                        {
                            ClientTokens = new List<string> { customer.Data.FCMToken },
                            Title = "Vehicle Registration",
                            Body = req.IsAccept ? "Your vehicle registration has been approved by Bai's Staff." : "Your vehicle registration has been rejected by Bai's Staff."
                        };
                        var notificationResult = await _firebaseService.SendNotificationAsync(firebaseReq);

                        if (notificationResult.IsSuccess == false)
                        {
                            _logger.LogError("Failed to send notification to customer Id {CustomerId}. Error: {Error}", customer.Data.Id, notificationResult.InternalErrorMessage);
                        }
                    }
                }

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
                if (!newestSession.IsSuccess || newestSession.Data is null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = newestSession.InternalErrorMessage
                    };
                }
                if (newestSession.Data.Status.Equals(SessionEnum.PARKED))
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

                // Firebase send notification
                var customer = await _customerRepository.GetCustomerByIdAsync(vehicle.Data.CustomerId ?? Guid.NewGuid());
                {
                    if (!string.IsNullOrEmpty(customer.Data?.FCMToken))
                    {
                        var firebaseReq = new FirebaseReqDto
                        {
                            ClientTokens = new List<string> { customer.Data.FCMToken },
                            Title = "Vehicle Status",
                            Body = req.IsActive ? "Your vehicle has been activated by Bai's Supervisor!" : "Your vehicle has been deactivated by Bai's Supervisor!"
                        };
                        var notificationResult = await _firebaseService.SendNotificationAsync(firebaseReq);

                        if (notificationResult.IsSuccess == false)
                        {
                            _logger.LogError("Failed to send notification to customer Id {CustomerId}. Error: {Error}", customer.Data.Id, notificationResult.InternalErrorMessage);
                        }
                    }
                }

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
                        PlateImage = vehicle.Data.PlateImage ?? "",
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

        public async Task<Return<dynamic>> UpdateVehicleInformationByUserAsync(UpdateCustomerVehicleByUserReqDto req)
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
                var vehicle = await _vehicleRepository.GetVehicleByIdAsync(req.VehicleId);
                if (!vehicle.IsSuccess || vehicle.Data is null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.VEHICLE_NOT_EXIST,
                        InternalErrorMessage = vehicle.InternalErrorMessage
                    };
                }
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
                if (newestSession.Data?.Status == (SessionEnum.PARKED))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.VEHICLE_IS_IN_SESSION
                    };
                }

                if (req.PlateNumber != null)
                {
                    // Check input 
                    if (string.IsNullOrEmpty(req.PlateNumber))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.NOT_A_PLATE_NUMBER,
                        };
                    }

                    // Check if the input PlateNumber is the same as the existing PlateNumber
                    if (vehicle.Data.PlateNumber != null && req.PlateNumber.Trim().Replace("-", "").Replace(".", "").Replace(" ", "") != vehicle.Data.PlateNumber)
                    {
                        // Check Plate Number is valid
                        req.PlateNumber = req.PlateNumber.Trim().Replace("-", "").Replace(".", "").Replace(" ", "");
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                        Regex regex = new(@"^[0-9]{2}[A-ZĐ]{1,2}[0-9]{4,6}$");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                        if (!regex.IsMatch(req.PlateNumber))
                        {
                            return new Return<dynamic>
                            {
                                Message = ErrorEnumApplication.NOT_A_PLATE_NUMBER
                            };
                        }

                        // Check PlateNumber is exist 
                        var vehicleByPlateNumber = await _vehicleRepository.GetVehicleByPlateNumberAsync(req.PlateNumber);
                        if (!vehicleByPlateNumber.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                        {
                            return new Return<dynamic>
                            {
                                InternalErrorMessage = vehicleByPlateNumber.InternalErrorMessage,
                                Message = ErrorEnumApplication.PLATE_NUMBER_IS_EXIST
                            };
                        }
                    }

                    vehicle.Data.PlateNumber = req.PlateNumber;
                }
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
                    vehicle.Data.VehicleTypeId = req.VehicleTypeId.Value;
                }
                vehicle.Data.StatusVehicle = StatusVehicleEnum.ACTIVE;
                vehicle.Data.LastModifyById = checkAuth.Data.Id;
                vehicle.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var result = await _vehicleRepository.UpdateVehicleAsync(vehicle.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };

                // Firebase send notification
                var customer = await _customerRepository.GetCustomerByIdAsync(vehicle.Data.CustomerId ?? Guid.NewGuid());
                {
                    if (!string.IsNullOrEmpty(customer.Data?.FCMToken))
                    {
                        var firebaseReq = new FirebaseReqDto
                        {
                            ClientTokens = new List<string> { customer.Data.FCMToken },
                            Title = "Vehicle Information Updated",
                            Body = $"Your vehicle with plate number {Helper.Utilities.FormatPlateNumber(vehicle.Data.PlateNumber)} has been updated by Bai's Manager! Open the Bai app to see more."
                        };
                        var notificationResult = await _firebaseService.SendNotificationAsync(firebaseReq);

                        if (notificationResult.IsSuccess == false)
                        {
                            _logger.LogError("Failed to send notification to customer Id {CustomerId}. Error: {Error}", customer.Data.Id, notificationResult.InternalErrorMessage);
                        }
                    }
                }
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

        public async Task<Return<IEnumerable<GetVehicleForUserResDto>>> GetListVehicleByCustomerIdForUserAsync(Guid customerId)
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

                var result = await _vehicleRepository.GetAllCustomerVehicleByCustomerIdAsync(customerId);
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
                        PlateImage = x.PlateImage ?? "",
                        StatusVehicle = x.StatusVehicle,
                        CreatedDate = x.CreatedDate,
                        Email = x.Customer?.Email ?? "",
                        LastModifyBy = x.LastModifyBy?.Email ?? "",
                        LastModifyDate = x.LastModifyDate,
                        StaffApproval = x.Staff?.Email ?? "",
                        VehicleTypeId = x.VehicleTypeId
                    }),
                    Message = result.TotalRecord > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = true,
                    TotalRecord = result.TotalRecord
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetVehicleForUserResDto>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> CreateListVehicleForCustomerByUserAsync(CreateListVehicleForCustomerByUserReqDto req)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
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

                var customer = await _customerRepository.GetCustomerByIdAsync(req.CustomerId);
                if (!customer.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customer.InternalErrorMessage };
                if (customer.Data is null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.CUSTOMER_NOT_EXIST };

                foreach (var item in req.Vehicles)
                {
                    var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(item.VehicleTypeId);
                    if (!vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || vehicleType.Data is null)
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                        };
                    }
                    // Check plate number exists
                    if (string.IsNullOrEmpty(item.PlateNumber) && item.PlateNumber == null)
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.NOT_A_PLATE_NUMBER,
                        };
                    }
                    // Check Plate Number is valid
                    item.PlateNumber = item.PlateNumber.Trim().Replace("-", "").Replace(".", "").Replace(" ", "");
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                    Regex regex = new(@"^[0-9]{2}[A-ZĐ]{1,2}[0-9]{4,6}$");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                    if (!regex.IsMatch(item.PlateNumber))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.NOT_A_PLATE_NUMBER
                        };
                    }
                    var vehicle = await _vehicleRepository.GetVehicleByPlateNumberAsync(item.PlateNumber);
                    if (!vehicle.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = vehicle.InternalErrorMessage,
                            Message = ErrorEnumApplication.PLATE_NUMBER_IS_EXIST
                        };
                    }
                    var newVehicle = new Vehicle
                    {
                        PlateNumber = item.PlateNumber,
                        VehicleTypeId = item.VehicleTypeId,
                        CustomerId = req.CustomerId,
                        StatusVehicle = StatusVehicleEnum.ACTIVE,
                        StaffId = checkAuth.Data.Id,
                    };
                    var result = await _vehicleRepository.CreateVehicleAsync(newVehicle);
                    if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || result.Data == null)
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = result.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                }
                scope.Complete();

                // send mail to customer
                MailRequest mailRequest = new()
                {
                    ToEmail = customer.Data.Email,
                    ToUsername = customer.Data.FullName,
                    Subject = "Vehicle Registration Confirmation",
                    Body = $"We are pleased to inform you that your vehicle(s) has been successfully added by Bai's Supervisor. " +
                           "You can now use the Bai app to view your registered vehicles."
                };

                await _mailService.SendEmailAsync(mailRequest);

                // Firebase send notification
                if (!string.IsNullOrEmpty(customer.Data.FCMToken))
                {
                    var firebaseReq = new FirebaseReqDto
                    {
                        ClientTokens = new List<string> { customer.Data.FCMToken },
                        Title = "Vehicle Registration",
                        Body = "Your vehicle has been added by Bai's Supervisor. Open the Bai app to see more."
                    };
                    var notificationResult = await _firebaseService.SendNotificationAsync(firebaseReq);

                    if (notificationResult.IsSuccess == false)
                    {
                        _logger.LogError("Failed to send notification to customer Id {CustomerId}. Error: {Error}", customer.Data.Id, notificationResult.InternalErrorMessage);
                    }
                }

                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
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

        public async Task<Return<dynamic>> DeleteVehicleByUserAsync(Guid vehicleId)
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

                var vehicle = await _vehicleRepository.GetVehicleByIdAsync(vehicleId);
                if (!vehicle.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicle.InternalErrorMessage };
                if (vehicle.Data == null || !vehicle.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_NOT_EXIST };
                vehicle.Data.DeletedDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                vehicle.Data.StatusVehicle = StatusVehicleEnum.INACTIVE;
                vehicle.Data.LastModifyById = checkAuth.Data.Id;
                vehicle.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var result = await _vehicleRepository.UpdateVehicleAsync(vehicle.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };

                // Firebase send notification
                var customer = await _customerRepository.GetCustomerByIdAsync(vehicle.Data.CustomerId ?? Guid.NewGuid());
                {
                    if (customer.Data != null && customer.Data is not null)
                    {
                        if (!string.IsNullOrEmpty(customer.Data.FCMToken))
                        {
                            var firebaseReq = new FirebaseReqDto
                            {
                                ClientTokens = new List<string> { customer.Data.FCMToken },
                                Title = "Vehicle Information Deleteted",
                                Body = $"Your vehicle information with plate number {Helper.Utilities.FormatPlateNumber(vehicle.Data.PlateNumber)} has been deleteted by Bai's Supervisor. Open the Bai app to see more."
                            };
                            var notificationResult = await _firebaseService.SendNotificationAsync(firebaseReq);

                            if (notificationResult.IsSuccess == false)
                            {
                                _logger.LogError("Failed to send notification to customer Id {CustomerId}. Error: {Error}", customer.Data.Id, notificationResult.InternalErrorMessage);
                            }
                        }

                        MailRequest mailRequest = new()
                        {
                            ToEmail = customer.Data.Email,
                            ToUsername = customer.Data.FullName,
                            Subject = "Vehicle Information Deleteted",
                            Body = $"We are sorry to inform you that your vehicle with plate number {Helper.Utilities.FormatPlateNumber(vehicle.Data.PlateNumber)} has been deleted by Bai's Supervisor. " +
                              "Please contact Bai's Supervisor for more information."
                        };

                        await _mailService.SendEmailAsync(mailRequest);
                    }
                }

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
