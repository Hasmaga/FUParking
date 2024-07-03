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
        private readonly ICustomerRepository _customerRepository;

        public SessionService(ISessionRepository sessionRepository, IHelpperService helpperService, IUserRepository userRepository, ICardRepository cardRepository, IGateRepository gateRepository, IMinioService minioService, IParkingAreaRepository parkingAreaRepository, ICustomerRepository customerRepository)
        {
            _sessionRepository = sessionRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
            _cardRepository = cardRepository;
            _gateRepository = gateRepository;
            _minioService = minioService;
            _parkingAreaRepository = parkingAreaRepository;
            _customerRepository = customerRepository;
        }

        public async Task<Return<bool>> CheckInAsync(CreateSessionReqDto req)
        {
            try
            {
                // Check token
                if (!_helpperService.IsTokenValid())
                    return new Return<bool> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                // Check role
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!accountLogin.IsSuccess) 
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = accountLogin.InternalErrorMessage };
                if (!accountLogin.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || accountLogin.Data == null)
                    return new Return<bool> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                // Check CardId
                var card = await _cardRepository.GetCardByIdAsync(req.CardId);
                if (!card.IsSuccess) 
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = card.InternalErrorMessage };
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                    return new Return<bool> { Message = ErrorEnumApplication.CARD_NOT_EXIST };

                // Check newest session of this card, check this session is closed
                var isSessionClosed = await _sessionRepository.GetNewestSessionByCardIdAsync(req.CardId);
                if (!isSessionClosed.IsSuccess)
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = isSessionClosed.InternalErrorMessage };
                if (isSessionClosed.Data != null && isSessionClosed.Data.GateOutId == null)
                {
                    // Close this session
                    isSessionClosed.Data.Status = SessionEnum.CANCELLED;
                    isSessionClosed.Data.LastModifyById = accountLogin.Data.Id;
                    isSessionClosed.Data.LastModifyDate = DateTime.Now;
                }
                // Check this plate number is in another session
                var sessionPlate = await _sessionRepository.GetNewestSessionByPlateNumberAsync(req.PlateNumber);
                if (!sessionPlate.IsSuccess)
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = sessionPlate.InternalErrorMessage };
                if (sessionPlate.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) && sessionPlate.Data != null && sessionPlate.Data.Status.Equals(SessionEnum.PARKED))
                    return new Return<bool> { Message = ErrorEnumApplication.PLATE_NUMBER_IN_USE };

                // Check GateInId
                var gateIn = await _gateRepository.GetGateByIdAsync(req.GateInId);
                if (!gateIn.IsSuccess)
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = gateIn.InternalErrorMessage };
                if (gateIn.Data == null || !gateIn.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<bool> { Message = ErrorEnumApplication.GATE_NOT_EXIST };

                // Check parking area
                var parkingArea = await _parkingAreaRepository.GetParkingAreaByGateIdAsync(req.GateInId);
                if (!parkingArea.IsSuccess)
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = parkingArea.InternalErrorMessage };
                if (parkingArea.Data == null || !parkingArea.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<bool> { Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST };

                // Check plateNumber is belong to any customer
                var customer = await _customerRepository.GetCustomerByPlateNumberAsync(req.PlateNumber);
                if (!customer.IsSuccess)
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customer.InternalErrorMessage };

                // Object name = PlateNumber + TimeIn + extension file
                var objName = "https://miniofile.khangbpa.com/" + BucketMinioEnum.BUCKET_PARKiNG + "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(req.ImageIn.FileName);

                // Create new UploadObjectReqDto                
                UploadObjectReqDto uploadObjectReqDto = new()
                {
                    BucketName = BucketMinioEnum.BUCKET_PARKiNG,
                    ObjFile = req.ImageIn,
                    ObjName = objName
                };

                // Upload image to Minio server and get url image
                var imageInUrl = await _minioService.UploadObjectAsync(uploadObjectReqDto);
                if (imageInUrl.IsSuccess == false || imageInUrl.Data == null)
                    return new Return<bool> { Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED };

                // Create session
                var newSession = new Session
                {
                    CardId = req.CardId,
                    PlateNumber = req.PlateNumber,
                    GateInId = req.GateInId,
                    ImageInUrl = imageInUrl.Data.ObjUrl,
                    TimeIn = DateTime.Now,
                    Mode = parkingArea.Data.Mode,
                    Status = SessionEnum.PARKED,
                    CreatedById = accountLogin.Data.Id,
                    CustomerId = customer.Data?.Id ?? null,                    
                };
                // Create session
                var newsession = await _sessionRepository.CreateSessionAsync(newSession);
                if (!newsession.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR };
                return new Return<bool> { IsSuccess = true, Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY };                
            }
            catch (Exception ex)
            {
                return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        //public async Task<Return<bool>> CheckOutAsync()
    }
}
