using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.Reflection;

namespace FUParkingService
{
    public class GateService : IGateService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHelpperService _helpperService;
        private readonly IGateRepository _gateRepository;
        private readonly IParkingAreaRepository _parkingAreaRepository;

        public GateService(IUserRepository userRepository, IHelpperService helpperService, IGateRepository gateRepository, IParkingAreaRepository parkingAreaRepository)
        {
            _userRepository = userRepository;
            _helpperService = helpperService;
            _gateRepository = gateRepository;
            _parkingAreaRepository = parkingAreaRepository;
        }

        public async Task<Return<IEnumerable<Gate>>> GetAllGateAsync()
        {
            try
            {                
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<IEnumerable<Gate>>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || userlogged.IsSuccess == false || userlogged.Data == null)
                {
                    return new Return<IEnumerable<Gate>>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<Gate>>
                    {                        
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                return new Return<IEnumerable<Gate>>
                {
                    Data = (await _gateRepository.GetAllGateAsync()).Data,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<Gate>>
                {                    
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<dynamic>> CreateGateAsync(CreateGateReqDto req)
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
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        InternalErrorMessage = userlogged.InternalErrorMessage
                    };
                }

                if (!Auth.AuthSupervisor.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check gate name is existed
                var isGateNameExisted = await _gateRepository.GetGateByNameAsync(req.Name);
                if (!isGateNameExisted.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.OBJECT_EXISTED,
                        InternalErrorMessage = isGateNameExisted.InternalErrorMessage
                    };
                }

                var isParkingAreaExist = await _parkingAreaRepository.GetParkingAreaByIdAsync(req.ParkingAreaId);
                if (!isParkingAreaExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || isParkingAreaExist.Data == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST,
                        InternalErrorMessage = isParkingAreaExist.InternalErrorMessage
                    };
                }

                var gate = new Gate
                {
                    Name = req.Name,
                    StatusGate = StatusParkingEnum.ACTIVE,
                    CreatedById = userlogged.Data.Id,
                    Description = req.Description,
                    GateTypeId = req.GateTypeId,
                    ParkingAreaId = isParkingAreaExist.Data.Id
                };

                var result = await _gateRepository.CreateGateAsync(gate);

                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                }
                return new Return<dynamic> { Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, IsSuccess = true };
            }
            catch(Exception ex) 
            {
                return new Return<dynamic>
                {                    
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> UpdateGateAsync(UpdateGateReqDto req, Guid id)
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
                    return new Return<dynamic> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                
                // Check gate is exist
                var existingGate = await _gateRepository.GetGateByIdAsync(id);
                if (!existingGate.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT)|| existingGate.Data == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.GATE_NOT_EXIST
                    };
                }
                if (req.Name?.Trim() is not null)
                {
                    // Check name gate is existed
                    var isGateNameExisted = await _gateRepository.GetGateByNameAsync(req.Name);
                    if (!isGateNameExisted.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.OBJECT_EXISTED
                        };
                    }
                    existingGate.Data.Name = req.Name;
                } 
                if (req.Description?.Trim() is not null)
                {
                    existingGate.Data.Description = req.Description;
                }
                if (req.GateTypeId is not null)
                {
                    // Check gate type is exist
                    var isGateTypeExist = await _gateRepository.GetGateTypeByIdAsync(req.GateTypeId ?? new Guid());
                    if (!isGateTypeExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || isGateTypeExist.Data == null)
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.GATE_TYPE_NOT_EXIST
                        };
                    }
                    existingGate.Data.GateTypeId = isGateTypeExist.Data.Id;
                }
                if (req.ParkingAreaId is not null)
                {
                    // Check parking area is exist
                    var isParkingAreaExist = await _parkingAreaRepository.GetParkingAreaByIdAsync(req.ParkingAreaId ?? new Guid());
                    if (!isParkingAreaExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || isParkingAreaExist.Data == null)
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST
                        };
                    }
                    existingGate.Data.ParkingAreaId = isParkingAreaExist.Data.Id;
                }
                var updateResult = await _gateRepository.UpdateGateAsync(existingGate.Data);
                if (!updateResult.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
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

        public async Task<Return<dynamic>> DeleteGate(Guid id)
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
                    return new Return<dynamic> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                // Check if gateId exists
                var existedGate = await _gateRepository.GetGateByIdAsync(id);
                if (existedGate.Data == null || !existedGate.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {                        
                        Message = ErrorEnumApplication.GATE_NOT_EXIST
                    };
                }                
                existedGate.Data.DeletedDate = DateTime.Now;
                var result = await _gateRepository.UpdateGateAsync(existedGate.Data);
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
    }
}
