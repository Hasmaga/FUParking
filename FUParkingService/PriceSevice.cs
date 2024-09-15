using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Price;
using FUParkingModel.ResponseObject;
using FUParkingModel.ResponseObject.Price;
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

        public PriceSevice(IPriceRepository priceRepository, IVehicleRepository vehicleRepository, IHelpperService helpperService)
        {
            _priceRepository = priceRepository;
            _vehicleRepository = vehicleRepository;
            _helpperService = helpperService;            
        }

        public async Task<Return<dynamic>> CreateDefaultPriceItemForDefaultPriceTableAsync(CreateDefaultItemPriceReqDto req)
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
                var isVehicleTypeExist = await _vehicleRepository.GetVehicleTypeByIdAsync(req.VehicleTypeId);
                if (!isVehicleTypeExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isVehicleTypeExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }

                // Check Default PriceTable is exist
                var isDefaultPriceTableExist = await _priceRepository.GetDefaultPriceTableByVehicleTypeAsync(req.VehicleTypeId);
                if (!isDefaultPriceTableExist.IsSuccess || isDefaultPriceTableExist.Data == null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isDefaultPriceTableExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.DEFAULT_PRICE_TABLE_IS_NOT_EXIST
                    };
                }
                var priceItem = new PriceItem
                {
                    PriceTableId = isDefaultPriceTableExist.Data.Id,
                    MaxPrice = req.MaxPrice,
                    MinPrice = req.MinPrice,
                    BlockPricing = req.BlockPricing,
                    CreatedById = checkAuth.Data.Id
                };
                var result = await _priceRepository.CreatePriceItemAsync(priceItem);
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
            catch (Exception e)
            {
                return new Return<dynamic>()
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> CreatePriceItemAsync(CreateListPriceItemReqDto req)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check PriceTableId is exist
                var isPriceTableExist = await _priceRepository.GetPriceTableByIdAsync(req.PriceTableId);
                if (isPriceTableExist.Data is null || !isPriceTableExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isPriceTableExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.PRICE_TABLE_NOT_EXIST
                    };
                }
                // Check price item is exist base on pricetableId
                var isPriceItemExist = await _priceRepository.GetAllPriceItemByPriceTableAsync(req.PriceTableId);
                if (!isPriceItemExist.IsSuccess)
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isPriceItemExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (isPriceItemExist.Data != null && isPriceItemExist.Data.Any())
                {
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
                if (!isDefaultPriceTableExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || isDefaultPriceTableExist.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isDefaultPriceTableExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.DEFAULT_PRICE_TABLE_IS_NOT_EXIST
                    };
                }
                // Get price item default
                var priceItemDefault = await _priceRepository.GetDefaultPriceItemByPriceTableIdAsync(isDefaultPriceTableExist.Data.Id);
                if (!priceItemDefault.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || priceItemDefault.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = priceItemDefault.InternalErrorMessage,
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
                        CreatedById = checkAuth.Data.Id,
                        BlockPricing = item.BlockPricing,
                    };
                    var result = await _priceRepository.CreatePriceItemAsync(priceItem);
                    if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
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

        public async Task<Return<IEnumerable<GetPriceItemResDto>>> GetAllPriceItemByPriceTableAsync(Guid PriceTableId, GetListObjectWithPageReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetPriceItemResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check PriceTableId is exist
                var isPriceTableExist = await _priceRepository.GetPriceTableByIdAsync(PriceTableId);
                if (isPriceTableExist.Data == null || isPriceTableExist.IsSuccess == false)
                {
                    return new Return<IEnumerable<GetPriceItemResDto>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.PRICE_TABLE_NOT_EXIST
                    };
                }

                var result = await _priceRepository.GetAllPriceItemByPriceTableWithPageAsync(PriceTableId, req);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetPriceItemResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }          
                return new Return<IEnumerable<GetPriceItemResDto>>
                {
                    Data = result.Data?.Select(t => new GetPriceItemResDto
                    {
                        ApplyToHour = t.ApplyToHour,
                        ApplyFromHour = t.ApplyFromHour,
                        MaxPrice = t.MaxPrice,
                        MinPrice = t.MinPrice,
                        BlockPricing = t.BlockPricing,
                        CreatedBy = t.CreateBy?.Email ?? "",
                        CreatedDate = t.CreatedDate,
                        Id = t.Id,
                        LastModifyBy = t.LastModifyBy?.Email ?? "",
                        LastModifyDate = t.LastModifyDate
                    }),
                    IsSuccess = true,
                    TotalRecord = result.TotalRecord,
                    Message = result.Message
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<GetPriceItemResDto>>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<bool>> UpdatePriceItemAsync(CreateListPriceItemReqDto req)
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
                // Check PriceTableId is exist
                var isPriceTableExist = await _priceRepository.GetPriceTableByIdAsync(req.PriceTableId);
                if (isPriceTableExist.Data is null || !isPriceTableExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    scope.Dispose();
                    return new Return<bool>
                    {
                        InternalErrorMessage = isPriceTableExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.PRICE_TABLE_NOT_EXIST
                    };
                }
                // Check have default price item
                var isDefaultPriceTableExist = await _priceRepository.GetDefaultPriceItemByPriceTableIdAsync(isPriceTableExist.Data.VehicleTypeId);
                if (isDefaultPriceTableExist.Data is null || !isDefaultPriceTableExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    scope.Dispose();
                    return new Return<bool>
                    {
                        InternalErrorMessage = isDefaultPriceTableExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.DEFAULT_PRICE_ITEM_NOT_EXIST
                    };
                }
                // Delete all price item
                var isDeleteAllPriceItem = await _priceRepository.DeleteAllPriceItemByPriceTableIdAsync(req.PriceTableId);
                if (!isDeleteAllPriceItem.IsSuccess)
                {
                    scope.Dispose();
                    return new Return<bool>
                    {
                        InternalErrorMessage = isDeleteAllPriceItem.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                // Create new price item
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
                        MaxPrice = isDefaultPriceTableExist.Data.MaxPrice,
                        MinPrice = isDefaultPriceTableExist.Data.MinPrice,
                        BlockPricing = isDefaultPriceTableExist.Data.BlockPricing,
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
                            MaxPrice = isDefaultPriceTableExist.Data.MaxPrice,
                            MinPrice = isDefaultPriceTableExist.Data.MinPrice,
                            BlockPricing = isDefaultPriceTableExist.Data.BlockPricing,
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
                        MaxPrice = isDefaultPriceTableExist.Data.MaxPrice,
                        MinPrice = isDefaultPriceTableExist.Data.MinPrice,
                        BlockPricing = isDefaultPriceTableExist.Data.BlockPricing,
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
                        CreatedById = checkAuth.Data.Id,
                        BlockPricing = item.BlockPricing,
                    };
                    var result = await _priceRepository.CreatePriceItemAsync(priceItem);
                    if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                    {
                        scope.Dispose();
                        return new Return<bool>
                        {
                            InternalErrorMessage = result.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                }
                scope.Complete();
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<dynamic>> CreateDefaultPriceTableAsync(CreateDefaultPriceTableReqDto req)
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
                    CreatedById = checkAuth.Data.Id
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
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check VehicleType is exist
                var isVehicleTypeExist = await _vehicleRepository.GetVehicleTypeByIdAsync(req.VehicleTypeId);
                if (isVehicleTypeExist.Data == null || !isVehicleTypeExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isVehicleTypeExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }
                // Check priority is exist
                var isPriorityExist = await _priceRepository.GetPriceTableByPriorityAndVehicleTypeAsync(req.Priority, isVehicleTypeExist.Data.Id);
                if (!isPriorityExist.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isPriorityExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.PRIORITY_IS_EXIST
                    };
                }
                var priceTable = new PriceTable
                {
                    VehicleTypeId = req.VehicleTypeId,
                    Priority = req.Priority,
                    Name = req.Name,
                    ApplyFromDate = req.ApplyFromDate,
                    ApplyToDate = req.ApplyToDate,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    CreatedById = checkAuth.Data.Id
                };
                var result = await _priceRepository.CreatePriceTableAsync(priceTable);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || result.Data is null)
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = result.InternalErrorMessage
                    };
                }
                // Create default price item for this price table
                var priceItem = new PriceItem
                {
                    PriceTableId = result.Data.Id,
                    MaxPrice = req.MaxPrice,
                    MinPrice = req.MinPrice,
                    BlockPricing = req.PricePerBlock,
                    CreatedById = checkAuth.Data.Id
                };
                var resultPriceItem = await _priceRepository.CreatePriceItemAsync(priceItem);
                if (!resultPriceItem.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = resultPriceItem.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
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
                return new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<GetPriceTableResDto>>> GetAllPriceTableAsync(GetListObjectWithFiller req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {                    
                    return new Return<IEnumerable<GetPriceTableResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _priceRepository.GetAllPriceTableAsync(req);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetPriceTableResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<IEnumerable<GetPriceTableResDto>>
                {
                    Data = result.Data?.Select(t => new GetPriceTableResDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        ApplyFromDate = t.ApplyFromDate,
                        ApplyToDate = t.ApplyToDate,
                        Priority = t.Priority,                        
                        StatusPriceTable = t.StatusPriceTable,
                        VehicleType = t.VehicleType?.Name ?? ""
                    }),
                    IsSuccess = true,
                    TotalRecord = result.TotalRecord,
                    Message = result.Message
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<GetPriceTableResDto>>
                {                    
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<bool>> UpdateStatusPriceTableAsync(ChangeStatusPriceTableReqDto req)
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
                // Check PriceTable is exist
                var isPriceTableExist = await _priceRepository.GetPriceTableByIdAsync(req.PriceTableId);
                if (isPriceTableExist.Data == null || isPriceTableExist.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.PRICE_TABLE_NOT_EXIST
                    };
                }

                // Check is default price table then can not update status
                if (isPriceTableExist.Data.Priority == 1)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.CAN_NOT_UPDATE_STATUS_DEFAULT_PRICE_TABLE
                    };
                }

                if (req.IsActive)
                {
                    if ((isPriceTableExist.Data.StatusPriceTable ?? "").Equals(StatusPriceTableEnum.ACTIVE))
                    {
                        return new Return<bool>
                        {
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
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    else
                    {
                        isPriceTableExist.Data.StatusPriceTable = StatusPriceTableEnum.INACTIVE;
                        // Update status Account
                        var isUpdate = await _priceRepository.UpdatePriceTableAsync(isPriceTableExist.Data);
                        if (isUpdate.Data == null || isUpdate.IsSuccess == false)
                        {
                            return new Return<bool>
                            {
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
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<bool>> DeletePriceTableAsync(Guid pricetableId)
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
                // Check PriceTable is exist
                var isPriceTableExist = await _priceRepository.GetPriceTableByIdAsync(pricetableId);
                if (isPriceTableExist.Data == null || isPriceTableExist.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.PRICE_TABLE_NOT_EXIST
                    };
                }
                // Check if is default price table then can not delete
                if (isPriceTableExist.Data.Priority == 1)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.CAN_NOT_DELETE_DEFAULT_PRICE_TABLE
                    };
                }
                isPriceTableExist.Data.StatusPriceTable = StatusPriceTableEnum.INACTIVE;
                isPriceTableExist.Data.LastModifyById = checkAuth.Data.Id;
                isPriceTableExist.Data.LastModifyDate = DateTime.Now;
                var isUpdate = await _priceRepository.UpdatePriceTableAsync(isPriceTableExist.Data);
                if (!isUpdate.IsSuccess)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = isUpdate.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };

            }
            catch (Exception e)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<dynamic>> UpdatePriceTableAsync(UpdatePriceTableReqDto req)
        {
            try
            {
                // check auth
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                // check price table
                var isPriceTableExist = await _priceRepository.GetPriceTableByIdAsync(req.PriceTableId);
                if (isPriceTableExist.Data is null || !isPriceTableExist.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.PRICE_TABLE_NOT_EXIST
                    };
                }

                // if default price table
                if (isPriceTableExist.Data.Priority == 1)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.CAN_NOT_UPDATE_DEFAULT_PRICE_TABLE
                    };
                }

                isPriceTableExist.Data.Name = req.Name ?? isPriceTableExist.Data.Name;
                isPriceTableExist.Data.ApplyFromDate = req.ApplyFromDate;
                isPriceTableExist.Data.ApplyToDate = req.ApplyToDate;
                isPriceTableExist.Data.LastModifyById = checkAuth.Data.Id;
                isPriceTableExist.Data.LastModifyDate = DateTime.Now;

                var result = await _priceRepository.UpdatePriceTableAsync(isPriceTableExist.Data);
                if (!result.IsSuccess)
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
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
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
    }
}
