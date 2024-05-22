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
                if (parkingAreaList.Data != null && parkingAreaList.IsSuccess)
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

        public async Task<Return<bool>> UpdateParkingAreaAsync(UpdateParkingAreaReqDto req)
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

                // Check if parking area exists
                var parkingAreaList = await _parkingAreaRepository.GetParkingAreasAsync();
                if (parkingAreaList == null || !parkingAreaList.IsSuccess || parkingAreaList.Data == null)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR
                    };
                }

                var existingParkingArea = parkingAreaList.Data.FirstOrDefault(x => x.Id == req.ParkingAreaId);
                if (existingParkingArea == null)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR
                    };
                }

                // Check for duplicate parking area name (excluding the current parking area)
                var isDuplicateName = parkingAreaList.Data.Any(x => x.Name.Equals(req.Name, StringComparison.OrdinalIgnoreCase) && x.Id != req.ParkingAreaId);
                if (isDuplicateName)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR
                    };
                }

                if (!string.IsNullOrEmpty(req.Name))
                {
                    existingParkingArea.Name = req.Name;
                }
                if (!string.IsNullOrEmpty(req.Description))
                {
                    existingParkingArea.Description = req.Description;
                }
                if (req.MaxCapacity.HasValue)
                {
                    existingParkingArea.MaxCapacity = req.MaxCapacity.Value;
                }
                if (req.Block.HasValue)
                {
                    existingParkingArea.Block = req.Block.Value;
                }
                if (!string.IsNullOrEmpty(req.Mode))
                {
                    existingParkingArea.Mode = req.Mode;
                }

                var updateResult = await _parkingAreaRepository.UpdateParkingAreaAsync(existingParkingArea);
                if (updateResult.IsSuccess)
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
