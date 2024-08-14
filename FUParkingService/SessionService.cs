using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Session;
using FUParkingModel.ResponseObject.Session;
using FUParkingModel.ResponseObject.SessionCheckOut;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ResponseObject.Vehicle;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.Transactions;

namespace FUParkingService
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IHelpperService _helpperService;
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

        public SessionService(ISessionRepository sessionRepository, IHelpperService helpperService, ICardRepository cardRepository, IGateRepository gateRepository, IMinioService minioService, IParkingAreaRepository parkingAreaRepository, ICustomerRepository customerRepository, IWalletRepository walletRepository, IPaymentRepository paymentRepository, ITransactionRepository transactionRepository, IPriceRepository priceRepository, IVehicleRepository vehicleRepository)
        {
            _sessionRepository = sessionRepository;
            _helpperService = helpperService;
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
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var card = await _cardRepository.GetCardByCardNumberAsync(req.CardNumber);
                if (!card.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = card.InternalErrorMessage };
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.CARD_NOT_EXIST };
                if (!card.Data.Status.Equals(CardStatusEnum.ACTIVE))
                    return new Return<dynamic> { Message = ErrorEnumApplication.CARD_IS_INACTIVE };
                // Check newest session of this card, check this session is closed
                var isSessionClosed = await _sessionRepository.GetNewestSessionByCardIdAsync(card.Data.Id);
                if (!isSessionClosed.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = isSessionClosed.InternalErrorMessage };
                if (isSessionClosed.Data != null && isSessionClosed.Data.GateOutId == null)
                {
                    // Close this session
                    isSessionClosed.Data.Status = SessionEnum.CANCELLED;
                    isSessionClosed.Data.LastModifyById = checkAuth.Data.Id;
                    isSessionClosed.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
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
                if (gateIn.Data.GateType == null || gateIn.Data.GateType.Name.Equals(GateTypeEnum.OUT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                // Check parking area
                var parkingArea = await _parkingAreaRepository.GetParkingAreaByGateIdAsync(req.GateInId);
                if (!parkingArea.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = parkingArea.InternalErrorMessage };
                if (parkingArea.Data == null || !parkingArea.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST };
                if (!parkingArea.Data.StatusParkingArea.Equals(StatusParkingEnum.ACTIVE))
                    return new Return<dynamic> { Message = ErrorEnumApplication.PARKING_AREA_INACTIVE };
                // Check plateNumber is belong to any customer
                var customer = await _customerRepository.GetCustomerByPlateNumberAsync(req.PlateNumber);
                if (!customer.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = customer.InternalErrorMessage };
                if (!customer.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customer.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.CUSTOMER_NOT_EXIST };
                // Check vehicle type of plate number
                var vehicle = await _vehicleRepository.GetVehicleByPlateNumberAsync(req.PlateNumber);
                if (!vehicle.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicle.InternalErrorMessage };
                if (!vehicle.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || vehicle.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_NOT_EXIST };
                if (vehicle.Data.StatusVehicle.Equals(StatusVehicleEnum.REJECTED))
                    return new Return<dynamic> { Message = ErrorEnumApplication.CUSTOMER_NOT_EXIST };
                if (vehicle.Data.StatusVehicle.Equals(StatusVehicleEnum.PENDING))
                    // show information vehicle 
                    return new Return<dynamic>
                    {
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                        Data = new GetVehicleInformationByStaffResDto
                        {
                            CreateDate = vehicle.Data.CreatedDate,
                            PlateImage = vehicle.Data.PlateImage,
                            PlateNumber = vehicle.Data.PlateNumber,
                            StatusVehicle = vehicle.Data.StatusVehicle,
                            VehicleType = vehicle.Data.VehicleTypeId,
                        }
                    };
                // Object name = PlateNumber + TimeIn + extension file
                var objName = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_In" + Path.GetExtension(req.ImageIn.FileName);
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
                    CardId = card.Data.Id,
                    Block = parkingArea.Data.Block,
                    PlateNumber = req.PlateNumber,
                    GateInId = req.GateInId,
                    ImageInUrl = imageInUrl.Data.ObjUrl,
                    TimeIn = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                    Mode = parkingArea.Data.Mode,
                    Status = SessionEnum.PARKED,
                    CreatedById = checkAuth.Data.Id,
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

        public async Task<Return<bool>> CheckInForGuestAsync(CheckInForGuestReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check CardId
                var card = await _cardRepository.GetCardByCardNumberAsync(req.CardNumber);
                if (!card.IsSuccess)
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = card.InternalErrorMessage };
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                    return new Return<bool> { Message = ErrorEnumApplication.CARD_NOT_EXIST };
                // Check newest session of this card, check this session is closed
                var isSessionClosed = await _sessionRepository.GetNewestSessionByCardIdAsync(card.Data.Id);
                if (!isSessionClosed.IsSuccess)
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = isSessionClosed.InternalErrorMessage };
                if (isSessionClosed.Data != null && isSessionClosed.Data.GateOutId == null)
                {
                    // Close this session
                    isSessionClosed.Data.Status = SessionEnum.CANCELLED;
                    isSessionClosed.Data.LastModifyById = checkAuth.Data.Id;
                    isSessionClosed.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
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
                // check vehicle type
                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(req.VehicleTypeId);
                if (!vehicleType.IsSuccess)
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<bool> { Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST };
                // Check parking area
                var parkingArea = await _parkingAreaRepository.GetParkingAreaByGateIdAsync(req.GateInId);
                if (!parkingArea.IsSuccess)
                    return new Return<bool> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = parkingArea.InternalErrorMessage };
                if (parkingArea.Data == null || !parkingArea.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<bool> { Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST };
                // Create new UploadObjectReqDto
                var objName = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_In" + Path.GetExtension(req.ImageIn.FileName);
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
                    CardId = card.Data.Id,
                    Block = parkingArea.Data.Block,
                    PlateNumber = req.PlateNumber,
                    GateInId = gateIn.Data.Id,
                    ImageInUrl = imageInUrl.Data.ObjUrl,
                    TimeIn = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                    Mode = parkingArea.Data.Mode,
                    Status = SessionEnum.PARKED,
                    CreatedById = checkAuth.Data.Id,
                    VehicleTypeId = vehicleType.Data.Id,
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

        public async Task<Return<CheckOutResDto>> CheckOutAsync(CheckOutAsyncReqDto req)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<CheckOutResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check CardId
                var card = await _cardRepository.GetCardByCardNumberAsync(req.CardNumber);
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
                // Check plate number
                if (sessionCard.Data.PlateNumber != req.PlateNumber)
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.PLATE_NUMBER_NOT_MATCH };
                var gateOut = await _gateRepository.GetGateByIdAsync(req.GateOutId);
                if (!gateOut.IsSuccess)
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = gateOut.InternalErrorMessage };
                if (gateOut.Data == null || !gateOut.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.GATE_NOT_EXIST };
                if ((sessionCard.Data.Customer?.CustomerType ?? new CustomerType() { Description = "", Name = "" }).Name.Equals(CustomerTypeEnum.FREE))
                {
                    var objNameNonePaid = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_Out" + Path.GetExtension(req.ImageOut.FileName);
                    var uploadObjectNonePaidReqDto = new UploadObjectReqDto
                    {
                        BucketName = BucketMinioEnum.BUCKET_PARKiNG,
                        ObjFile = req.ImageOut,
                        ObjName = objNameNonePaid
                    };
                    var imageOutNonePaidUrl = await _minioService.UploadObjectAsync(uploadObjectNonePaidReqDto);
                    if (imageOutNonePaidUrl.IsSuccess == false || imageOutNonePaidUrl.Data == null)
                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED };
                    sessionCard.Data.GateOutId = gateOut.Data.Id;
                    sessionCard.Data.ImageOutUrl = imageOutNonePaidUrl.Data.ObjUrl;
                    sessionCard.Data.TimeOut = req.TimeOut;
                    sessionCard.Data.LastModifyById = checkAuth.Data.Id;
                    sessionCard.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                    sessionCard.Data.Status = SessionEnum.CLOSED;
                    var updateNonePaidSession = await _sessionRepository.UpdateSessionAsync(sessionCard.Data);
                    if (!updateNonePaidSession.IsSuccess)
                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = updateNonePaidSession.InternalErrorMessage };
                    scope.Complete();
                    return new Return<CheckOutResDto> 
                    { 
                        IsSuccess = true, 
                        Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                        Data = new CheckOutResDto
                        {
                            TimeIn = sessionCard.Data.TimeIn,
                            Amount = 0,
                            ImageIn = sessionCard.Data.ImageInUrl,
                            Message = "Check out successfully",
                            PlateNumber = sessionCard.Data.PlateNumber,
                            TypeOfCustomer = sessionCard.Data.Customer?.CustomerType?.Name ?? "",                            
                        }
                    };
                }
                // Calculate total block time in minutes
                // Check block of parking area                    
                if (sessionCard.Data.GateIn?.ParkingArea?.Block == null)
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST };
                var totalBlockTime = (int)(req.TimeOut - sessionCard.Data.TimeIn).TotalMinutes / sessionCard.Data.Block;
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
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
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
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
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
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
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
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    default:
                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                // Upload image out
                var objName = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_Out" + Path.GetExtension(req.ImageOut.FileName);
                var uploadObjectReqDto = new UploadObjectReqDto
                {
                    BucketName = BucketMinioEnum.BUCKET_PARKiNG,
                    ObjFile = req.ImageOut,
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
                        {
                            scope.Dispose();
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        // Create transaction
                        var transaction = new FUParkingModel.Object.Transaction
                        {
                            WalletId = walletExtra.Data.Id,
                            PaymentId = createPayment.Data.Id,
                            Amount = price,
                            TransactionDescription = "Pay for parking",
                            TransactionStatus = StatusTransactionEnum.SUCCEED,
                        };
                        var createTransaction = await _transactionRepository.CreateTransactionAsync(transaction);
                        if (!createTransaction.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || createTransaction.Data == null)
                        {
                            scope.Dispose();
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        walletExtra.Data.Balance -= price;
                        var updateWalletExtra = await _walletRepository.UpdateWalletAsync(walletExtra.Data);
                        if (!updateWalletExtra.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            scope.Dispose();
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
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
                        {
                            scope.Dispose();
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        var paymentExtra = new Payment
                        {
                            PaymentMethodId = paymentMethod.Data.Id,
                            SessionId = sessionCard.Data.Id,
                            TotalPrice = walletExtra.Data.Balance,
                        };
                        var createPaymentExtra = await _paymentRepository.CreatePaymentAsync(paymentExtra);
                        if (!createPaymentExtra.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || createPaymentExtra.Data == null)
                        {
                            scope.Dispose();
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        // Create 2 transaction for 2 wallet main and extra
                        var transactionMain = new FUParkingModel.Object.Transaction
                        {
                            WalletId = walletMain.Data.Id,
                            PaymentId = createPaymentMain.Data.Id,
                            Amount = price - walletExtra.Data.Balance,
                            TransactionDescription = "Pay for parking",
                            TransactionStatus = StatusTransactionEnum.SUCCEED,
                        };
                        var createTransactionMain = await _transactionRepository.CreateTransactionAsync(transactionMain);
                        if (!createTransactionMain.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || createTransactionMain.Data == null)
                        {
                            scope.Dispose();
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        var transactionExtra = new FUParkingModel.Object.Transaction
                        {
                            WalletId = walletExtra.Data.Id,
                            PaymentId = createPaymentExtra.Data.Id,
                            Amount = walletExtra.Data.Balance,
                            TransactionDescription = "Pay for parking",
                            TransactionStatus = StatusTransactionEnum.SUCCEED,
                        };
                        var createTransactionExtra = await _transactionRepository.CreateTransactionAsync(transactionExtra);
                        if (!createTransactionExtra.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || createTransactionExtra.Data == null)
                        {
                            scope.Dispose();
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        walletExtra.Data.Balance = 0;
                        walletMain.Data.Balance -= price - walletExtra.Data.Balance;
                        var updateWalletExtra = await _walletRepository.UpdateWalletAsync(walletExtra.Data);
                        if (!updateWalletExtra.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            scope.Dispose();
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        var updateWalletMain = await _walletRepository.UpdateWalletAsync(walletMain.Data);
                        if (!updateWalletMain.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            scope.Dispose();
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                    }
                    else
                    {
                        // Show money customer/guest need to pay
                        // Update session
                        // Get payment method id is Cash
                        var paymentMethod = await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.CASH);
                        if (!paymentMethod.IsSuccess || paymentMethod.Data == null || !paymentMethod.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                        {
                            scope.Dispose();
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = paymentMethod.InternalErrorMessage };
                        }
                        sessionCard.Data.ImageOutUrl = imageOutUrl.Data.ObjUrl;
                        sessionCard.Data.GateOutId = req.GateOutId;
                        sessionCard.Data.TimeOut = req.TimeOut;
                        sessionCard.Data.LastModifyById = checkAuth.Data.Id;
                        sessionCard.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                        sessionCard.Data.PaymentMethodId = paymentMethod.Data.Id;
                        var isUpdateSession = await _sessionRepository.UpdateSessionAsync(sessionCard.Data);
                        if (!isUpdateSession.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            scope.Dispose();
                            return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        scope.Complete();
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
                                TimeIn = sessionCard.Data.TimeIn,
                            }
                        };
                    }
                    // GetPaymentMethod wallet
                    var paymentMethodWallet = await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.WALLET);
                    if (!paymentMethodWallet.IsSuccess || paymentMethodWallet.Data == null || !paymentMethodWallet.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    {
                        scope.Dispose();
                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = paymentMethodWallet.InternalErrorMessage };
                    }
                    sessionCard.Data.ImageOutUrl = imageOutUrl.Data.ObjUrl;
                    sessionCard.Data.GateOutId = req.GateOutId;
                    sessionCard.Data.TimeOut = req.TimeOut;
                    sessionCard.Data.Status = SessionEnum.CLOSED;
                    sessionCard.Data.LastModifyById = checkAuth.Data.Id;
                    sessionCard.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                    sessionCard.Data.PaymentMethodId = paymentMethodWallet.Data.Id;
                    var updateSession = await _sessionRepository.UpdateSessionAsync(sessionCard.Data);
                    if (!updateSession.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    {
                        scope.Dispose();
                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                    }
                    scope.Complete();
                    return new Return<CheckOutResDto>
                    {
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                        Data = new CheckOutResDto
                        {
                            ImageIn = sessionCard.Data.ImageInUrl,
                            Message = "Check out successfully",
                            PlateNumber = sessionCard.Data.PlateNumber,
                            Amount = price,
                            TimeIn = sessionCard.Data.TimeIn,
                            TypeOfCustomer = sessionCard.Data.Customer?.CustomerType?.Name ?? "",
                        }
                    };
                }
                // For guest 
                else
                {
                    var paymentMethod = await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.CASH);
                    if (!paymentMethod.IsSuccess || paymentMethod.Data == null || !paymentMethod.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    {
                        scope.Dispose();
                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = paymentMethod.InternalErrorMessage };
                    }
                    sessionCard.Data.ImageOutUrl = imageOutUrl.Data.ObjUrl;
                    sessionCard.Data.GateOutId = req.GateOutId;
                    sessionCard.Data.TimeOut = req.TimeOut;
                    sessionCard.Data.LastModifyById = checkAuth.Data.Id;
                    sessionCard.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                    sessionCard.Data.PaymentMethodId = paymentMethod.Data.Id;
                    var isUpdateSession = await _sessionRepository.UpdateSessionAsync(sessionCard.Data);
                    if (!isUpdateSession.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    {
                        scope.Dispose();
                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                    }
                    scope.Complete();
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
                            TypeOfCustomer = CustomerTypeEnum.PAID,
                            TimeIn = sessionCard.Data.TimeIn,
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                scope.Dispose();
                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<dynamic>> UpdatePaymentSessionAsync(string CardNumber)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check CardId
                var card = await _cardRepository.GetCardByCardNumberAsync(CardNumber);
                if (!card.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = card.InternalErrorMessage };
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.CARD_NOT_EXIST };
                var sessionCard = await _sessionRepository.GetNewestSessionByCardIdAsync(card.Data.Id);
                if (!sessionCard.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = sessionCard.InternalErrorMessage };
                if (sessionCard.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                if (!sessionCard.Data.Status.Equals(SessionEnum.PARKED))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                // Update session to close
                sessionCard.Data.Status = SessionEnum.CLOSED;
                sessionCard.Data.LastModifyById = checkAuth.Data.Id;
                sessionCard.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
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
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetHistorySessionResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var listSession = await _sessionRepository.GetListSessionByCustomerIdAsync(checkAuth.Data.Id, req.StartDate, req.EndDate, req.PageSize, req.PageIndex);
                if (!listSession.IsSuccess)
                {
                    return new Return<IEnumerable<GetHistorySessionResDto>>
                    {
                        InternalErrorMessage = listSession.InternalErrorMessage,
                        Message = listSession.Message
                    };
                }
                var listSessionData = new List<GetHistorySessionResDto>();
                if (listSession.Data == null)
                {
                    return new Return<IEnumerable<GetHistorySessionResDto>>
                    {
                        IsSuccess = true,
                        Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                        Data = listSessionData,
                        TotalRecord = 0
                    };
                }                
                foreach (var item in listSession.Data)
                {                   
                    var payment = await _paymentRepository.GetPaymentBySessionIdAsync(item.Id);
                    if (!payment.IsSuccess)
                    {
                        return new Return<IEnumerable<GetHistorySessionResDto>>
                        {
                            InternalErrorMessage = payment.InternalErrorMessage,
                            Message = payment.Message
                        };
                    }
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    listSessionData.Add(new GetHistorySessionResDto
                    {
                        Id = item.Id,
                        Amount = payment.Data?.TotalPrice,
                        TimeIn = item.TimeIn,
                        TimeOut = item.TimeOut,
                        PlateNumber = item.PlateNumber,
                        Status = item.Status,
                        GateIn = item.GateIn?.Name ?? "",
                        GateOut = item.GateOut?.Name,
                        PaymentMethod = item.PaymentMethod?.Name,
                        ParkingArea = item.GateIn?.ParkingArea?.Name ?? "",
                        IsFeedback = item.Feedbacks.Count > 0
                    });
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
                return new Return<IEnumerable<GetHistorySessionResDto>>
                {
                    IsSuccess = true,
                    Message = listSession.TotalRecord > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = listSessionData,
                    TotalRecord = listSession.TotalRecord
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<IEnumerable<StatisticSessionAppResDto>>> StatisticSessionAppAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<StatisticSessionAppResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _sessionRepository.StatisticSessionAppAsync();
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<StatisticSessionAppResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<StatisticSessionAppResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<CheckOutResDto>> CheckOutSessionByPlateNumberAsync(string PlateNumber, DateTime timeOut)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    scope.Dispose();
                    return new Return<CheckOutResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check PlateNumber
                var session = await _sessionRepository.GetNewestSessionByPlateNumberAsync(PlateNumber);
                if (!session.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || session.Data is null) 
                {
                    scope.Dispose();
                    return new Return<CheckOutResDto>
                    {
                        InternalErrorMessage = session.InternalErrorMessage,
                        Message = ErrorEnumApplication.NOT_FOUND_SESSION_WITH_PLATE_NUMBER
                    };
                }                
                if (session.Data.Status.Equals(SessionEnum.CLOSED))
                {
                    scope.Dispose();
                    return new Return<CheckOutResDto>
                    {
                        Message = ErrorEnumApplication.SESSION_CLOSE
                    };
                }
                if (session.Data.Status.Equals(SessionEnum.CANCELLED))
                {
                    scope.Dispose();
                    return new Return<CheckOutResDto>
                    {
                        Message = ErrorEnumApplication.SESSION_CANCELLED
                    };
                }
                // Get virtual gate
                var gateOut = await _gateRepository.GetVirtualGateAsync();
                if (!gateOut.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || gateOut.Data == null)
                {
                    scope.Dispose();
                    return new Return<CheckOutResDto>
                    {
                        InternalErrorMessage = gateOut.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                var totalBlockTime = (int)(timeOut - session.Data.TimeIn).TotalMinutes / session.Data.Block;
                int price = 0;
                // check customer type is free
                if ((session.Data.Customer?.CustomerType ?? new CustomerType() { Description = "", Name = "" }).Name.Equals(CustomerTypeEnum.FREE))
                {
                    session.Data.GateOutId = gateOut.Data.Id;                    
                    session.Data.TimeOut = timeOut;
                    session.Data.LastModifyById = checkAuth.Data.Id;
                    session.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                    session.Data.Status = SessionEnum.CLOSED;
                    var updateNonePaidSession = await _sessionRepository.UpdateSessionAsync(session.Data);
                    if (!updateNonePaidSession.IsSuccess)
                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = updateNonePaidSession.InternalErrorMessage };
                    scope.Complete();
                    return new Return<CheckOutResDto>
                    {
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                        Data = new CheckOutResDto
                        {
                            TimeIn = session.Data.TimeIn,
                            Amount = 0,
                            ImageIn = session.Data.ImageInUrl,
                            Message = "Check out successfully",
                            PlateNumber = session.Data.PlateNumber,
                            TypeOfCustomer = session.Data.Customer?.CustomerType?.Name ?? "",
                        }
                    };
                }
                
                switch (session.Data.Mode)
                {
                    case ModeEnum.MODE1:
                        {
                            // Calculate price base on time in
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(session.Data.VehicleTypeId);
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
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= session.Data.TimeIn.Hour && x.ApplyToHour >= session.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).LastOrDefault();
                                if (priceItem == null)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE2:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(session.Data.VehicleTypeId);
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
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= session.Data.TimeIn.Hour && x.ApplyToHour >= session.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).FirstOrDefault();
                                if (priceItem == null)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE3:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(session.Data.VehicleTypeId);
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
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= session.Data.TimeIn.Hour && x.ApplyToHour >= session.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).FirstOrDefault();
                                if (priceItem == null)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE4:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(session.Data.VehicleTypeId);
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
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= session.Data.TimeIn.Hour && x.ApplyToHour >= session.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).LastOrDefault();
                                if (priceItem == null)
                                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    default:
                        return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                var paymentMethod = await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.CASH);
                if (!paymentMethod.IsSuccess || paymentMethod.Data == null || !paymentMethod.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    scope.Dispose();
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = paymentMethod.InternalErrorMessage };
                }
                session.Data.GateOutId = gateOut.Data.Id;
                session.Data.TimeOut = timeOut;
                session.Data.LastModifyById = checkAuth.Data.Id;
                session.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                session.Data.PaymentMethodId = paymentMethod.Data.Id;
                var isUpdateSession = await _sessionRepository.UpdateSessionAsync(session.Data);
                if (!isUpdateSession.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    scope.Dispose();
                    return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                scope.Complete();
                return new Return<CheckOutResDto>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                    Data = new CheckOutResDto
                    {
                        Amount = price,
                        Message = "Need to pay",
                        ImageIn = session.Data.ImageInUrl,
                        PlateNumber = session.Data.PlateNumber,
                        TypeOfCustomer = CustomerTypeEnum.PAID,
                        TimeIn = session.Data.TimeIn,
                    }
                };
            }
            catch (Exception ex)
            {
                return new Return<CheckOutResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<IEnumerable<GetSessionByUserResDto>>> GetListSessionByUserAsync(GetListObjectWithFillerAttributeAndDateReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetSessionByUserResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _sessionRepository.GetListSessionAsync(req);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetSessionByUserResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                return new Return<IEnumerable<GetSessionByUserResDto>>
                {
                    IsSuccess = true,
                    Message = result.TotalRecord > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = result.Data?.Select(x => new GetSessionByUserResDto
                    {
                        Id = x.Id,
                        Block = x.Block,
                        CardNumber = x.Card?.CardNumber ?? "",
                        Mode = x.Mode,
                        ImageOutUrl = x.ImageOutUrl ?? "",
                        ImageInUrl = x.ImageInUrl,
                        GateOutName = x.GateOut?.Name ?? "",
                        GateInName = x.GateIn?.Name ?? "",
                        PlateNumber = x.PlateNumber,
                        CheckInStaff = x.CreateBy?.Email ?? "",
                        CheckOutStaff = x.LastModifyBy?.Email ?? "",
                        TimeIn = x.TimeIn,
                        CustomerEmail = x.Customer?.Email ?? "",
                        ParkingArea = x.GateIn?.ParkingArea?.Name ?? "",
                        PaymentMethodName = x.PaymentMethod?.Name ?? "",
                        Status = x.Status,
                        TimeOut = x.TimeOut,
                        VehicleTypeName = x.VehicleType?.Name ?? "",
                    }),
                    TotalRecord = result.TotalRecord
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetSessionByUserResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }
    }
}
