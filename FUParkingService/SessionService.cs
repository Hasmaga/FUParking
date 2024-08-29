﻿using FUParkingModel.Enum;
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
                if (!string.IsNullOrEmpty(card.Data.PlateNumber))
                {
                    if (!card.Data.PlateNumber.Equals(req.PlateNumber))
                    {
                        return new Return<dynamic> { Message = ErrorEnumApplication.PLATE_NUMBER_NOT_MATCH };
                    }
                }
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
                if (string.IsNullOrEmpty(card.Data.PlateNumber))
                {
                    if (!customer.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customer.Data == null)
                        return new Return<dynamic> { Message = ErrorEnumApplication.CUSTOMER_NOT_EXIST };
                } else if (!string.IsNullOrEmpty(card.Data.PlateNumber))
                {
                    var objNameNonPaid = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_PLate_In" + Path.GetExtension(req.ImageIn.FileName);
                    var objBodyNameNonPaid = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_Body_IN" + Path.GetExtension(req.ImageBodyIn.FileName);

                    // Create new UploadObjectReqDto                
                    UploadObjectReqDto uploadObjectNonPaidReqDto = new()
                    {
                        BucketName = BucketMinioEnum.BUCKET_PARKiNG,
                        ObjFile = req.ImageIn,
                        ObjName = objNameNonPaid
                    };
                    // Upload image to Minio server and get url image
                    var imageInNonPaidUrl = await _minioService.UploadObjectAsync(uploadObjectNonPaidReqDto);
                    if (imageInNonPaidUrl.IsSuccess == false || imageInNonPaidUrl.Data == null)
                        return new Return<dynamic> { Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED };

                    UploadObjectReqDto uploadImageNonPaidBody = new()
                    {
                        BucketName = BucketMinioEnum.BUCKET_IMAGE_BODY,
                        ObjFile = req.ImageBodyIn,
                        ObjName = objBodyNameNonPaid
                    };
                    var imageNonPaidBodyUrl = await _minioService.UploadObjectAsync(uploadImageNonPaidBody);
                    if (imageNonPaidBodyUrl.IsSuccess == false || imageNonPaidBodyUrl.Data == null)
                        return new Return<dynamic> { Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED };

                    // Create session
                    var newSessionNonPaid = new Session
                    {
                        CardId = card.Data.Id,
                        Block = parkingArea.Data.Block,
                        PlateNumber = req.PlateNumber,
                        GateInId = req.GateInId,
                        ImageInUrl = imageInNonPaidUrl.Data.ObjUrl,
                        ImageInBodyUrl = imageNonPaidBodyUrl.Data.ObjUrl,
                        TimeIn = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                        Mode = parkingArea.Data.Mode,
                        Status = SessionEnum.PARKED,
                        CreatedById = checkAuth.Data.Id,                        
                    };
                    // Create session
                    var newsessionNonPaid = await _sessionRepository.CreateSessionAsync(newSessionNonPaid);
                    if (!newsessionNonPaid.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                    return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY };
                }
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
                var objName = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_PLate_In" + Path.GetExtension(req.ImageIn.FileName);
                var objBodyName = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_Body_IN" + Path.GetExtension(req.ImageBodyIn.FileName);

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

                UploadObjectReqDto uploadImageBody = new()
                {
                    BucketName = BucketMinioEnum.BUCKET_IMAGE_BODY,
                    ObjFile = req.ImageBodyIn,
                    ObjName = objBodyName
                };
                var imageBodyUrl = await _minioService.UploadObjectAsync(uploadImageBody);
                if (imageBodyUrl.IsSuccess == false || imageBodyUrl.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED };

                // Create session
                var newSession = new Session
                {
                    CardId = card.Data.Id,
                    Block = parkingArea.Data.Block,
                    PlateNumber = req.PlateNumber,
                    GateInId = req.GateInId,
                    ImageInUrl = imageInUrl.Data.ObjUrl,
                    ImageInBodyUrl = imageBodyUrl.Data.ObjUrl,
                    TimeIn = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                    Mode = parkingArea.Data.Mode,
                    Status = SessionEnum.PARKED,
                    CreatedById = checkAuth.Data.Id,
                    CustomerId = customer.Data?.Id,
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
                var objName = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_Plate_In" + Path.GetExtension(req.ImageIn.FileName);
                var objBodyName = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_Body_In" + Path.GetExtension(req.ImageBody.FileName);
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

                UploadObjectReqDto uploadImageBody = new()
                {
                    BucketName = BucketMinioEnum.BUCKET_IMAGE_BODY,
                    ObjFile = req.ImageBody,
                    ObjName = objBodyName
                };
                var imageBodyUrl = await _minioService.UploadObjectAsync(uploadImageBody);
                if (imageBodyUrl.IsSuccess == false || imageBodyUrl.Data == null)
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
                    ImageInBodyUrl = imageBodyUrl.Data.ObjUrl,
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

        public async Task<Return<dynamic>> CheckOutAsync(CheckOutAsyncReqDto req)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
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
                var card = await _cardRepository.GetCardByCardNumberAsync(req.CardNumber);
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
                var gateOut = await _gateRepository.GetGateByIdAsync(req.GateOutId);
                if (!gateOut.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = gateOut.InternalErrorMessage };
                if (gateOut.Data == null || !gateOut.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.GATE_NOT_EXIST };
                if (!string.IsNullOrEmpty(sessionCard.Data.Card?.PlateNumber) && sessionCard.Data.VehicleTypeId is null)
                {
                    var objNameNonePaid = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_Plate_Out" + Path.GetExtension(req.ImageOut.FileName);
                    var objNameBodyNonePaid = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_Body_Out" + Path.GetExtension(req.ImageBody.FileName);
                    var uploadObjectNonePaidReqDto = new UploadObjectReqDto
                    {
                        BucketName = BucketMinioEnum.BUCKET_PARKiNG,
                        ObjFile = req.ImageOut,
                        ObjName = objNameNonePaid
                    };
                    var imageOutNonePaidUrl = await _minioService.UploadObjectAsync(uploadObjectNonePaidReqDto);
                    if (imageOutNonePaidUrl.IsSuccess == false || imageOutNonePaidUrl.Data == null)
                        return new Return<dynamic> { Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED };

                    var uploadObjectBodyNonePaidReqDto = new UploadObjectReqDto
                    {
                        BucketName = BucketMinioEnum.BUCKET_IMAGE_BODY,
                        ObjFile = req.ImageBody,
                        ObjName = objNameBodyNonePaid
                    };

                    var imageBodyNonePaidUrl = await _minioService.UploadObjectAsync(uploadObjectBodyNonePaidReqDto);
                    if (imageBodyNonePaidUrl.IsSuccess == false || imageBodyNonePaidUrl.Data == null)
                        return new Return<dynamic> { Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED };

                    sessionCard.Data.GateOutId = gateOut.Data.Id;
                    sessionCard.Data.ImageOutUrl = imageOutNonePaidUrl.Data.ObjUrl;
                    sessionCard.Data.ImageOutBodyUrl = imageBodyNonePaidUrl.Data.ObjUrl;
                    sessionCard.Data.TimeOut = req.TimeOut;
                    sessionCard.Data.LastModifyById = checkAuth.Data.Id;
                    sessionCard.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                    sessionCard.Data.Status = SessionEnum.CLOSED;
                    var updateNonePaidSession = await _sessionRepository.UpdateSessionAsync(sessionCard.Data);
                    if (!updateNonePaidSession.IsSuccess)
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = updateNonePaidSession.InternalErrorMessage };
                    scope.Complete();
                    return new Return<dynamic>
                    {
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY                        
                    };
                }
                // Calculate total block time in minutes
                // Check block of parking area                    
                if (sessionCard.Data.GateIn?.ParkingArea?.Block == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST };
                var totalBlockTime = (int)(req.TimeOut - sessionCard.Data.TimeIn).TotalMinutes / sessionCard.Data.Block;
                int price = 0;
                switch (sessionCard.Data.Mode)
                {
                    case ModeEnum.MODE1:
                        {
                            // Calculate price base on time in
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(sessionCard.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= sessionCard.Data.TimeIn.Hour && x.ApplyToHour >= sessionCard.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).LastOrDefault();
                                if (priceItem == null)
                                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE2:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(sessionCard.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= sessionCard.Data.TimeIn.Hour && x.ApplyToHour >= sessionCard.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).FirstOrDefault();
                                if (priceItem == null)
                                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE3:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(sessionCard.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= sessionCard.Data.TimeIn.Hour && x.ApplyToHour >= sessionCard.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).FirstOrDefault();
                                if (priceItem == null)
                                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE4:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(sessionCard.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= sessionCard.Data.TimeIn.Hour && x.ApplyToHour >= sessionCard.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).LastOrDefault();
                                if (priceItem == null)
                                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    default:
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                // Upload image out
                var objName = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_Plate_Out" + Path.GetExtension(req.ImageOut.FileName);
                var objBodyName = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_Body_Out" + Path.GetExtension(req.ImageBody.FileName);
                var uploadObjectReqDto = new UploadObjectReqDto
                {
                    BucketName = BucketMinioEnum.BUCKET_PARKiNG,
                    ObjFile = req.ImageOut,
                    ObjName = objName
                };
                var imageOutUrl = await _minioService.UploadObjectAsync(uploadObjectReqDto);
                if (imageOutUrl.IsSuccess == false || imageOutUrl.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED };

                var uploadObjectBodyReqDto = new UploadObjectReqDto
                {
                    BucketName = BucketMinioEnum.BUCKET_IMAGE_BODY,
                    ObjFile = req.ImageBody,
                    ObjName = objBodyName
                };
                var imageBodyUrl = await _minioService.UploadObjectAsync(uploadObjectBodyReqDto);
                if (imageBodyUrl.IsSuccess == false || imageBodyUrl.Data == null)
                    return new Return<dynamic> { Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED };

                // Try minus balance of customer wallet

                if (sessionCard.Data.CustomerId.HasValue)
                {
                    var walletMain = await _walletRepository.GetMainWalletByCustomerId(sessionCard.Data.CustomerId.Value);
                    if (!walletMain.IsSuccess)
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = walletMain.InternalErrorMessage };
                    if (walletMain.Data == null || !walletMain.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                        return new Return<dynamic> { Message = ErrorEnumApplication.WALLET_NOT_EXIST };
                    var walletExtra = await _walletRepository.GetExtraWalletByCustomerId(sessionCard.Data.CustomerId.Value);
                    if (!walletExtra.IsSuccess)
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = walletExtra.InternalErrorMessage };
                    if (walletExtra.Data == null || !walletExtra.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                        return new Return<dynamic> { Message = ErrorEnumApplication.WALLET_NOT_EXIST };                   // Minus balance of waller Extra first if balance of wallet Extra is enough to pay price of session then minus balance of walletExtra + walletMain if not enough then return error
                    if (walletExtra.Data.Balance >= price)
                    {
                        // Get PaymentMethodId
                        var paymentMethod = await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.WALLET);
                        if (!paymentMethod.IsSuccess || paymentMethod.Data == null || !paymentMethod.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = paymentMethod.InternalErrorMessage };
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
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
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
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        walletExtra.Data.Balance -= price;
                        var updateWalletExtra = await _walletRepository.UpdateWalletAsync(walletExtra.Data);
                        if (!updateWalletExtra.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            scope.Dispose();
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                    }
                    else if (walletExtra.Data.Balance + walletMain.Data.Balance >= price)
                    {
                        // Get PaymentMethodId
                        var paymentMethod = await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.WALLET);
                        if (!paymentMethod.IsSuccess || paymentMethod.Data == null || !paymentMethod.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = paymentMethod.InternalErrorMessage };
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
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
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
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
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
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
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
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        walletExtra.Data.Balance = 0;
                        walletMain.Data.Balance -= price - walletExtra.Data.Balance;
                        var updateWalletExtra = await _walletRepository.UpdateWalletAsync(walletExtra.Data);
                        if (!updateWalletExtra.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            scope.Dispose();
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        var updateWalletMain = await _walletRepository.UpdateWalletAsync(walletMain.Data);
                        if (!updateWalletMain.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            scope.Dispose();
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
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
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = paymentMethod.InternalErrorMessage };
                        }
                        sessionCard.Data.ImageOutBodyUrl = imageBodyUrl.Data.ObjUrl;
                        sessionCard.Data.ImageOutUrl = imageOutUrl.Data.ObjUrl;
                        sessionCard.Data.GateOutId = req.GateOutId;
                        sessionCard.Data.TimeOut = req.TimeOut;
                        sessionCard.Data.LastModifyById = checkAuth.Data.Id;
                        sessionCard.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                        sessionCard.Data.PaymentMethodId = paymentMethod.Data.Id;
                        sessionCard.Data.Status = SessionEnum.CLOSED;
                        var isUpdateSession = await _sessionRepository.UpdateSessionAsync(sessionCard.Data);
                        if (!isUpdateSession.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            scope.Dispose();
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }

                        // Create payment for session
                        var payment = new Payment
                        {
                            PaymentMethodId = paymentMethod.Data.Id,
                            SessionId = sessionCard.Data.Id,
                            TotalPrice = price,
                        };
                        var createPayment = await _paymentRepository.CreatePaymentAsync(payment);
                        if (!createPayment.IsSuccess || createPayment.Data == null)
                        {
                            scope.Dispose();
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        var transaction = new FUParkingModel.Object.Transaction
                        {
                            PaymentId = createPayment.Data.Id,
                            Amount = price,
                            TransactionDescription = "Pay for parking",
                            TransactionStatus = StatusTransactionEnum.SUCCEED,
                        };
                        var createTransaction = await _transactionRepository.CreateTransactionAsync(transaction);
                        if (!createTransaction.IsSuccess || createTransaction.Data == null)
                        {
                            scope.Dispose();
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        scope.Complete();
                        return new Return<dynamic>
                        {
                            IsSuccess = true,
                            Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY                            
                        };
                    }
                    // GetPaymentMethod wallet
                    var paymentMethodWallet = await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.WALLET);
                    if (!paymentMethodWallet.IsSuccess || paymentMethodWallet.Data == null || !paymentMethodWallet.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    {
                        scope.Dispose();
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = paymentMethodWallet.InternalErrorMessage };
                    }
                    sessionCard.Data.ImageOutBodyUrl = imageBodyUrl.Data.ObjUrl;
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
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                    }
                    scope.Complete();
                    return new Return<dynamic>
                    {
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY                        
                    };
                }
                // For guest 
                else
                {
                    var paymentMethod = await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.CASH);
                    if (!paymentMethod.IsSuccess || paymentMethod.Data == null || !paymentMethod.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    {
                        scope.Dispose();
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = paymentMethod.InternalErrorMessage };
                    }
                    sessionCard.Data.ImageOutBodyUrl = imageBodyUrl.Data.ObjUrl;
                    sessionCard.Data.ImageOutUrl = imageOutUrl.Data.ObjUrl;
                    sessionCard.Data.GateOutId = req.GateOutId;
                    sessionCard.Data.TimeOut = req.TimeOut;
                    sessionCard.Data.LastModifyById = checkAuth.Data.Id;
                    sessionCard.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                    sessionCard.Data.PaymentMethodId = paymentMethod.Data.Id;
                    sessionCard.Data.Status = SessionEnum.CLOSED;
                    var isUpdateSession = await _sessionRepository.UpdateSessionAsync(sessionCard.Data);
                    if (!isUpdateSession.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    {
                        scope.Dispose();
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                    }
                    // Create payment for session
                    var payment = new Payment
                    {
                        PaymentMethodId = paymentMethod.Data.Id,
                        SessionId = sessionCard.Data.Id,
                        TotalPrice = price,
                    };
                    var createPayment = await _paymentRepository.CreatePaymentAsync(payment);
                    if (!createPayment.IsSuccess || createPayment.Data == null)
                    {
                        scope.Dispose();
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                    }
                    var transaction = new FUParkingModel.Object.Transaction
                    {
                        PaymentId = createPayment.Data.Id,
                        Amount = price,
                        TransactionDescription = "Pay for parking",
                        TransactionStatus = StatusTransactionEnum.SUCCEED,
                    };
                    var createTransaction = await _transactionRepository.CreateTransactionAsync(transaction);
                    if (!createTransaction.IsSuccess || createTransaction.Data == null)
                    {
                        scope.Dispose();
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                    }
                    scope.Complete();
                    return new Return<dynamic>
                    {
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,                        
                    };
                }
            }
            catch (Exception ex)
            {
                scope.Dispose();
                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<dynamic>> UpdatePaymentSessionAsync(string CardNumber)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    scope.Dispose();
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
                if (!updateSession.IsSuccess)
                {
                    scope.Dispose();
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                // Update transaction of that session to SUCCEED
                var transaction = await _transactionRepository.GetTransactionBySessionIdAsync(sessionCard.Data.Id);
                if (!transaction.IsSuccess || transaction.Data is null)
                {
                    scope.Dispose();
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = transaction.InternalErrorMessage };
                }
                transaction.Data.TransactionStatus = StatusTransactionEnum.SUCCEED;
                var updateTransaction = await _transactionRepository.UpdateTransactionAsync(transaction.Data);
                if (!updateSession.IsSuccess)
                {
                    scope.Dispose();
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                scope.Complete();
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

                    var moneyEstimatedNeedToPay = 0;
                    if (item.Status.Equals(SessionEnum.PARKED))
                    {
                        var totalBlockTime = (int)(TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")) - item.TimeIn).TotalMinutes / item.Block;
                        switch (item.Mode)
                        {
                            case ModeEnum.MODE1:
                                {
                                    // Calculate price base on time in
                                    // Check VehicleTypeId
                                    var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(item.VehicleTypeId ?? Guid.Empty);
                                    if (!vehicleType.IsSuccess)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                                    if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                                    if (!listPriceTable.IsSuccess)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                                    if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    // Check which package is have higher piority
                                    var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                                    if (priceTable == null)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    // Get list price item in price table
                                    var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                                    if (!listPriceItem.IsSuccess)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                                    if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                                    var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= item.TimeIn.Hour && x.ApplyToHour >= item.TimeIn.Hour).FirstOrDefault();
                                    if (priceItem == null)
                                    {
                                        // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                        priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).LastOrDefault();
                                        if (priceItem == null)
                                            return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    }
                                    // Calculate price
                                    moneyEstimatedNeedToPay = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                                    break;
                                }
                            case ModeEnum.MODE2:
                                {
                                    // Calculate price base on time out
                                    // Check VehicleTypeId
                                    var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(item.VehicleTypeId ?? Guid.Empty);
                                    if (!vehicleType.IsSuccess)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                                    if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                                    if (!listPriceTable.IsSuccess)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                                    if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    // Check which package is have higher piority
                                    var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                                    if (priceTable == null)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    // Get list price item in price table
                                    var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                                    if (!listPriceItem.IsSuccess)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                                    if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                                    var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= item.TimeIn.Hour && x.ApplyToHour >= item.TimeIn.Hour).FirstOrDefault();
                                    if (priceItem == null)
                                    {
                                        // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                        priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).FirstOrDefault();
                                        if (priceItem == null)
                                            return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    }
                                    // Calculate price
                                    moneyEstimatedNeedToPay = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                                    break;
                                }
                            case ModeEnum.MODE3:
                                {
                                    // Calculate price base on time out
                                    // Check VehicleTypeId
                                    var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(item.VehicleTypeId ?? Guid.Empty);
                                    if (!vehicleType.IsSuccess)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                                    if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                                    if (!listPriceTable.IsSuccess)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                                    if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    // Check which package is have higher piority
                                    var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                                    if (priceTable == null)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    // Get list price item in price table
                                    var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                                    if (!listPriceItem.IsSuccess)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                                    if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                                    var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= item.TimeIn.Hour && x.ApplyToHour >= item.TimeIn.Hour).FirstOrDefault();
                                    if (priceItem == null)
                                    {
                                        // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                        priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).FirstOrDefault();
                                        if (priceItem == null)
                                            return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    }
                                    // Calculate price
                                    moneyEstimatedNeedToPay = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                                    break;
                                }
                            case ModeEnum.MODE4:
                                {
                                    // Calculate price base on time out
                                    // Check VehicleTypeId
                                    var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(item.VehicleTypeId ?? Guid.Empty);
                                    if (!vehicleType.IsSuccess)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                                    if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                                    if (!listPriceTable.IsSuccess)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                                    if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    // Check which package is have higher piority
                                    var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                                    if (priceTable == null)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    // Get list price item in price table
                                    var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                                    if (!listPriceItem.IsSuccess)
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                                    if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                        return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                                    var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= item.TimeIn.Hour && x.ApplyToHour >= item.TimeIn.Hour).FirstOrDefault();
                                    if (priceItem == null)
                                    {
                                        // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                        priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).LastOrDefault();
                                        if (priceItem == null)
                                            return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                                    }
                                    // Calculate price
                                    moneyEstimatedNeedToPay = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                                    break;
                                }
                            default:
                                return new Return<IEnumerable<GetHistorySessionResDto>> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
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
                        IsFeedback = item.Feedbacks.Count > 0,
                        MoneyEstimated = moneyEstimatedNeedToPay == 0 ? null : moneyEstimatedNeedToPay
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

        public async Task<Return<dynamic>> CheckOutSessionByPlateNumberAsync(CheckOutSessionByPlateNumberReqDto req)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check Gate
                if (req.GateId != null)
                {
                    var isGateExist = await _gateRepository.GetGateByIdAsync(req.GateId ?? Guid.Empty);
                    if (!isGateExist.IsSuccess || isGateExist.Data == null || isGateExist.Data.GateType == null)
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = isGateExist.InternalErrorMessage,
                            Message = ErrorEnumApplication.GATE_NOT_EXIST
                        };
                    }

                    // Check Gate is gate out 
                    if (!isGateExist.Data.GateType.Name.Equals(GateTypeEnum.OUT))
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.NOT_GATE_OUT
                        };
                    }
                }

                // Check PlateNumber
                var session = await _sessionRepository.GetNewestSessionByPlateNumberAsync(req.PlateNumber);
                if (!session.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || session.Data is null)
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = session.InternalErrorMessage,
                        Message = ErrorEnumApplication.NOT_FOUND_SESSION_WITH_PLATE_NUMBER
                    };
                }
                if (session.Data.Status.Equals(SessionEnum.CLOSED))
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SESSION_CLOSE
                    };
                }
                if (session.Data.Status.Equals(SessionEnum.CANCELLED))
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SESSION_CANCELLED
                    };
                }
                // Get virtual gate
                var gateOut = await _gateRepository.GetVirtualGateAsync();
                if (!gateOut.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || gateOut.Data == null)
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = gateOut.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }

                string imagePlateOutUrl = "";
                string imageBodyOutUrl = "";

                // Upload image if have 
                if (req.ImagePlate is not null && req.ImageBody is not null)
                {
                    var objNameImagePlateOut = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_Plate_Out" + Path.GetExtension(req.ImagePlate.FileName);
                    var objImagePlateOut = new UploadObjectReqDto()
                    {
                        BucketName = BucketMinioEnum.BUCKET_PARKiNG,
                        ObjFile = req.ImagePlate,
                        ObjName = objNameImagePlateOut
                    };
                    var uploadImagePlateOut = await _minioService.UploadObjectAsync(objImagePlateOut);
                    if (uploadImagePlateOut.IsSuccess == false || uploadImagePlateOut.Data == null)
                    {
                        scope.Dispose();
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = uploadImagePlateOut.InternalErrorMessage };
                    }

                    var objNameImageBodyOut = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + "_Body_Out" + Path.GetExtension(req.ImageBody.FileName);
                    var objImageBodyOut = new UploadObjectReqDto()
                    {
                        BucketName = BucketMinioEnum.BUCKET_IMAGE_BODY,
                        ObjFile = req.ImageBody,
                        ObjName = objNameImageBodyOut
                    };
                    var uploadImageBodyOut = await _minioService.UploadObjectAsync(objImageBodyOut);
                    if (uploadImageBodyOut.IsSuccess == false || uploadImageBodyOut.Data == null)
                    {
                        scope.Dispose();
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = uploadImageBodyOut.InternalErrorMessage };
                    }
                    imagePlateOutUrl = uploadImagePlateOut.Data.ObjUrl;
                    imageBodyOutUrl = uploadImageBodyOut.Data.ObjUrl;
                }

                var totalBlockTime = (int)(req.CheckOutTime - session.Data.TimeIn).TotalMinutes / session.Data.Block;
                int price = 0;
                // check customer type is free
                if ((session.Data.Customer?.CustomerType ?? new CustomerType() { Description = "", Name = "" }).Name.Equals(CustomerTypeEnum.FREE))
                {
                    session.Data.GateOutId = req.GateId is null ? gateOut.Data.Id : req.GateId;
                    session.Data.TimeOut = req.CheckOutTime;
                    session.Data.ImageOutUrl = imagePlateOutUrl;
                    session.Data.ImageOutBodyUrl = imageBodyOutUrl;
                    session.Data.LastModifyById = checkAuth.Data.Id;
                    session.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                    session.Data.Status = SessionEnum.CLOSED;
                    var updateNonePaidSession = await _sessionRepository.UpdateSessionAsync(session.Data);
                    if (!updateNonePaidSession.IsSuccess)
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = updateNonePaidSession.InternalErrorMessage };
                    scope.Complete();
                    return new Return<dynamic>
                    {
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                    };
                }

                switch (session.Data.Mode)
                {
                    case ModeEnum.MODE1:
                        {
                            // Calculate price base on time in
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(session.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= session.Data.TimeIn.Hour && x.ApplyToHour >= session.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).LastOrDefault();
                                if (priceItem == null)
                                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE2:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(session.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= session.Data.TimeIn.Hour && x.ApplyToHour >= session.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).FirstOrDefault();
                                if (priceItem == null)
                                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE3:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(session.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= session.Data.TimeIn.Hour && x.ApplyToHour >= session.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).FirstOrDefault();
                                if (priceItem == null)
                                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE4:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(session.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= session.Data.TimeIn.Hour && x.ApplyToHour >= session.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).LastOrDefault();
                                if (priceItem == null)
                                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            price = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    default:
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                var paymentMethod = await _paymentRepository.GetPaymentMethodByNameAsync(PaymentMethods.CASH);
                if (!paymentMethod.IsSuccess || paymentMethod.Data == null || !paymentMethod.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    scope.Dispose();
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = paymentMethod.InternalErrorMessage };
                }
                session.Data.GateOutId = req.GateId is null ? gateOut.Data.Id : req.GateId;
                session.Data.TimeOut = req.CheckOutTime;
                session.Data.ImageOutBodyUrl = imageBodyOutUrl;
                session.Data.ImageOutUrl = imagePlateOutUrl;
                session.Data.LastModifyById = checkAuth.Data.Id;
                session.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                session.Data.PaymentMethodId = paymentMethod.Data.Id;
                session.Data.Status = SessionEnum.CLOSED;
                var isUpdateSession = await _sessionRepository.UpdateSessionAsync(session.Data);
                if (!isUpdateSession.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    scope.Dispose();
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                // Create payment 
                Payment payment = new()
                {
                    PaymentMethodId = paymentMethod.Data.Id,
                    TotalPrice = price,
                    SessionId = session.Data.Id
                };

                var createPayment = await _paymentRepository.CreatePaymentAsync(payment);
                if (!createPayment.IsSuccess || createPayment.Data is null)
                {
                    scope.Dispose();
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = createPayment.InternalErrorMessage };
                }

                // Create transaction
                FUParkingModel.Object.Transaction transaction = new()
                {
                    TransactionDescription = "Pay for parking",
                    TransactionStatus = StatusTransactionEnum.SUCCEED,
                    PaymentId = createPayment.Data.Id,
                    Amount = price
                };

                var createTransaction = await _transactionRepository.CreateTransactionAsync(transaction);
                if (!createTransaction.IsSuccess || createTransaction.Data is null)
                {
                    scope.Dispose();
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = createTransaction.InternalErrorMessage };
                }

                // Update Card to missing 
                if (session.Data.Card?.Id is not null)
                {
                    var card = await _cardRepository.GetCardByIdAsync(session.Data.Card.Id);
                    if (!card.IsSuccess || card.Data == null || !card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    {
                        scope.Dispose();
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = card.InternalErrorMessage };
                    }
                    card.Data.Status = CardStatusEnum.MISSING;
                    var updateCard = await _cardRepository.UpdateCardAsync(card.Data);
                    if (!updateCard.IsSuccess)
                    {
                        scope.Dispose();
                        return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = updateCard.InternalErrorMessage };
                    }
                }
                scope.Complete();
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                };
            }
            catch (Exception ex)
            {
                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<IEnumerable<GetSessionByUserResDto>>> GetListSessionByUserAsync(GetListObjectWithFillerAttributeAndDateReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
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

        public async Task<Return<GetSessionByUserResDto>> GetSessionBySessionIdAsync(Guid sessionId)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<GetSessionByUserResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _sessionRepository.GetSessionByIdAsync(sessionId);
                if (!result.IsSuccess || result.Data == null)
                {
                    return new Return<GetSessionByUserResDto>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                return new Return<GetSessionByUserResDto>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new GetSessionByUserResDto
                    {
                        Id = result.Data.Id,
                        CardNumber = result.Data.Card?.CardNumber ?? "",
                        Mode = result.Data.Mode,
                        ImageOutUrl = result.Data.ImageOutUrl ?? "",
                        ImageInUrl = result.Data.ImageInUrl,
                        GateOutName = result.Data.GateOut?.Name ?? "",
                        GateInName = result.Data.GateIn?.Name ?? "",
                        PlateNumber = result.Data.PlateNumber,
                        CheckInStaff = result.Data.CreateBy?.Email ?? "",
                        CheckOutStaff = result.Data.LastModifyBy?.Email ?? "",
                        TimeIn = result.Data.TimeIn,
                        CustomerEmail = result.Data.Customer?.Email ?? "",
                        ParkingArea = result.Data.GateIn?.ParkingArea?.Name ?? "",
                        PaymentMethodName = result.Data.PaymentMethod?.Name ?? "",
                        Status = result.Data.Status,
                        TimeOut = result.Data.TimeOut,
                        VehicleTypeName = result.Data.VehicleType?.Name ?? "",
                    }
                };
            }
            catch (Exception ex)
            {
                return new Return<GetSessionByUserResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<bool>> CancleSessionByIdAsync(Guid sessionId)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _sessionRepository.GetSessionByIdAsync(sessionId);
                if (!result.IsSuccess || result.Data == null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                if (!result.Data.Status.Equals(SessionEnum.PARKED))
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                    };
                }
                result.Data.Status = SessionEnum.CANCELLED;
                result.Data.LastModifyById = checkAuth.Data.Id;
                result.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var updateSession = await _sessionRepository.UpdateSessionAsync(result.Data);
                if (!updateSession.IsSuccess)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = updateSession.InternalErrorMessage,
                        Message = updateSession.Message
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                    Data = true
                };

            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<int>> GetTotalSessionParkedAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<int>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _sessionRepository.GetTotalSessionParkedAsync();
                if (!result.IsSuccess)
                {
                    return new Return<int>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                return new Return<int>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                return new Return<int>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<double>> GetAverageSessionDurationPerDayAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<double>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _sessionRepository.GetAverageSessionDurationPerDayAsync();
                if (!result.IsSuccess)
                {
                    return new Return<double>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }

                return new Return<double>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                return new Return<double>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<StatisticCheckInCheckOutResDto>> GetStatisticCheckInCheckOutAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<StatisticCheckInCheckOutResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _sessionRepository.GetStatisticCheckInCheckOutAsync();
                if (!result.IsSuccess)
                {
                    return new Return<StatisticCheckInCheckOutResDto>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                return new Return<StatisticCheckInCheckOutResDto>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                return new Return<StatisticCheckInCheckOutResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<StatisticSessionTodayResDto>> GetStatisticCheckInCheckOutInParkingAreaAsync(Guid parkingId)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<StatisticSessionTodayResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _sessionRepository.GetStatisticCheckInCheckOutInParkingAreaAsync(parkingId);
                if (!result.IsSuccess)
                {
                    return new Return<StatisticSessionTodayResDto>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                return new Return<StatisticSessionTodayResDto>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                return new Return<StatisticSessionTodayResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<IEnumerable<GetAllSessionTodayResDto>>> GetAllSessionByCardNumberAndPlateNumberAsync(Guid parkingId, string? plateNum, string? cardNum, int pageIndex, int pageSize, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetAllSessionTodayResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _sessionRepository.GetAllSessionByCardNumberAndPlateNumberAsync(parkingId, plateNum, cardNum, pageIndex, pageSize, startDate, endDate);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetAllSessionTodayResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                return new Return<IEnumerable<GetAllSessionTodayResDto>>
                {
                    IsSuccess = true,
                    Data = result.Data?.Select(fb => new GetAllSessionTodayResDto
                    {
                        Id = fb.Id,
                        PlateNumber = fb.PlateNumber,
                        CardNumber = fb.Card?.CardNumber ?? "N/A",
                        GateInName = fb.GateIn?.Name ?? "N/A",
                        GateOutName = fb.GateOut?.Name ?? "N/A",
                        ImageInUrl = fb.ImageInUrl,
                        ImageOutUrl = fb.ImageOutUrl!,
                        TimeIn = fb.TimeIn,
                        TimeOut = fb.TimeOut,
                        Status = fb.Status,
                        Mode = fb.Mode,
                        Block = fb.Block,
                        VehicleTypeName = fb.VehicleType?.Name ?? "N/A",
                        PaymentMethodName = fb.PaymentMethod?.Name ?? "N/A",
                        CustomerEmail = fb.Customer?.Email ?? "N/A",
                        ImageInBodyUrl = fb.ImageInBodyUrl,
                        ImageOutBodyUrl = fb.ImageOutBodyUrl!,
                    }),
                    Message = result.TotalRecord > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    TotalRecord = result.TotalRecord
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetAllSessionTodayResDto>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<GetSessionByCardNumberResDto>> GetNewestSessionByCardNumberAsync(string CardNumber, DateTime TimeOut)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<GetSessionByCardNumberResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _sessionRepository.GetNewestSessionByCardNumberAsync(CardNumber);
                if (!result.IsSuccess)
                {
                    return new Return<GetSessionByCardNumberResDto>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                // If Session is not found
                if (result.Data == null)
                {
                    return new Return<GetSessionByCardNumberResDto>
                    {
                        Message = ErrorEnumApplication.NOT_FOUND_SESSION_WITH_CARD_NUMBER
                    };
                }
                if (result.Data.Status.Equals(SessionEnum.CLOSED))
                {
                    return new Return<GetSessionByCardNumberResDto>
                    {
                        Message = ErrorEnumApplication.SESSION_CLOSE
                    };
                }
                if (result.Data.Status.Equals(SessionEnum.CANCELLED))
                {
                    return new Return<GetSessionByCardNumberResDto>
                    {
                        Message = ErrorEnumApplication.SESSION_CANCELLED
                    };
                }
                // Check timeOut is greater than TimeIn
                if (TimeOut < result.Data.TimeIn)
                {
                    return new Return<GetSessionByCardNumberResDto>
                    {
                        Message = ErrorEnumApplication.TIME_OUT_IS_MUST_BE_GREATER_TIME_IN
                    };
                }

                var amount = 0;
                var totalBlockTime = (int)(TimeOut - result.Data.TimeIn).TotalMinutes / result.Data.Block;
                switch (result.Data.Mode)
                {
                    case ModeEnum.MODE1:
                        {
                            // Calculate price base on time in
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(result.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= result.Data.TimeIn.Hour && x.ApplyToHour >= result.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).LastOrDefault();
                                if (priceItem == null)
                                    return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            amount = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE2:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(result.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= result.Data.TimeIn.Hour && x.ApplyToHour >= result.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).FirstOrDefault();
                                if (priceItem == null)
                                    return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            amount = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE3:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(result.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= result.Data.TimeIn.Hour && x.ApplyToHour >= result.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).FirstOrDefault();
                                if (priceItem == null)
                                    return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            amount = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE4:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(result.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= result.Data.TimeIn.Hour && x.ApplyToHour >= result.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).LastOrDefault();
                                if (priceItem == null)
                                    return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            amount = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    default:
                        return new Return<GetSessionByCardNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                bool? isEnoughToPay;
                // Check if customer is paid
                if (result.Data.Customer?.CustomerType?.Name.Equals(CustomerTypeEnum.PAID) ?? false && result.Data.CustomerId is not null)
                {
                    // Get Customer Wallet Main 
                    var customerWalletMain = await _walletRepository.GetMainWalletByCustomerId(result.Data.CustomerId ?? new Guid());
                    if (!customerWalletMain.IsSuccess)
                    {
                        return new Return<GetSessionByCardNumberResDto>
                        {
                            InternalErrorMessage = customerWalletMain.InternalErrorMessage,
                            Message = customerWalletMain.Message
                        };
                    }

                    var customerWalletExtra = await _walletRepository.GetExtraWalletByCustomerId(result.Data.CustomerId ?? new Guid());
                    if (!customerWalletExtra.IsSuccess)
                    {
                        return new Return<GetSessionByCardNumberResDto>
                        {
                            InternalErrorMessage = customerWalletExtra.InternalErrorMessage,
                            Message = customerWalletExtra.Message
                        };
                    }

                    if (customerWalletMain.Data is not null && customerWalletExtra.Data is not null)
                    {
                        isEnoughToPay = (customerWalletMain.Data.Balance + customerWalletExtra.Data.Balance) >= amount;
                    }
                    else
                    {
                        isEnoughToPay = null;
                    }
                }
                else
                {
                    isEnoughToPay = null;
                }

                return new Return<GetSessionByCardNumberResDto>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new GetSessionByCardNumberResDto
                    {
                        CardId = result.Data.CardId,
                        GateIn = result.Data.GateIn?.Name ?? "",
                        GateOut = result.Data.GateOut?.Name ?? "",
                        ImageInBodyUrl = result.Data.ImageInBodyUrl,
                        ImageInUrl = result.Data.ImageInUrl,
                        PlateNumber = result.Data.PlateNumber,
                        TimeIn = result.Data.TimeIn,
                        VehicleType = result.Data.VehicleType?.Name ?? "",
                        Amount = amount,
                        IsEnoughToPay = isEnoughToPay
                    }
                };
            }
            catch (Exception ex)
            {
                return new Return<GetSessionByCardNumberResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<GetSessionByPlateNumberResDto>> GetNewestSessionByPlateNumberAsync(string PlateNumber, DateTime TimeOut)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<GetSessionByPlateNumberResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                // Check newest session by plate number
                var result = await _sessionRepository.GetNewestSessionByPlateNumberAsync(PlateNumber);
                if (!result.IsSuccess)
                {
                    return new Return<GetSessionByPlateNumberResDto>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }

                if (result.Data == null)
                {
                    return new Return<GetSessionByPlateNumberResDto>
                    {
                        Message = ErrorEnumApplication.NOT_FOUND_SESSION_WITH_PLATE_NUMBER
                    };
                }

                // if session close 
                if (result.Data.Status.Equals(SessionEnum.CLOSED))
                {
                    return new Return<GetSessionByPlateNumberResDto>
                    {
                        Message = ErrorEnumApplication.SESSION_CLOSE
                    };
                }

                // if session cancelled
                if (result.Data.Status.Equals(SessionEnum.CANCELLED))
                {
                    return new Return<GetSessionByPlateNumberResDto>
                    {
                        Message = ErrorEnumApplication.SESSION_CANCELLED
                    };
                }
                // Check timeOut is greater than TimeIn
                if (TimeOut < result.Data.TimeIn)
                {
                    return new Return<GetSessionByPlateNumberResDto>
                    {
                        Message = ErrorEnumApplication.TIME_OUT_IS_MUST_BE_GREATER_TIME_IN
                    };
                }
                var amount = 0;
                var totalBlockTime = (int)(TimeOut - result.Data.TimeIn).TotalMinutes / result.Data.Block;
                switch (result.Data.Mode)
                {
                    case ModeEnum.MODE1:
                        {
                            // Calculate price base on time in
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(result.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= result.Data.TimeIn.Hour && x.ApplyToHour >= result.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).LastOrDefault();
                                if (priceItem == null)
                                    return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            amount = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE2:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(result.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= result.Data.TimeIn.Hour && x.ApplyToHour >= result.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.ApplyFromHour).FirstOrDefault();
                                if (priceItem == null)
                                    return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            amount = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE3:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(result.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= result.Data.TimeIn.Hour && x.ApplyToHour >= result.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).FirstOrDefault();
                                if (priceItem == null)
                                    return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            amount = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    case ModeEnum.MODE4:
                        {
                            // Calculate price base on time out
                            // Check VehicleTypeId
                            var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(result.Data.VehicleTypeId ?? Guid.Empty);
                            if (!vehicleType.IsSuccess)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };
                            if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            var listPriceTable = await _priceRepository.GetListPriceTableActiveByVehicleTypeAsync(vehicleType.Data.Id);
                            if (!listPriceTable.IsSuccess)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceTable.InternalErrorMessage };
                            if (listPriceTable.Data == null || !listPriceTable.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Check which package is have higher piority
                            var priceTable = listPriceTable.Data.OrderByDescending(x => x.Priority).FirstOrDefault();
                            if (priceTable == null)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get list price item in price table
                            var listPriceItem = await _priceRepository.GetAllPriceItemByPriceTableAsync(priceTable.Id);
                            if (!listPriceItem.IsSuccess)
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = listPriceItem.InternalErrorMessage };
                            if (listPriceItem.Data == null || !listPriceItem.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            // Get price item have ApplyFromHour is <= TimeIn and ApplyToHour is >= TimeIn
                            var priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour <= result.Data.TimeIn.Hour && x.ApplyToHour >= result.Data.TimeIn.Hour).FirstOrDefault();
                            if (priceItem == null)
                            {
                                // Use default price item have ApplyFromHour is null and ApplyToHour is null
                                priceItem = listPriceItem.Data.Where(x => x.ApplyFromHour == null && x.ApplyToHour == null).OrderByDescending(t => t.MaxPrice).LastOrDefault();
                                if (priceItem == null)
                                    return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                            }
                            // Calculate price
                            amount = Math.Min(Math.Max(priceItem.BlockPricing * totalBlockTime, priceItem.MinPrice), priceItem.MaxPrice);
                            break;
                        }
                    default:
                        return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR };
                }

                return new Return<GetSessionByPlateNumberResDto>
                {
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new GetSessionByPlateNumberResDto
                    {
                        Amount = amount,
                        CustomerEmail = result.Data.Customer?.Email ?? "",
                        GateIn = result.Data.GateIn?.Name ?? "",
                        ImageInBodyUrl = result.Data.ImageInUrl,
                        ImageInUrl = result.Data.ImageInUrl,
                        StaffCheckInEmail = result.Data.CreateBy?.Email ?? "",
                        TimeIn = result.Data.TimeIn,
                        VehicleType = result.Data.VehicleType?.Name ?? ""
                    }
                };
            }
            catch (Exception ex)
            {
                return new Return<GetSessionByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<dynamic>> UpdatePlateNumberInSessionAsync(UpdatePlateNumberInSessionReqDto req)
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

                var session = await _sessionRepository.GetSessionByIdAsync(req.SessionId);
                if (!session.IsSuccess || session.Data == null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = session.InternalErrorMessage,
                        Message = session.Message
                    };
                }
                if (session.Data.Status.Equals(SessionEnum.CLOSED))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SESSION_CLOSE
                    };
                }
                if (session.Data.Status.Equals(SessionEnum.CANCELLED))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SESSION_CANCELLED
                    };
                }
                // Check Plate Number is belong to another session
                var checkPlateNumber = await _sessionRepository.GetNewestSessionByPlateNumberAsync(req.PlateNumber);
                if (!checkPlateNumber.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkPlateNumber.InternalErrorMessage,
                        Message = checkPlateNumber.Message
                    };
                }
                if (checkPlateNumber.Data != null && checkPlateNumber.Data.Id != req.SessionId)
                {
                    if (checkPlateNumber.Data.Status.Equals(SessionEnum.PARKED))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.PLATE_NUMBER_IS_BELONG_TO_ANOTHER_SESSION
                        };
                    }
                }

                // Check Plate Number is belong to any user
                var checkPlateNumberIsBelongToUser = await _customerRepository.GetCustomerByPlateNumberAsync(req.PlateNumber);
                if (!checkPlateNumberIsBelongToUser.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkPlateNumberIsBelongToUser.InternalErrorMessage,
                        Message = checkPlateNumberIsBelongToUser.Message
                    };
                }
                // Update Plate Number
                session.Data.PlateNumber = req.PlateNumber;
                session.Data.CustomerId = checkPlateNumberIsBelongToUser.Data?.Id;
                session.Data.LastModifyById = checkAuth.Data.Id;
                session.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var result = await _sessionRepository.UpdateSessionAsync(session.Data);
                if (!result.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
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
                return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }
    }
}
