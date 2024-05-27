using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class GateService : IGateService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IHelpperService _helpperService;
        private readonly IGateRepository _gateRepository;

        public GateService(ICardRepository cardRepository, IUserRepository userRepository, IRoleRepository roleRepository, IHelpperService helpperService, IGateRepository gateRepository)
        {
            _cardRepository = cardRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _helpperService = helpperService;
            _gateRepository = gateRepository;
        }

        public async Task<Return<IEnumerable<Gate>>> GetAllGate()
        {
            try
            {
                // Check bearerToken is valid or not
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<IEnumerable<Gate>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<IEnumerable<Gate>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }                
                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<Gate>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                return new Return<IEnumerable<Gate>>
                {
                    Data = (await _gateRepository.GetAllGateAsync()).Data,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
                
            } catch (Exception ex)
            {
                return new Return<IEnumerable<Gate>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = ex.Message
                };
            }
        }

        public async Task<Return<bool>> UpdateGateAsync(UpdateGateReqDto req, Guid id)
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
                var gateList = await _gateRepository.GetAllGateAsync();
                if (gateList == null || !gateList.IsSuccess || gateList.Data == null)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR
                    };
                }

                var existingGate = gateList.Data.FirstOrDefault(x => x.Id == id);
                if (existingGate == null)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR
                    };
                }

                // Check for duplicate parking area name (excluding the current parking area)
                var isDuplicateName = gateList.Data.Any(x => x.Name.Equals(req.Name, StringComparison.OrdinalIgnoreCase) && x.Id != id);
                if (isDuplicateName)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR
                    };
                }

                existingGate.Name = req.Name;
                existingGate.Description = req.Description;
                existingGate.GateTypeId = req.GateTypeId;
                existingGate.ParkingAreaId = req.ParkingAreaId;

                var updateResult = await _gateRepository.UpdateGateAsync(existingGate);
                return new Return<bool>
                {
                    IsSuccess = updateResult.IsSuccess,
                    Data = updateResult.IsSuccess,
                    Message = updateResult.IsSuccess ? SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY : ErrorEnumApplication.UPDATE_OBJECT_ERROR
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
    }
}
