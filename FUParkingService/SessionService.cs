using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
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
        private readonly IMinioService _minioService;
        private readonly IParkingAreaRepository _parkingAreaRepository;

        public SessionService(ISessionRepository sessionRepository, IHelpperService helpperService, IUserRepository userRepository, ICardRepository cardRepository, IGateRepository gateRepository, IMinioService minioService, IParkingAreaRepository parkingAreaRepository)
        {
            _sessionRepository = sessionRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
            _cardRepository = cardRepository;
            _gateRepository = gateRepository;
            _minioService = minioService;
            _parkingAreaRepository = parkingAreaRepository;
        }

        public async Task<Return<bool>> CheckInAsync(CreateSessionReqDto req)
        {
            try
            {
                // Check token
                if (!_helpperService.IsTokenValid())
                    return new Return<bool> { Message = ErrorEnumApplication.NOT_AUTHORITY, IsSuccess = false };

                // Check role
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                    return new Return<bool> { Message = ErrorEnumApplication.NOT_AUTHORITY, IsSuccess = false };

                // Check CardId
                var card = await _cardRepository.GetCardByIdAsync(req.CardId);
                if (card.IsSuccess == false || card.Data == null)
                    return new Return<bool> { Message = ErrorEnumApplication.CARD_NOT_EXIST, IsSuccess = false };

                // Check CardId is in use
                var session = await _sessionRepository.GetSessionByCardIdAsync(req.CardId);
                if (session.IsSuccess == true && session.Data != null && session.Data.GateOutId == null)
                    return new Return<bool> { Message = ErrorEnumApplication.CARD_IN_USE, IsSuccess = false };

                // Check this plate number is in another session
                var sessionPlate = await _sessionRepository.GetSessionByPlateNumberAsync(req.PlateNumber);
                if (sessionPlate.IsSuccess == true && sessionPlate.Data != null && sessionPlate.Data.GateOutId == null)
                    return new Return<bool> { Message = ErrorEnumApplication.PLATE_NUMBER_IN_USE, IsSuccess = false };

                // Check GateInId
                var gateIn = await _gateRepository.GetGateByIdAsync(req.GateInId);
                if (gateIn.IsSuccess == false || gateIn.Data == null)
                    return new Return<bool> { Message = ErrorEnumApplication.GATE_NOT_EXIST, IsSuccess = false };

                var parkingArea = await _parkingAreaRepository.GetParkingAreaByGateIdAsync(req.GateInId);
                if (parkingArea.IsSuccess == false || parkingArea.Data == null)
                    return new Return<bool> { Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST, IsSuccess = false };

                // Object name = PlateNumber + TimeIn + extension file
                var objName = req.PlateNumber + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(req.ImageIn.FileName);

                // Create new UploadObjectReqDto                
                UploadObjectReqDto uploadObjectReqDto = new()
                {
                    BucketName = "parking",
                    ObjFile = req.ImageIn,
                    ObjName = objName
                };

                // Upload image to Minio server and get url image
                var imageInUrl = await _minioService.UploadObjectAsync(uploadObjectReqDto);
                if (imageInUrl.IsSuccess == false || imageInUrl.Data == null)
                    return new Return<bool> { Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED, IsSuccess = false };

                // Create session
                var newSession = new Session
                {
                    CardId = req.CardId,
                    PlateNumber = req.PlateNumber,
                    GateInId = req.GateInId,
                    ImageInUrl = imageInUrl.Data.ObjUrl,
                    TimeIn = DateTime.Now,
                    Mode = parkingArea.Data?.Mode ?? "",
                };

                // Create session
                var newsession = await _sessionRepository.CreateSessionAsync(newSession);
                if (newsession.IsSuccess == false || newsession.Data == null)
                    return new Return<bool> { Message = ErrorEnumApplication.ADD_OBJECT_ERROR, IsSuccess = false };

                return new Return<bool> { IsSuccess = true, Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY };
            }
            catch (Exception ex)
            {
                return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, IsSuccess = false, InternalErrorMessage = ex.Message };
            }
        }

        //public async Task<Return<bool>> CheckOutAsync()
    }
}
