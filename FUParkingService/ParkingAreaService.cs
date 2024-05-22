using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using Microsoft.IdentityModel.Tokens;

namespace FUParkingService
{
    public class ParkingAreaService : IParkingAreaService
    {
        private readonly IParkingAreaRepository _parkingAreaRepository;
        private readonly IHelpperService _helpperService;
        private readonly IUserRepository _userRepository;

        public ParkingAreaService(IParkingAreaRepository parkingAreaRepository, IHelpperService helpperService, IUserRepository userRepository)
        {
            _parkingAreaRepository = parkingAreaRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
        }

        //public async Task<Return<bool>> ChangeStatusParkingAreaAsync(ChangeStatusParkingAreaReqDto req)
        //{
        //    try
        //    {
        //        // Check token 
        //        var isValidToken = _helpperService.IsTokenValid();
        //        if (!isValidToken)
        //        {
        //            return new Return<bool>
        //            {
        //                IsSuccess = false,
        //                Message = ErrorEnumApplication.NOT_AUTHORITY
        //            };
        //        }
        //        // Check role 
        //        var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
        //        if (userlogged.Data == null || userlogged.IsSuccess == false)
        //        {
        //            return new Return<bool>
        //            {
        //                IsSuccess = false,
        //                Message = ErrorEnumApplication.NOT_AUTHORITY
        //            };
        //        }
        //        if (!Auth.AuthManager.Contains((userlogged.Data.Role ?? new Role()).Name ?? ""))
        //        {
        //            return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
        //        }

        //        //Check ParkingAreaId is exist
        //        var parkingAreaList = await _parkingAreaRepository.GetParkingAreasAsync();

        //        if(parkingAreaList.Data == null)
        //        {
        //            return new Return<bool>
        //            {
        //                IsSuccess = false,
        //                Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST
        //            };
        //        }
        //        var existingParkingArea = parkingAreaList.Data.FirstOrDefault(x => x.Id == req.ParkingAreaId);
        //        if (existingParkingArea == null)
        //        {
        //            return new Return<bool>
        //            {
        //                IsSuccess = false,
        //                Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST
        //            };
        //        }

        //        if (req.IsActive)
        //        {
        //            if ((existingParkingArea.StatusParkingArea ?? "").Equals(StatusParkingEnum.ACTIVE))
        //            {
        //                return new Return<bool>
        //                {
        //                    IsSuccess = false,
        //                    Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
        //                };
        //            }
        //            else
        //            {
        //                existingParkingArea.StatusParkingArea = StatusParkingEnum.ACTIVE;
        //                // Update status Account
        //                var isUpdate = await _parkingAreaRepository.
        //                if (isUpdate.Data == null || isUpdate.IsSuccess == false)
        //                {
        //                    return new Return<bool>
        //                    {
        //                        IsSuccess = false,
        //                        Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR
        //                    };
        //                }
        //                else
        //                {
        //                    return new Return<bool> { IsSuccess = true, Data = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
        //                }
        //            }

        //            if (existingParkingArea.StatusParkingArea == StatusCustomerEnum.ACTIVE)
        //            {
        //                return new Return<bool>
        //                {
        //                    IsSuccess = false,
        //                    Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
        //                };
        //            }
        //            else
        //            {
        //                existingParkingArea.StatusParkingArea = StatusCustomerEnum.ACTIVE;
        //            }
        //        }
        //        else
        //        {
        //            if (existingParkingArea.StatusParkingArea == StatusCustomerEnum.INACTIVE)
        //            {
        //                return new Return<bool>
        //                {
        //                    IsSuccess = false,
        //                    Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
        //                };
        //            }
        //            else
        //            {
        //                existingParkingArea.StatusParkingArea = StatusCustomerEnum.INACTIVE;
        //            }
        //        }

        //        // Update the parking area status
        //        var updateResult = await _parkingAreaRepository.UpdateParkingAreaAsync(existingParkingArea);
        //        if (updateResult == null || !updateResult.IsSuccess)
        //        {
        //            return new Return<bool>
        //            {
        //                IsSuccess = false,
        //                Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR
        //            };
        //        }

        //        return new Return<bool>
        //        {
        //            IsSuccess = true,
        //            Data = true,
        //            Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
        //        };
        //    }
        //    } catch (Exception)
        //    {
        //        return new Return<bool>
        //        {
        //            IsSuccess = false,
        //            Message = ErrorEnumApplication.SERVER_ERROR,
        //            InternalErrorMessage = ex.Message
        //        };
        //    }
        //}

        public async Task<Return<bool>> CreateParkingAreaAsync(CreateParkingAreaReqDto req)
        {
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

                //create parking area
                var parkingAreaList = await _parkingAreaRepository.GetParkingAreasAsync();
                if(parkingAreaList.Data != null && parkingAreaList.IsSuccess)
                {
                    bool isParkingAreaNameExist = parkingAreaList.Data.Any(x => x.Name.Equals(req.Name, StringComparison.OrdinalIgnoreCase));

                    if (isParkingAreaNameExist == true)
                    {
                        return new Return<bool>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.GET_OBJECT_ERROR
                        };
                    }
                }

                var parkingArea = new ParkingArea
                {
                    Name = req.Name,
                    Description = req.Description,
                    MaxCapacity = req.MaxCapacity,
                    Block = req.Block,
                    Mode = req.Mode,
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                };

                var result = await _parkingAreaRepository.CreateParkingAreaAsync(parkingArea);
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
    }
}
