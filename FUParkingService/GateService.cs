using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
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
    }
}
