using FUParkingModel.Enum;
using FUParkingModel.RequestObject.Session;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IHelpperService _helpperService;
        private readonly IUserRepository _userRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IGateRepository _gateRepository;

        public SessionService(ISessionRepository sessionRepository, IHelpperService helpperService, IUserRepository userRepository, ICardRepository cardRepository, IGateRepository gateRepository)
        {
            _sessionRepository = sessionRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
            _cardRepository = cardRepository;
            _gateRepository = gateRepository;
        }

        public async Task<Return<bool>> CheckInAsync(CreateSessionReqDto req)
        {
            try
            {
                //// Check token
                //if (!_helpperService.IsTokenValid())
                //    return new Return<bool> { Message = ErrorEnumApplication.NOT_AUTHORITY, IsSuccess = false };

                //// Check role
                //var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                //if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                //    return new Return<bool> { Message = ErrorEnumApplication.NOT_AUTHORITY, IsSuccess = false };

                //// Check CardId
                //var card = await _cardRepository.GetCardByIdAsync(req.CardId);
                //if (card.IsSuccess == false || card.Data == null)
                //    return new Return<bool> { Message = ErrorEnumApplication.CARD_NOT_EXIST, IsSuccess = false };

                //// Check CardId is in use
                //var session = await _sessionRepository.GetSessionByCardIdAsync(req.CardId);
                //if (session.IsSuccess == true && session.Data != null)
                //    return new Return<bool> { Message = ErrorEnumApplication.CARD_IN_USE, IsSuccess = false };

                //// Check GateInId
                //var gateIn = await _gateRepository.GetGateByIdAsync(req.GateInId);
                //if (gateIn.IsSuccess == false || gateIn.Data == null)
                //    return new Return<bool> { Message = ErrorEnumApplication.GATE_NOT_EXIST, IsSuccess = false };

                throw new NotImplementedException();
            } catch (Exception ex)
            {
                return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, IsSuccess = false };
            }
        }
    }
}
