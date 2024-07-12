using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class PriceSevice : IPriceService
    {
        private readonly IPriceRepository _priceRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IHelpperService _helpperService;
        private readonly IUserRepository _userRepository;

        public PriceSevice(IPriceRepository priceRepository, IVehicleRepository vehicleRepository, IHelpperService helpperService, IUserRepository userRepository)
        {
            _priceRepository = priceRepository;
            _vehicleRepository = vehicleRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
        }

        public async Task<Return<bool>> CreatePriceItemAsync(CreatePriceItemReqDto req)
        {
            // Check token 
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check PriceTableId is exist
                var isPriceTableExist = await _priceRepository.GetPriceTableByIdAsync(req.PriceTableId);
                if (isPriceTableExist.Data == null || isPriceTableExist.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.PRICE_TABLE_NOT_EXIST
                    };
                }

                // Add PriceItem
                var priceItem = new PriceItem
                {
                    PriceTableId = req.PriceTableId,
                    ApplyFromHour = req.ApplyFromHour.HasValue ? req.ApplyFromHour.Value.Hour : null,
                    ApplyToHour = req.ApplyToHour.HasValue ? req.ApplyToHour.Value.Hour : null,
                    MaxPrice = req.MaxPrice,
                    MinPrice = req.MinPrice
                };

                var result = await _priceRepository.CreatePriceItemAsync(priceItem);
                if (result.IsSuccess)
                {
                    return new Return<bool>
                    {
                        IsSuccess = true,
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
            }
            catch (Exception e)
            {
                return new Return<bool>()
                {
                    InternalErrorMessage = e,
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<bool>> DeletePriceItemAsync(Guid id)
        {
            try
            {
                // Check token 
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                // Check PriceItem is exist
                var isPriceItemExist = await _priceRepository.GetPriceItemByIdAsync(id);

                if (isPriceItemExist.Data == null || isPriceItemExist.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.PRICE_ITEM_NOT_EXIST
                    };
                }
                else
                {
                    var result = await _priceRepository.DeletePriceItemAsync(isPriceItemExist.Data);
                    if (result.IsSuccess)
                    {
                        return new Return<bool>
                        {
                            IsSuccess = true,
                            Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
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
                }
            }
            catch (Exception e)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<PriceItem>>> GetAllPriceItemByPriceTableAsync(Guid PriceTableId)
        {
            try
            {
                // Check token 
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<IEnumerable<PriceItem>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<IEnumerable<PriceItem>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<PriceItem>> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check PriceTableId is exist
                var isPriceTableExist = await _priceRepository.GetPriceTableByIdAsync(PriceTableId);
                if (isPriceTableExist.Data == null || isPriceTableExist.IsSuccess == false)
                {
                    return new Return<IEnumerable<PriceItem>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.PRICE_TABLE_NOT_EXIST
                    };
                }

                var result = await _priceRepository.GetAllPriceItemByPriceTableAsync(PriceTableId);
                if (result.IsSuccess)
                {
                    return new Return<IEnumerable<PriceItem>>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.FOUND_OBJECT
                    };
                }
                else
                {
                    return new Return<IEnumerable<PriceItem>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = false,
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> CreatePriceTableAsync(CreatePriceTableReqDto req)
        {
            try
            {
                // Check token 
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                // Check VehicleType is exist
                var isVehicleTypeExist = await _vehicleRepository.GetVehicleTypeByIdAsync(req.VehicleTypeId);
                if (isVehicleTypeExist.Data == null || isVehicleTypeExist.IsSuccess == false)
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }
                // Check priority is exist
                var isPriorityExist = await _priceRepository.GetPriceTableByPriorityAndVehicleTypeAsync(req.Priority, isVehicleTypeExist.Data.Id);
                if (!isPriorityExist.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = isPriorityExist.InternalErrorMessage };
                if (isPriorityExist.Data != null && isPriorityExist.IsSuccess == true)
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.PRIORITY_IS_EXIST
                    };
                }
                // Check 
                var priceTable = new PriceTable
                {
                    VehicleTypeId = req.VehicleTypeId,
                    Priority = req.Priority,
                    Name = req.Name,
                    ApplyFromDate = req.ApplyFromDate,
                    ApplyToDate = req.ApplyToDate,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                };
                var result = await _priceRepository.CreatePriceTableAsync(priceTable);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                }
                return new Return<dynamic>
                {
                    Data = result.Data,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<dynamic>
                {                    
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<GetPriceTableResDto>>> GetAllPriceTableAsync()
        {
            try
            {
                // Check token 
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<IEnumerable<GetPriceTableResDto>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<IEnumerable<GetPriceTableResDto>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<GetPriceTableResDto>> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                var result = await _priceRepository.GetAllPriceTableAsync();
                if (result.IsSuccess && !(result.Data == null))
                {
                    var listPriceTable = new List<GetPriceTableResDto>();
                    foreach (var item in result.Data)
                    {
                        listPriceTable.Add(new GetPriceTableResDto
                        {
                            PriceTableId = item.Id,
                            Name = item.Name,
                            ApplyFromDate = item.ApplyFromDate,
                            ApplyToDate = item.ApplyToDate,
                            Priority = item.Priority,
                            StatusPriceTable = item.StatusPriceTable ?? "",
                            VehicleType = item.VehicleType?.Name ?? ""
                        });
                    }
                    return new Return<IEnumerable<GetPriceTableResDto>>
                    {
                        Data = listPriceTable,
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.FOUND_OBJECT
                    };
                }
                else
                {
                    return new Return<IEnumerable<GetPriceTableResDto>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR
                    };
                }
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<GetPriceTableResDto>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<bool>> UpdateStatusPriceTableAsync(ChangeStatusPriceTableReqDto req)
        {
            try
            {
                // Check token 
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check PriceTable is exist
                var isPriceTableExist = await _priceRepository.GetPriceTableByIdAsync(req.PriceTableId);
                if (isPriceTableExist.Data == null || isPriceTableExist.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.PRICE_TABLE_NOT_EXIST
                    };
                }

                // 
                if (req.IsActive)
                {
                    if ((isPriceTableExist.Data.StatusPriceTable ?? "").Equals(StatusPriceTableEnum.ACTIVE))
                    {
                        return new Return<bool>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    else
                    {
                        isPriceTableExist.Data.StatusPriceTable = StatusPriceTableEnum.ACTIVE;
                        var isUpdate = await _priceRepository.UpdatePriceTableAsync(isPriceTableExist.Data);
                        if (isUpdate.Data == null || isUpdate.IsSuccess == false)
                        {
                            return new Return<bool>
                            {
                                IsSuccess = false,
                                Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR
                            };
                        }
                        else
                        {
                            return new Return<bool> { IsSuccess = true, Data = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
                        }
                    }
                }
                else
                {
                    if ((isPriceTableExist.Data.StatusPriceTable ?? "").Equals(StatusPriceTableEnum.INACTIVE))
                    {
                        return new Return<bool>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    else
                    {
                        isPriceTableExist.Data.StatusPriceTable = StatusPriceTableEnum.ACTIVE;
                        // Update status Account
                        var isUpdate = await _priceRepository.UpdatePriceTableAsync(isPriceTableExist.Data);
                        if (isUpdate.Data == null || isUpdate.IsSuccess == false)
                        {
                            return new Return<bool>
                            {
                                IsSuccess = false,
                                Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR
                            };
                        }
                        else
                        {
                            return new Return<bool> { IsSuccess = true, Data = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }
    }
}
