using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Session;
using FUParkingModel.ResponseObject.Session;
using FUParkingModel.ResponseObject.SessionCheckOut;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;

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
        private readonly IWalletRepository _walletRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPriceRepository _priceRepository;
        private readonly IVehicleRepository _vehicleRepository;

        public SessionService(ISessionRepository sessionRepository, IHelpperService helpperService, IUserRepository userRepository, ICardRepository cardRepository, IGateRepository gateRepository, IMinioService minioService, IParkingAreaRepository parkingAreaRepository, ICustomerRepository customerRepository, IWalletRepository walletRepository, IPaymentRepository paymentRepository, ITransactionRepository transactionRepository, IPriceRepository priceRepository, IVehicleRepository vehicleRepository)
        {
            _sessionRepository = sessionRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
            _cardRepository = cardRepository;
            _gateRepository = gateRepository;
            _minioService = minioService;
            _parkingAreaRepository = parkingAreaRepository;
            _customerRepository = customerRepository;
            _walletRepository = walletRepository;
            _paymentRepository = paymentRepository;
            _transactionRepository = transactionRepository;
            _priceRepository = priceRepository;
            _vehicleRepository = vehicleRepository;
        }

        public async Task<Return<dynamic>> CheckInAsync(CreateSessionReqDto req)
        {
            try
            {
                // Check token
                if (!_helpperService.IsTokenValid())
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                // Check role
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!accountLogin.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = accountLogin.InternalErrorMessage };
                if (!accountLogin.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || accountLogin.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                // Check CardId
                var card = await _cardRepository.GetCardByIdAsync(req.CardId);
                if (!card.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = card.InternalErrorMessage };
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.CARD_NOT_EXIST };
                // Check newest session of this card, check this session is closed
                var isSessionClosed = await _sessionRepository.GetNewestSessionByCardIdAsync(req.CardId);
                if (!isSessionClosed.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = isSessionClosed.InternalErrorMessage };
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
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = sessionPlate.InternalErrorMessage };
                if (sessionPlate.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) && sessionPlate.Data != null && sessionPlate.Data.Status.Equals(SessionEnum.PARKED))
                    return new Return<dynamic> { Message = ErrorEnumApplication.PLATE_NUMBER_IN_USE };
                // Check GateInId
                var gateIn = await _gateRepository.GetGateByIdAsync(req.GateInId);
                if (!gateIn.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = gateIn.InternalErrorMessage };
                if (gateIn.Data == null || !gateIn.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.GATE_NOT_EXIST };
                // Check parking area
                var parkingArea = await _parkingAreaRepository.GetParkingAreaByGateIdAsync(req.GateInId);
                if (!parkingArea.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = parkingArea.InternalErrorMessage };
                if (parkingArea.Data == null || !parkingArea.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST };
                // Check plateNumber is belong to any customer
                var customer = await _customerRepository.GetCustomerByPlateNumberAsync(req.PlateNumber);
                if (!customer.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customer.InternalErrorMessage };
                if (customer.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customer.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.CUSTOMER_NOT_EXIST };
                // Check vehicle type of plate number
                var vehicle = await _vehicleRepository.GetVehicleByPlateNumberAsync(req.PlateNumber);
                if (!vehicle.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicle.InternalErrorMessage };
                if (vehicle.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || vehicle.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_NOT_EXIST };
                // Object name = PlateNumber + TimeIn + extension file
                var objName = "https://miniofile.khangbpa.com/" + BucketMinioEnum.BUCKET_PARKiNG + "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + new Guid() + "_In" + Path.GetExtension(req.ImageIn.FileName);
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
                    return new Return<dynamic> { Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED };
                // Create session
                var newSession = new Session
                {
                    CardId = req.CardId,
                    Block = parkingArea.Data.Block,
                    PlateNumber = req.PlateNumber,
                    GateInId = req.GateInId,
                    ImageInUrl = imageInUrl.Data.ObjUrl,
                    TimeIn = DateTime.Now,
                    Mode = parkingArea.Data.Mode,
                    Status = SessionEnum.PARKED,
                    CreatedById = accountLogin.Data.Id,
                    CustomerId = customer.Data.Id,
                    VehicleTypeId = vehicle.Data.VehicleTypeId,
                };
                // Create session
                var newsession = await _sessionRepository.CreateSessionAsync(newSession);
                if (!newsession.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY };
            }
            catch (Exception ex)
            {
                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<bool>> CheckInForGuestAsync(string PlateNumber, Guid CardId, Guid GateInId, IFormFile ImageIn, Guid VehicleType)
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
                var card = await _cardRepository.GetCardByIdAsync(CardId);
                if (!card.IsSuccess)
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = card.InternalErrorMessage };
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                    return new Return<bool> { Message = ErrorEnumApplication.CARD_NOT_EXIST };
                // Check newest session of this card, check this session is closed
                var isSessionClosed = await _sessionRepository.GetNewestSessionByCardIdAsync(CardId);
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
                var sessionPlate = await _sessionRepository.GetNewestSessionByPlateNumberAsync(PlateNumber);
                if (!sessionPlate.IsSuccess)
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = sessionPlate.InternalErrorMessage };
                if (sessionPlate.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) && sessionPlate.Data != null && sessionPlate.Data.Status.Equals(SessionEnum.PARKED))
                    return new Return<bool> { Message = ErrorEnumApplication.PLATE_NUMBER_IN_USE };
                // Check GateInId
                var gateIn = await _gateRepository.GetGateByIdAsync(GateInId);
                if (!gateIn.IsSuccess)
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = gateIn.InternalErrorMessage };
                if (gateIn.Data == null || !gateIn.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<bool> { Message = ErrorEnumApplication.GATE_NOT_EXIST };
                // Check parking area
                var parkingArea = await _parkingAreaRepository.GetParkingAreaByGateIdAsync(GateInId);
                if (!parkingArea.IsSuccess)
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = parkingArea.InternalErrorMessage };
                if (parkingArea.Data == null || !parkingArea.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<bool> { Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST };
                // Create new UploadObjectReqDto
                var objName = "https://miniofile.khangbpa.com/" + BucketMinioEnum.BUCKET_PARKiNG + "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + new Guid() + "_In" + Path.GetExtension(ImageIn.FileName);
                UploadObjectReqDto uploadObjectReqDto = new()
                {
                    BucketName = BucketMinioEnum.BUCKET_PARKiNG,
                    ObjFile = ImageIn,
                    ObjName = objName
                };
                // Upload image to Minio server and get url image
                var imageInUrl = await _minioService.UploadObjectAsync(uploadObjectReqDto);
                if (imageInUrl.IsSuccess == false || imageInUrl.Data == null)
                    return new Return<bool> { Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED };
                // Create session
                var newSession = new Session
                {
                    CardId = CardId,
                    Block = parkingArea.Data.Block,
                    PlateNumber = PlateNumber,
                    GateInId = GateInId,
                    ImageInUrl = imageInUrl.Data.ObjUrl,
                    TimeIn = DateTime.Now,
                    Mode = parkingArea.Data.Mode,
                    Status = SessionEnum.PARKED,
                    CreatedById = accountLogin.Data.Id,
                    VehicleTypeId = VehicleType,
                };
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

        public async Task<Return<CheckOutResDto>> CheckOutAsync(string CardNumber, Guid GateOutId, DateTime TimeOut, IFormFile ImageOut)
        {
            try
            {
                if (!_helpperService.IsTokenValid())
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                // Check role
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!accountLogin.IsSuccess)
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = accountLogin.InternalErrorMessage };
                if (!accountLogin.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || accountLogin.Data == null)
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                // Check CardId
                var card = await _cardRepository.GetCardByCardNumberAsync(CardNumber);
                if (!card.IsSuccess)
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = card.InternalErrorMessage };
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.CARD_NOT_EXIST };
                var sessionCard = await _sessionRepository.GetNewestSessionByCardIdAsync(card.Data.Id);
                if (!sessionCard.IsSuccess)
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = sessionCard.InternalErrorMessage };
                if (sessionCard.Data == null || sessionCard.Data.GateOutId != null)
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SESSION_CLOSE };
                if (sessionCard.Data.Status.Equals(SessionEnum.CANCELLED))
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SESSION_CANCELLED };
                var gateOut = await _gateRepository.GetGateByIdAsync(GateOutId);
                if (!gateOut.IsSuccess)
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = gateOut.InternalErrorMessage };
                if (gateOut.Data == null || !gateOut.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.GATE_NOT_EXIST };

                if ((sessionCard.Data.Customer?.CustomerType ?? new CustomerType() { Description = "", Name = "" }).Name.Equals(CustomerTypeEnum.PAID) || sessionCard.Data.CustomerId == null)
                {
                    // Calculate total block time in minutes
                    // Check block of parking area                    
                    if (gateOut.Data.ParkingArea?.Block == null)
                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST };
                    var totalBlockTime = (int)(TimeOut - sessionCard.Data.TimeIn).TotalMinutes / sessionCard.Data.Block;
                    int price = 0;
                    switch (sessionCard.Data.Mode)
                    {
                        case ModeEnum.MODE1:
                            {
                                // Calculate price base on time in
                                // Check VehicleTypeId
                                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(sessionCard.Data.VehicleTypeId);
                                if (!vehicleType.IsSuccess)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                                if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                                if (!listPriceTable.IsSuccess)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                                if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                // Check which package is have higher piority
                                var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                                if (priceTable == null)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                // Get list price item in price table
                                var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                                if (!listPriceItem.IsSuccess)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                                if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                                var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= sessionCard.Data.TimeIn.Hour && x.ApplyToHour >= sessionCard.Data.TimeIn.Hour).FirstOrDefault();
                                if (priceItem == null)
                                {
                                    // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                    priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).LastOrDefault();
                                    if (priceItem == null)
                                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                }
                                // Calculate price
                                price = priceItem.MinPrice * totalBlockTime > priceItem.MaxPrice ? priceItem.MaxPrice : priceItem.MinPrice * totalBlockTime;
                                break;
                            }
                        case ModeEnum.MODE2:
                            {
                                // Calculate price base on time out
                                // Check VehicleTypeId
                                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(sessionCard.Data.VehicleTypeId);
                                if (!vehicleType.IsSuccess)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                                if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                                if (!listPriceTable.IsSuccess)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                                if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                // Check which package is have higher piority
                                var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                                if (priceTable == null)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                // Get list price item in price table
                                var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                                if (!listPriceItem.IsSuccess)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                                if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                                var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= sessionCard.Data.TimeIn.Hour && x.ApplyToHour >= sessionCard.Data.TimeIn.Hour).FirstOrDefault();
                                if (priceItem == null)
                                {
                                    // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                    priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).FirstOrDefault();
                                    if (priceItem == null)
                                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                }
                                // Calculate price
                                price = priceItem.MinPrice * totalBlockTime > priceItem.MaxPrice ? priceItem.MaxPrice : priceItem.MinPrice * totalBlockTime;
                                break;
                            }
                        case ModeEnum.MODE3:
                            {
                                // Calculate price base on time out
                                // Check VehicleTypeId
                                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(sessionCard.Data.VehicleTypeId);
                                if (!vehicleType.IsSuccess)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                                if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                                if (!listPriceTable.IsSuccess)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                                if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                // Check which package is have higher piority
                                var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                                if (priceTable == null)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                // Get list price item in price table
                                var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                                if (!listPriceItem.IsSuccess)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                                if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                                var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= sessionCard.Data.TimeIn.Hour && x.ApplyToHour >= sessionCard.Data.TimeIn.Hour).FirstOrDefault();
                                if (priceItem == null)
                                {
                                    // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                    priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).FirstOrDefault();
                                    if (priceItem == null)
                                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                }
                                // Calculate price
                                price = priceItem.MinPrice * totalBlockTime > priceItem.MaxPrice ? priceItem.MaxPrice : priceItem.MinPrice * totalBlockTime;
                                break;
                            }
                        case ModeEnum.MODE4:
                            {
                                // Calculate price base on time out
                                // Check VehicleTypeId
                                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(sessionCard.Data.VehicleTypeId);
                                if (!vehicleType.IsSuccess)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                                if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                                if (!listPriceTable.IsSuccess)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                                if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                // Check which package is have higher piority
                                var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                                if (priceTable == null)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                // Get list price item in price table
                                var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                                if (!listPriceItem.IsSuccess)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                                if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                                var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= sessionCard.Data.TimeIn.Hour && x.ApplyToHour >= sessionCard.Data.TimeIn.Hour).FirstOrDefault();
                                if (priceItem == null)
                                {
                                    // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                    priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).LastOrDefault();
                                    if (priceItem == null)
                                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                                }
                                // Calculate price
                                price = priceItem.MinPrice * totalBlockTime > priceItem.MaxPrice ? priceItem.MaxPrice : priceItem.MinPrice * totalBlockTime;
                                break;
                            }
                        default:
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                    }
                    // Upload image out
                    var objName = "https://miniofile.khangbpa.com/" + BucketMinioEnum.BUCKET_PARKiNG + "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + new Guid() + "_Out" + Path.GetExtension(ImageOut.FileName);
                    var uploadObjectReqDto = new UploadObjectReqDto
                    {
                        BucketName = BucketMinioEnum.BUCKET_PARKiNG,
                        ObjFile = ImageOut,
                        ObjName = objName
                    };
                    var imageOutUrl = await _minioService.UploadObjectAsync(uploadObjectReqDto);
                    if (imageOutUrl.IsSuccess == false || imageOutUrl.Data == null)
                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED };
                    // Try minus balance of customer wallet

                    if (sessionCard.Data.CustomerId.HasValue)
                    {
                        var walletMain = await _walletRepository.GetMainWalletByCustomerId(sessionCard.Data.CustomerId.Value);
                        if (!walletMain.IsSuccess)
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = walletMain.InternalErrorMessage };
                        if (walletMain.Data == null || !walletMain.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.WALLET_NOT_EXIST };
                        var walletExtra = await _walletRepository.GetExtraWalletByCustomerId(sessionCard.Data.CustomerId.Value);
                        if (!walletExtra.IsSuccess)
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = walletExtra.InternalErrorMessage };
                        if (walletExtra.Data == null || !walletExtra.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.WALLET_NOT_EXIST };                   // Minus balance of waller Extra first if balance of wallet Extra is enough to pay price of session then minus balance of walletExtra + walletMain if not enough then return error
                        if (walletExtra.Data.Balance >= price)
                        {
                            // Get PaymentMethodId
                            var paymentMethod = await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.WALLET);
                            if (!paymentMethod.IsSuccess || paymentMethod.Data == null || !paymentMethod.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = paymentMethod.InternalErrorMessage };
                            // Create Payment
                            var payment = new Payment
                            {
                                PaymentMethodId = paymentMethod.Data.Id,
                                SessionId = sessionCard.Data.Id,
                                TotalPrice = price,
                            };
                            var createPayment = await _paymentRepository.CreatePaymentAsync(payment);
                            if (!createPayment.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || createPayment.Data == null)
                                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Create transaction
                            var transaction = new Transaction
                            {
                                WalletId = walletExtra.Data.Id,
                                PaymentId = createPayment.Data.Id,
                                Amount = price,
                                TransactionDescription = "Pay for parking",
                                TransactionStatus = StatusTransactionEnum.SUCCEED,
                            };
                            var createTransaction = await _transactionRepository.CreateTransactionAsync(transaction);
                            if (!createTransaction.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || createTransaction.Data == null)
                                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            walletExtra.Data.Balance -= price;
                            var updateWalletExtra = await _walletRepository.UpdateWalletAsync(walletExtra.Data);
                            if (!updateWalletExtra.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        else if (walletExtra.Data.Balance + walletMain.Data.Balance >= price)
                        {
                            // Get PaymentMethodId
                            var paymentMethod = await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.WALLET);
                            if (!paymentMethod.IsSuccess || paymentMethod.Data == null || !paymentMethod.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = paymentMethod.InternalErrorMessage };
                            // Create 2 Payment for 2 wallet main and extra
                            var paymentMain = new Payment
                            {
                                PaymentMethodId = paymentMethod.Data.Id,
                                SessionId = sessionCard.Data.Id,
                                TotalPrice = price - walletExtra.Data.Balance,
                            };
                            var createPaymentMain = await _paymentRepository.CreatePaymentAsync(paymentMain);
                            if (!createPaymentMain.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || createPaymentMain.Data == null)
                                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var paymentExtra = new Payment
                            {
                                PaymentMethodId = paymentMethod.Data.Id,
                                SessionId = sessionCard.Data.Id,
                                TotalPrice = walletExtra.Data.Balance,
                            };
                            var createPaymentExtra = await _paymentRepository.CreatePaymentAsync(paymentExtra);
                            if (!createPaymentExtra.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || createPaymentExtra.Data == null)
                                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Create 2 transaction for 2 wallet main and extra
                            var transactionMain = new Transaction
                            {
                                WalletId = walletMain.Data.Id,
                                PaymentId = createPaymentMain.Data.Id,
                                Amount = price - walletExtra.Data.Balance,
                                TransactionDescription = "Pay for parking",
                                TransactionStatus = StatusTransactionEnum.SUCCEED,
                            };
                            var createTransactionMain = await _transactionRepository.CreateTransactionAsync(transactionMain);
                            if (!createTransactionMain.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || createTransactionMain.Data == null)
                                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var transactionExtra = new Transaction
                            {
                                WalletId = walletExtra.Data.Id,
                                PaymentId = createPaymentExtra.Data.Id,
                                Amount = walletExtra.Data.Balance,
                                TransactionDescription = "Pay for parking",
                                TransactionStatus = StatusTransactionEnum.SUCCEED,
                            };
                            var createTransactionExtra = await _transactionRepository.CreateTransactionAsync(transactionExtra);
                            if (!createTransactionExtra.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || createTransactionExtra.Data == null)
                                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            walletExtra.Data.Balance = 0;
                            walletMain.Data.Balance -= price - walletExtra.Data.Balance;
                            var updateWalletExtra = await _walletRepository.UpdateWalletAsync(walletExtra.Data);
                            if (!updateWalletExtra.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var updateWalletMain = await _walletRepository.UpdateWalletAsync(walletMain.Data);
                            if (!updateWalletMain.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        else
                        {
                            // Show money customer/guest need to pay
                            // Update session
                            sessionCard.Data.GateOutId = GateOutId;
                            sessionCard.Data.TimeOut = TimeOut;
                            sessionCard.Data.LastModifyById = accountLogin.Data.Id;
                            sessionCard.Data.LastModifyDate = DateTime.Now;
                            var isUpdateSession = await _sessionRepository.UpdateSessionAsync(sessionCard.Data);
                            if (!isUpdateSession.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            return new Return<CheckOutResDto>
                            {
                                IsSuccess = true,
                                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                                Data = new CheckOutResDto
                                {
                                    Amount = price,
                                    Message = "Need to pay",
                                    ImageIn = sessionCard.Data.ImageInUrl,
                                    PlateNumber = sessionCard.Data.PlateNumber,
                                }
                            };
                        }
                    }
                }
                sessionCard.Data.GateOutId = GateOutId;
                sessionCard.Data.TimeOut = TimeOut;
                sessionCard.Data.Status = SessionEnum.CLOSED;
                sessionCard.Data.LastModifyById = accountLogin.Data.Id;
                sessionCard.Data.LastModifyDate = DateTime.Now;
                var updateSession = await _sessionRepository.UpdateSessionAsync(sessionCard.Data);
                if (!updateSession.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                return new Return<CheckOutResDto>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                    Data = new CheckOutResDto
                    {
                        ImageIn = sessionCard.Data.ImageInUrl,
                        Message = "Check out successfully",
                        PlateNumber = sessionCard.Data.PlateNumber,
                    }
                };
            }
            catch (Exception ex)
            {
                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<dynamic>> UpdatePaymentSessionAsync(string CardNumber)
        {
            try
            {
                if (!_helpperService.IsTokenValid())
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                // Check role
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!accountLogin.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = accountLogin.InternalErrorMessage };
                if (!accountLogin.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || accountLogin.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                // Check CardId
                var card = await _cardRepository.GetCardByCardNumberAsync(CardNumber);
                if (!card.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = card.InternalErrorMessage };
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.CARD_NOT_EXIST };
                var sessionCard = await _sessionRepository.GetNewestSessionByCardIdAsync(card.Data.Id);
                if (!sessionCard.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = sessionCard.InternalErrorMessage };
                if (sessionCard.Data == null || sessionCard.Data.GateOutId != null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SESSION_CLOSE };
                if (sessionCard.Data.Status.Equals(SessionEnum.CANCELLED))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SESSION_CANCELLED };
                // Update session to close
                sessionCard.Data.Status = SessionEnum.CLOSED;
                sessionCard.Data.LastModifyById = accountLogin.Data.Id;
                sessionCard.Data.LastModifyDate = DateTime.Now;
                var updateSession = await _sessionRepository.UpdateSessionAsync(sessionCard.Data);
                if (!updateSession.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
            }
            catch (Exception ex)
            {
                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<IEnumerable<GetHistorySessionResDto>>> GetListSessionByCustomerAsync(GetListObjectWithFillerDateReqDto req)
        {
            try
            {
                if (!_helpperService.IsTokenValid())
                    return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                var customerLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!customerLogged.IsSuccess)
                    return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customerLogged.InternalErrorMessage };
                if (customerLogged.Data == null || !customerLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.NOT_AUTHORITY };                
                var listSession = await _sessionRepository.GetListSessionByCustomerIdAsync(customerLogged.Data.Id, req.StartDate, req.EndDate, req.PageSize, req.PageIndex);
                if (!listSession.IsSuccess)
                    return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listSession.InternalErrorMessage };
                if (listSession.Data == null || !listSession.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.NOT_FOUND_OBJECT, Data = [], IsSuccess = true };

                var listSessionData = new List<GetHistorySessionResDto>();
                foreach (var item in listSession.Data)
                {
                    string? GateOutName = null;
                    var amount = await _paymentRepository.GetPaymentBySessionIdAsync(item.Id);
                    if (!amount.IsSuccess)
                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = amount.InternalErrorMessage };
                    var GateIn = await _gateRepository.GetGateByIdAsync(item.GateInId);
                    if (!GateIn.IsSuccess)
                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = GateIn.InternalErrorMessage };
                    if (!GateIn.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || GateIn.Data == null)
                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                    if (item.GateOutId is not null)
                    {
                        var GateOut = await _gateRepository.GetGateByIdAsync(item.GateOutId.Value);
                        if (!GateOut.IsSuccess)
                            return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = GateOut.InternalErrorMessage };
                        if (!GateOut.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || GateOut.Data == null)
                            return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                        GateOutName = GateOut.Data.Name;
                    }
                    listSessionData.Add(new GetHistorySessionResDto
                    {
                        PlateNumber = item.PlateNumber,
                        TimeIn = item.TimeIn,
                        TimeOut = item.TimeOut,
                        Status = item.Status,
                        Amount = amount.Data?.TotalPrice,
                        GateIn = GateIn.Data.Name,
                        GateOut = GateOutName,
                    });
                }
                return new Return<IEnumerable<GetHistorySessionResDto>>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = listSessionData
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }
    }
}
