using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Price;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.Transactions;

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

        public async Task<Return<dynamic>> CreateDefaultPriceItemForDefaultPriceTableAsync(CreateDefaultItemPriceReqDto req)
        {
            try
            {
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
                if (!userlogged.IsSuccess)
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = userlogged.InternalErrorMessage };
                }

                if (!userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || userlogged.Data == null || !Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check VehicleType is exist
                var isVehicleTypeExist = await _vehicleRepository.GetVehicleTypeByIdAsync(req.VehicleTypeId);
                if (!isVehicleTypeExist.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isVehicleTypeExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (!isVehicleTypeExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }

                // Check Default PriceTable is exist
                var isDefaultPriceTableExist = await _priceRepository.GetDefaultPriceTableByVehicleTypeAsync(req.VehicleTypeId);
                if (!isDefaultPriceTableExist.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isDefaultPriceTableExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (isDefaultPriceTableExist.Data == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.DEFAULT_PRICE_TABLE_IS_NOT_EXIST
                    };
                }

                // Create Default PriceItem
                var priceItem = new PriceItem
                {
                    PriceTableId = isDefaultPriceTableExist.Data.Id,                    
                    MaxPrice = req.MaxPrice,
                    MinPrice = req.MinPrice,
                    BlockPricing = req.BlockPricing,
                    CreatedById = userlogged.Data.Id
                };
                var result = await _priceRepository.CreatePriceItemAsync(priceItem);
                if (!result.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<dynamic>()
                {
                    InternalErrorMessage = e,
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> CreatePriceItemAsync(CreateListPriceItemReqDto req)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
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
                if (!userlogged.IsSuccess)
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = userlogged.InternalErrorMessage };
                }

                if (!userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || userlogged.Data == null || !Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check PriceTableId is exist
                var isPriceTableExist = await _priceRepository.GetPriceTableByIdAsync(req.PriceTableId);
                if (isPriceTableExist.Data == null || isPriceTableExist.IsSuccess == false)
                {
                    return new Return<dynamic>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.PRICE_TABLE_NOT_EXIST
                    };
                }
                // Check price item is exist base on pricetableId
                var isPriceItemExist = await _priceRepository.GetAllPriceItemByPriceTableAsync(req.PriceTableId);
                if (!isPriceItemExist.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isPriceItemExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (isPriceItemExist.Data != null && isPriceItemExist.Data.Any())
                {
                    // Check the default price item is exist in the list and if anything else other than the default price item is found, return error
                    foreach (var item in isPriceItemExist.Data)
                    {
                        if (item.ApplyFromHour != null && item.ApplyToHour != null)
                        {
                            return new Return<dynamic>
                            {
                                Message = ErrorEnumApplication.PRICE_ITEM_IS_EXIST
                            };
                        }
                    }
                }
                // Get default price table
                var isDefaultPriceTableExist = await _priceRepository.GetDefaultPriceTableByVehicleTypeAsync(isPriceTableExist.Data.VehicleTypeId);
                if (!isDefaultPriceTableExist.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isDefaultPriceTableExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (isDefaultPriceTableExist.Data == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.DEFAULT_PRICE_TABLE_IS_NOT_EXIST
                    };
                }
                // Get price item default
                var priceItemDefault = await _priceRepository.GetDefaultPriceItemByPriceTableIdAsync(isDefaultPriceTableExist.Data.Id);
                if (!priceItemDefault.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = priceItemDefault.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (priceItemDefault.Data == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.PRICE_ITEM_NOT_EXIST
                    };
                }
                var listTime = new List<CreatePriceItemReqDto>(req.PriceItems);
                // Order list based on From
                listTime = [.. listTime.OrderBy(x => x.From)];

                // Check time from 0 => first time from have in list
                if (listTime[0].From > 0)
                {
                    listTime.Insert(0, new CreatePriceItemReqDto
                    {
                        From = 0,
                        To = listTime[0].From,
                        MaxPrice = priceItemDefault.Data.MaxPrice,
                        MinPrice = priceItemDefault.Data.MinPrice,
                        BlockPricing = priceItemDefault.Data.BlockPricing,                        
                    });
                }
                // To avoid modifying the collection while iterating, we'll collect required changes first
                var inserts = new List<(int index, CreatePriceItemReqDto item)>();
                for (int i = 1; i < listTime.Count; i++)
                {
                    if (listTime[i].From > listTime[i - 1].To)
                    {
                        var newItem = new CreatePriceItemReqDto
                        {
                            From = listTime[i - 1].To,
                            To = listTime[i].From,
                            MaxPrice = priceItemDefault.Data.MaxPrice,
                            MinPrice = priceItemDefault.Data.MinPrice,
                            BlockPricing = priceItemDefault.Data.BlockPricing,
                        };
                        inserts.Add((i, newItem));
                    }
                }
                // Apply the collected changes
                foreach (var (index, item) in inserts.OrderByDescending(x => x.index))
                {
                    listTime.Insert(index, item);
                }

                // Check time from last time to 24
                if (listTime[^1].To < 24)
                {
                    listTime.Add(new CreatePriceItemReqDto
                    {
                        From = listTime[^1].To,
                        To = 24,
                        MaxPrice = priceItemDefault.Data.MaxPrice,
                        MinPrice = priceItemDefault.Data.MinPrice,
                        BlockPricing = priceItemDefault.Data.BlockPricing,
                    });
                }
                // Create PriceItem
                foreach (var item in listTime)
                {
                    var priceItem = new PriceItem
                    {
                        PriceTableId = req.PriceTableId,
                        ApplyFromHour = item.From,
                        ApplyToHour = item.To,
                        MaxPrice = item.MaxPrice,
                        MinPrice = item.MinPrice,
                        CreatedById = userlogged.Data.Id,
                        BlockPricing = item.BlockPricing,
                    };
                    var result = await _priceRepository.CreatePriceItemAsync(priceItem);
                    if (!result.IsSuccess)
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = result.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                    if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                }
                scope.Complete();
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                scope.Dispose();
                return new Return<dynamic>()
                {
                    InternalErrorMessage = e,                    
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
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                }
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

        public async Task<Return<dynamic>> CreateDefaultPriceTableAsync(CreateDefaultPriceTableReqDto req)
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
                if (!isVehicleTypeExist.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isVehicleTypeExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (!isVehicleTypeExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }
                // Check this VehicleType is exist default PriceTable
                var isDefaultPriceTableExist = await _priceRepository.GetDefaultPriceTableByVehicleTypeAsync(req.VehicleTypeId);
                if (!isDefaultPriceTableExist.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isDefaultPriceTableExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (isDefaultPriceTableExist.Data != null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.DEFAULT_PRICE_TABLE_IS_EXIST
                    };
                }
                // Create Default PriceTable
                var priceTable = new PriceTable
                {
                    VehicleTypeId = req.VehicleTypeId,
                    Priority = 1,
                    Name = "Default Price Table",
                    ApplyFromDate = null,
                    ApplyToDate = null,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    CreatedById = userlogged.Data.Id
                };
                var result = await _priceRepository.CreatePriceTableAsync(priceTable);
                if (!result.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<dynamic>
                {
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
                // Check the default price table is exist
                var isDefaultPriceTableExist = await _priceRepository.GetDefaultPriceTableByVehicleTypeAsync(req.VehicleTypeId);
                if (!isDefaultPriceTableExist.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isDefaultPriceTableExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (isDefaultPriceTableExist.Data == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.DEFAULT_PRICE_TABLE_IS_NOT_EXIST
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
                    IsSuccess = true,
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
