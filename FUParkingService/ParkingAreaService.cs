using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Builder;

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

        public async Task<Return<dynamic>> DeleteParkingArea(Guid id)
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
                if (userlogged.Data == null || !userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
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

                // Check if ParkingAreaId exists
                var existedParking = await _parkingAreaRepository.GetParkingAreaByIdAsync(id);
                if (existedParking.Data == null || existedParking.IsSuccess == false)
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST
                    };
                }                
                existedParking.Data.DeletedDate = DateTime.Now;

                var result = await _parkingAreaRepository.UpdateParkingAreaAsync(existedParking.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,                    
                    Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
                };
                
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {                    
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<dynamic>> CreateParkingAreaAsync(CreateParkingAreaReqDto req)
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
                if (userlogged.Data == null || !userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
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

                // Check for duplicate parking area name
                var isDuplicateName = await _parkingAreaRepository.GetParkingAreaByNameAsync(req.Name);
                if (isDuplicateName.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || isDuplicateName.Data != null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.OBJECT_EXISTED
                    };
                }

                var parkingArea = new ParkingArea
                {
                    Mode = ModeEnum.MODE1,
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Name = req.Name,
                    Description = req.Description,
                    MaxCapacity = req.MaxCapacity,
                    Block = req.Block,
                    CreatedById = userlogged.Data.Id,
                };

                var result = await _parkingAreaRepository.CreateParkingAreaAsync(parkingArea);
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
            catch (Exception ex)
            {
                return new Return<dynamic>
                {                    
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }

        }

        public async Task<Return<IEnumerable<ParkingArea>>> GetParkingAreasAsync(int pageIndex, int pageSize)
        {
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<IEnumerable<ParkingArea>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || !userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<IEnumerable<ParkingArea>>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthSupervisor.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<ParkingArea>> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                var result = await _parkingAreaRepository.GetAllParkingAreasAsync(pageIndex, pageSize);
                if (!result.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<IEnumerable<ParkingArea>>
                    {
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR
                    };
                }
                return new Return<IEnumerable<ParkingArea>>
                {
                    IsSuccess = true,                    
                    TotalRecord = result.TotalRecord,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<ParkingArea>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> UpdateParkingAreaAsync(UpdateParkingAreaReqDto req)
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
                if (userlogged.Data == null || !userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
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

                // Check ParkingArea
                var existingParkingArea = await _parkingAreaRepository.GetParkingAreaByIdAsync(req.ParkingAreaId);
                if (existingParkingArea.Data == null || !existingParkingArea.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_FOUND_OBJECT };
                }

                if (req.Name?.Trim() is not null)
                {
                    var isNameParkingExist = await _parkingAreaRepository.GetParkingAreaByNameAsync(req.Name);
                    if (isNameParkingExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    {
                        return new Return<dynamic> { Message = ErrorEnumApplication.OBJECT_EXISTED };
                    }
                    existingParkingArea.Data.Name = req.Name;
                }
                
                if (!string.IsNullOrEmpty(req.Description))
                {
                    existingParkingArea.Data.Description = req.Description;
                }
                if (req.MaxCapacity.HasValue)
                {
                    existingParkingArea.Data.MaxCapacity = req.MaxCapacity.Value;
                }
                if (req.Block.HasValue)
                {
                    existingParkingArea.Data.Block = req.Block.Value;
                }
                if (!string.IsNullOrEmpty(req.Mode))
                {
                    existingParkingArea.Data.Mode = req.Mode;
                }

                var updateResult = await _parkingAreaRepository.UpdateParkingAreaAsync(existingParkingArea.Data);
                if (!updateResult.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                return new Return<dynamic> { Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY, IsSuccess = true };
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
