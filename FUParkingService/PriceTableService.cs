﻿using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class PriceTableService : IPriceTableService
    {
        private readonly IPriceTableRepository _priceTableRepository;
        private readonly IHelpperService _helpperService;
        private readonly IUserRepository _userRepository;
        private readonly IVehicleTypeRepository _vehicleTypeRepository;

        public PriceTableService(IPriceTableRepository priceTableRepository, IHelpperService helpperService, IUserRepository userRepository, IVehicleTypeRepository vehicleTypeRepository)
        {
            _priceTableRepository = priceTableRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
            _vehicleTypeRepository = vehicleTypeRepository;
        }

        public async Task<Return<bool>> CreatePriceTableAsync(CreatePriceTableReqDto req)
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
                if (!Auth.AuthManager.Contains((userlogged.Data.Role ?? new Role()).Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                // Check VehicleType is exist
                var isVehicleTypeExist = await _vehicleTypeRepository.GetVehicleTypeByIdAsync(req.VehicleTypeId);
                if (isVehicleTypeExist.Data == null || isVehicleTypeExist.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }

                var priceTable = new PriceTable
                {
                    VehicleTypeId = req.VehicleTypeId,
                    Priority = req.Priority,
                    Name = req.Name,
                    ApplyFromDate = req.ApplyFromDate ?? DefaultType.DefaultDateTime,
                    ApplyToDate = req.ApplyToDate ?? DefaultType.DefaultDateTime,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                };

                var result = await _priceTableRepository.CreatePriceTableAsync(priceTable);
                if (result.IsSuccess)
                {
                    return new Return<bool>
                    {
                        Data = true,
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                    };
                }
                else
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.ADD_OBJECT_ERROR
                    };
                }
            }
            catch (Exception e)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
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
                if (!Auth.AuthManager.Contains((userlogged.Data.Role ?? new Role()).Name ?? ""))
                {
                    return new Return<IEnumerable<GetPriceTableResDto>> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                var result = await _priceTableRepository.GetAllPriceTableAsync();
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
                        Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
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
                    InternalErrorMessage = e.Message
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
                if (!Auth.AuthManager.Contains((userlogged.Data.Role ?? new Role()).Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check PriceTable is exist
                var isPriceTableExist = await _priceTableRepository.GetPriceTableByIdAsync(req.PriceTableId);
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
                        var isUpdate = await _priceTableRepository.UpdatePriceTableAsync(isPriceTableExist.Data);
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
                        var isUpdate = await _priceTableRepository.UpdatePriceTableAsync(isPriceTableExist.Data);
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
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
