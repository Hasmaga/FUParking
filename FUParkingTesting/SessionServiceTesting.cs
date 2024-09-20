using FirebaseService;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Firebase;
using FUParkingModel.RequestObject.Session;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using FUParkingModel.ResponseObject;
using Vehicle = FUParkingModel.Object.Vehicle;
using Microsoft.EntityFrameworkCore;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.RequestObject.Common;
using System.ComponentModel.Design;
using FUParkingModel.ResponseObject.Session;
using FUParkingModel.RequestObject.Customer;
using FirebaseAdmin.Messaging;
using Org.BouncyCastle.Ocsp;

namespace FUParkingTesting
{
    public class SessionServiceTesting
    {
        private readonly Mock<ISessionRepository> _sessionRepositoryMock = new();
        private readonly Mock<IHelpperService> _helpperServiceMock = new();
        private readonly Mock<ICardRepository> _cardRepositoryMock = new();
        private readonly Mock<IGateRepository> _gateRepositoryMock = new();
        private readonly Mock<IMinioService> _minioServiceMock = new();
        private readonly Mock<IParkingAreaRepository> _parkingAreaRepositoryMock = new();
        private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
        private readonly Mock<IWalletRepository> _walletRepositoryMock = new();
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock = new();
        private readonly Mock<IPriceRepository> _priceRepositoryMock = new();
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock = new();
        private readonly Mock<IFirebaseService> _firebaseServiceMock = new();

        private readonly SessionService _sessionService;

        public SessionServiceTesting()
        {
            _sessionRepositoryMock = new Mock<ISessionRepository>();
            _helpperServiceMock = new Mock<IHelpperService>();
            _cardRepositoryMock = new Mock<ICardRepository>();
            _gateRepositoryMock = new Mock<IGateRepository>();
            _minioServiceMock = new Mock<IMinioService>();
            _parkingAreaRepositoryMock = new Mock<IParkingAreaRepository>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _walletRepositoryMock = new Mock<IWalletRepository>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _priceRepositoryMock = new Mock<IPriceRepository>();
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();
            _firebaseServiceMock = new Mock<IFirebaseService>();

            _sessionService = new SessionService(
                _sessionRepositoryMock.Object,
                _helpperServiceMock.Object,
                _cardRepositoryMock.Object,
                _gateRepositoryMock.Object,
                _minioServiceMock.Object,
                _parkingAreaRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _walletRepositoryMock.Object,
                _paymentRepositoryMock.Object,
                _transactionRepositoryMock.Object,
                _priceRepositoryMock.Object,
                _vehicleRepositoryMock.Object,
                _firebaseServiceMock.Object
            );
        }

        // CheckInAsync
        // Successful
        [Fact]
        public async Task CheckInAsync_ShouldReturnSuccess_WhenCheckInIsSuccessful()
        {
            // Arrange
            var req = new CreateSessionReqDto
            {
                CardNumber = "123456",
                PlateNumber = "ABC123",
                GateInId = Guid.NewGuid(),
                ImageIn = Mock.Of<IFormFile>(),
                ImageBodyIn = Mock.Of<IFormFile>()
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Status = CardStatusEnum.ACTIVE
            };

            var parkingArea = new ParkingArea
            {
                Id = Guid.NewGuid(),
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
                Name = "FPTU 1"
            };

            var vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                PlateNumber = req.PlateNumber,
                VehicleTypeId = Guid.NewGuid(),
                StatusVehicle = StatusVehicleEnum.ACTIVE
            };

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FCMToken = "fcm-token",
                StatusCustomer = StatusCustomerEnum.ACTIVE,
                Email = "customer@gmail.com",
                FullName = "Customer",
            };

            var imageInUrl = new UploadObjectReqDto
            {
                BucketName = BucketMinioEnum.BUCKET_IMAGE_VEHICLE,
                ObjFile = req.ImageIn,
                ObjName = Mock.Of<IFormFile>().Name
            };

            var imageBodyUrl = new UploadObjectReqDto
            {
                BucketName = BucketMinioEnum.BUCKET_IMAGE_BODY,
                ObjFile = req.ImageIn,
                ObjName = Mock.Of<IFormFile>().Name
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = card,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(req.GateInId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Gate
                    {
                        Name = "Gate 1",
                        StatusGate = StatusGateEnum.ACTIVE
                    }
                });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByGateIdAsync(req.GateInId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Data = parkingArea,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = customer,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Vehicle>
                {
                    IsSuccess = true,
                    Data = vehicle,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
                .ReturnsAsync(new Return<ReturnObjectUrlResDto>
                {
                    IsSuccess = true,
                    Data = new ReturnObjectUrlResDto
                    {
                        ObjUrl = "link",
                    },
                    Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY
                });


            _sessionRepositoryMock.Setup(x => x.CreateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY });

            _firebaseServiceMock.Setup(x => x.SendNotificationAsync(It.IsAny<FirebaseReqDto>()))
                .ReturnsAsync(new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                });

            // Act
            var result = await _sessionService.CheckInAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInAsync_ShouldReturnFailure_WhenCardDoesNotExist()
        {
            // Arrange
            var req = new CreateSessionReqDto { CardNumber = "123456" };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.CARD_NOT_EXIST,
                    Data = null
                });

            // Act
            var result = await _sessionService.CheckInAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInAsync_ShouldReturnFailure_WhenCardIsInactive()
        {
            // Arrange
            var req = new CreateSessionReqDto { CardNumber = "123456" };
            var inactiveCard = new Card
            {
                Status = CardStatusEnum.INACTIVE
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = inactiveCard
                });

            // Act
            var result = await _sessionService.CheckInAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_IS_INACTIVE, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInAsync_ShouldReturnFailure_WhenCardIsMissing()
        {
            // Arrange
            var req = new CreateSessionReqDto { CardNumber = "123456" };
            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Status = CardStatusEnum.MISSING
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = card,
                });

            // Act
            var result = await _sessionService.CheckInAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_IS_MISSING, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInAsync_ShouldReturnFailure_WhenCardIsAlreadyInSession()
        {
            // Arrange
            var req = new CreateSessionReqDto { PlateNumber = "ABC123" };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Status = CardStatusEnum.ACTIVE
            };

            var existingSession = new Session
            {
                Status = SessionEnum.PARKED,
                PlateNumber = req.PlateNumber,
                CardId = Guid.NewGuid(),
                GateInId = Guid.NewGuid(),
                TimeIn = DateTime.Now,
                Mode = "MODE1",
                Block = 30,
                ImageInUrl = "link",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = card,
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = existingSession,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CheckInAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_IS_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInAsync_ShouldReturnFailure_WhenPlateNumberIsAlreadyInSession()
        {
            // Arrange
            var req = new CreateSessionReqDto { PlateNumber = "ABC123" };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Status = CardStatusEnum.ACTIVE
            };

            var existingSession = new Session
            {
                Status = SessionEnum.PARKED,
                PlateNumber = req.PlateNumber,
                CardId = Guid.NewGuid(),
                GateInId = Guid.NewGuid(),
                TimeIn = DateTime.Now,
                Mode = "MODE1",
                Block = 30,
                ImageInUrl = "link",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = card,
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = existingSession,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CheckInAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PLATE_NUMBER_IN_USE, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInAsync_ShouldReturnFailure_WhenGateDoesNotExist()
        {
            // Arrange
            var req = new CreateSessionReqDto
            {
                GateInId = Guid.NewGuid()
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Status = CardStatusEnum.ACTIVE
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = card,
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(req.GateInId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            // Act
            var result = await _sessionService.CheckInAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.GATE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInAsync_ShouldReturnFailure_WhenParkingAreaIsInactive()
        {
            // Arrange
            var req = new CreateSessionReqDto { GateInId = Guid.NewGuid() };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Status = CardStatusEnum.ACTIVE
            };

            var parkingArea = new ParkingArea
            {
                Id = Guid.NewGuid(),
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.INACTIVE,
                Name = "FPTU 1"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = card,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(req.GateInId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Gate
                    {
                        Name = "Gate 1",
                        StatusGate = StatusGateEnum.ACTIVE
                    }
                });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByGateIdAsync(req.GateInId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Data = parkingArea,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CheckInAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PARKING_AREA_INACTIVE, result.Message);
        }


        // Failure
        [Fact]
        public async Task CheckInAsync_ShouldReturnVehicleNotExist_WhenVehicleDoesNotExist()
        {
            // Arrange
            var req = new CreateSessionReqDto { PlateNumber = "ABC123" };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Status = CardStatusEnum.ACTIVE
            };

            var parkingArea = new ParkingArea
            {
                Id = Guid.NewGuid(),
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
                Name = "FPTU 1"
            };

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FCMToken = "fcm-token",
                StatusCustomer = StatusCustomerEnum.ACTIVE,
                Email = "customer@gmail.com",
                FullName = "Customer",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = card,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(req.GateInId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Gate
                    {
                        Name = "Gate 1",
                        StatusGate = StatusGateEnum.ACTIVE
                    }
                });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByGateIdAsync(req.GateInId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Data = parkingArea,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = customer,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Vehicle>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                });

            // Act
            var result = await _sessionService.CheckInAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInAsync_ShouldReturnUploadImageFailed_WhenImageUploadFails()
        {
            // Arrange
            var req = new CreateSessionReqDto
            {
                CardNumber = "123456",
                PlateNumber = "ABC123",
                GateInId = Guid.NewGuid(),
                ImageIn = Mock.Of<IFormFile>(),
                ImageBodyIn = Mock.Of<IFormFile>()
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                PlateNumber = req.PlateNumber,
                VehicleTypeId = Guid.NewGuid(),
                StatusVehicle = StatusVehicleEnum.ACTIVE
            };

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Status = CardStatusEnum.ACTIVE
            };

            var parkingArea = new ParkingArea
            {
                Id = Guid.NewGuid(),
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
                Name = "FPTU 1"
            };

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FCMToken = "fcm-token",
                StatusCustomer = StatusCustomerEnum.ACTIVE,
                Email = "customer@gmail.com",
                FullName = "Customer",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = card,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(req.GateInId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Gate
                    {
                        Name = "Gate 1",
                        StatusGate = StatusGateEnum.ACTIVE
                    }
                });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByGateIdAsync(req.GateInId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Data = parkingArea,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = customer,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Vehicle>
                {
                    IsSuccess = true,
                    Data = vehicle,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
                .ReturnsAsync(new Return<ReturnObjectUrlResDto>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED,
                    Data = null
                });

            // Act
            var result = await _sessionService.CheckInAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.UPLOAD_IMAGE_FAILED, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInAsync_ShouldReturnFailure_WhenCheckInIsFail()
        {
            // Arrange
            var req = new CreateSessionReqDto
            {
                CardNumber = "123456",
                PlateNumber = "ABC123",
                GateInId = Guid.NewGuid(),
                ImageIn = Mock.Of<IFormFile>(),
                ImageBodyIn = Mock.Of<IFormFile>()
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Status = CardStatusEnum.ACTIVE
            };

            var parkingArea = new ParkingArea
            {
                Id = Guid.NewGuid(),
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
                Name = "FPTU 1"
            };

            var vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                PlateNumber = req.PlateNumber,
                VehicleTypeId = Guid.NewGuid(),
                StatusVehicle = StatusVehicleEnum.ACTIVE
            };

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FCMToken = "fcm-token",
                StatusCustomer = StatusCustomerEnum.ACTIVE,
                Email = "customer@gmail.com",
                FullName = "Customer",
            };

            var imageInUrl = new UploadObjectReqDto
            {
                BucketName = BucketMinioEnum.BUCKET_IMAGE_VEHICLE,
                ObjFile = req.ImageIn,
                ObjName = Mock.Of<IFormFile>().Name
            };

            var imageBodyUrl = new UploadObjectReqDto
            {
                BucketName = BucketMinioEnum.BUCKET_IMAGE_BODY,
                ObjFile = req.ImageIn,
                ObjName = Mock.Of<IFormFile>().Name
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = card,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(req.GateInId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Gate
                    {
                        Name = "Gate 1",
                        StatusGate = StatusGateEnum.ACTIVE
                    }
                });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByGateIdAsync(req.GateInId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Data = parkingArea,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = customer,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Vehicle>
                {
                    IsSuccess = true,
                    Data = vehicle,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
                .ReturnsAsync(new Return<ReturnObjectUrlResDto>
                {
                    IsSuccess = true,
                    Data = new ReturnObjectUrlResDto
                    {
                        ObjUrl = "link",
                    },
                    Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY
                });

            _sessionRepositoryMock.Setup(x => x.CreateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            _firebaseServiceMock.Setup(x => x.SendNotificationAsync(It.IsAny<FirebaseReqDto>()))
                .ReturnsAsync(new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                });

            // Act
            var result = await _sessionService.CheckInAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInAsync_ShouldReturnFailure_WhenInvalidUser()
        {
            // Arrange
            var req = new CreateSessionReqDto
            {
                CardNumber = "123456",
                PlateNumber = "ABC123",
                GateInId = Guid.NewGuid(),
                ImageIn = Mock.Of<IFormFile>(),
                ImageBodyIn = Mock.Of<IFormFile>()
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _sessionService.CheckInAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // CheckInForGuestAsync
        // Successful
        [Fact]
        public async Task CheckInForGuestAsync_ShouldReturnSuccess()
        {
            // Arrange
            var req = new CheckInForGuestReqDto
            {
                CardNumber = "123456789",
                PlateNumber = "99L999999",
                GateInId = Guid.NewGuid(),
                VehicleTypeId = Guid.NewGuid(),
                ImageIn = new FormFile(null, 0, 0, null, "plate.jpg"),
                ImageBody = new FormFile(null, 0, 0, null, "body.jpg")
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Status = CardStatusEnum.ACTIVE
            };

            var parkingArea = new ParkingArea
            {
                Id = Guid.NewGuid(),
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
                Name = "FPTU 1"
            };

            var vehicleType = new VehicleType
            {
                Id = Guid.NewGuid(),
                Name = "Car",
                StatusVehicleType = StatusVehicleType.ACTIVE
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = card,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(req.GateInId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Gate
                    {
                        Name = "Gate 1",
                        StatusGate = StatusGateEnum.ACTIVE
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = vehicleType,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByGateIdAsync(req.GateInId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Data = parkingArea,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
                .ReturnsAsync(new Return<ReturnObjectUrlResDto>
                {
                    IsSuccess = true,
                    Data = new ReturnObjectUrlResDto
                    {
                        ObjUrl = "link",
                    },
                    Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY
                });

            _sessionRepositoryMock.Setup(x => x.CreateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                });
            // Act
            var result = await _sessionService.CheckInForGuestAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInForGuestAsync_ShouldReturnFailure_WhenInvalidUser()
        {
            // Arrange
            var req = new CheckInForGuestReqDto
            {
                CardNumber = "123456789",
                PlateNumber = "99L999999",
                GateInId = Guid.NewGuid(),
                VehicleTypeId = Guid.NewGuid(),
                ImageIn = new FormFile(null, 0, 0, null, "plate.jpg"),
                ImageBody = new FormFile(null, 0, 0, null, "body.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _sessionService.CheckInForGuestAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInForGuestAsync_ShouldReturnFailure_WhenCardNotFound()
        {
            // Arrange
            var req = new CheckInForGuestReqDto
            {
                CardNumber = "123456789",
                PlateNumber = "99L999999",
                GateInId = Guid.NewGuid(),
                VehicleTypeId = Guid.NewGuid(),
                ImageIn = new FormFile(null, 0, 0, null, "plate.jpg"),
                ImageBody = new FormFile(null, 0, 0, null, "body.jpg")
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            // Act
            var result = await _sessionService.CheckInForGuestAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInForGuestAsync_ShouldReturnFailure_WhenCardAlreadyInUse()
        {
            // Arrange
            var req = new CheckInForGuestReqDto
            {
                CardNumber = "123456789",
                PlateNumber = "99L999999",
                GateInId = Guid.NewGuid(),
                VehicleTypeId = Guid.NewGuid(),
                ImageIn = new FormFile(null, 0, 0, null, "plate.jpg"),
                ImageBody = new FormFile(null, 0, 0, null, "body.jpg")
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = new Card(), Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        PlateNumber = req.PlateNumber,
                        CardId = Guid.NewGuid(),
                        GateInId = Guid.NewGuid(),
                        TimeIn = DateTime.Now,
                        Mode = "MODE1",
                        Block = 30,
                        ImageInUrl = "link",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CheckInForGuestAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_IS_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInForGuestAsync_ShouldReturnFailure_WhenPlateNumberInUse()
        {
            // Arrange
            var req = new CheckInForGuestReqDto
            {
                CardNumber = "123456789",
                PlateNumber = "99L999999",
                GateInId = Guid.NewGuid(),
                VehicleTypeId = Guid.NewGuid(),
                ImageIn = new FormFile(null, 0, 0, null, "plate.jpg"),
                ImageBody = new FormFile(null, 0, 0, null, "body.jpg")
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = new Card(), Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        PlateNumber = req.PlateNumber,
                        CardId = Guid.NewGuid(),
                        GateInId = Guid.NewGuid(),
                        TimeIn = DateTime.Now,
                        Mode = "MODE1",
                        Block = 30,
                        ImageInUrl = "link",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CheckInForGuestAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PLATE_NUMBER_IN_USE, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInForGuestAsync_ShouldReturnFailure_WhenGateNotFound()
        {
            // Arrange
            var req = new CheckInForGuestReqDto
            {
                CardNumber = "123456789",
                PlateNumber = "99L999999",
                GateInId = Guid.NewGuid(),
                VehicleTypeId = Guid.NewGuid(),
                ImageIn = new FormFile(null, 0, 0, null, "plate.jpg"),
                ImageBody = new FormFile(null, 0, 0, null, "body.jpg")
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = new Card(), Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(req.GateInId))
                .ReturnsAsync(new Return<Gate> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            // Act
            var result = await _sessionService.CheckInForGuestAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.GATE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInForGuestAsync_ShouldReturnFailure_WhenVehicleTypeNotFound()
        {
            // Arrange
            var req = new CheckInForGuestReqDto
            {
                CardNumber = "123456789",
                PlateNumber = "99L999999",
                GateInId = Guid.NewGuid(),
                VehicleTypeId = Guid.NewGuid(),
                ImageIn = new FormFile(null, 0, 0, null, "plate.jpg"),
                ImageBody = new FormFile(null, 0, 0, null, "body.jpg")
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var gate = new Gate
            {
                Id = Guid.NewGuid(),
                Name = "Gate 1",
                StatusGate = StatusGateEnum.ACTIVE
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = new Card(), Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(req.GateInId))
                .ReturnsAsync(new Return<Gate> { IsSuccess = true, Data = gate, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId))
                .ReturnsAsync(new Return<VehicleType> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            // Act
            var result = await _sessionService.CheckInForGuestAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInForGuestAsync_ShouldReturnFailure_WhenParkingAreaNotFound()
        {
            // Arrange
            var req = new CheckInForGuestReqDto
            {
                CardNumber = "123456789",
                PlateNumber = "99L999999",
                GateInId = Guid.NewGuid(),
                VehicleTypeId = Guid.NewGuid(),
                ImageIn = new FormFile(null, 0, 0, null, "plate.jpg"),
                ImageBody = new FormFile(null, 0, 0, null, "body.jpg")
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var gate = new Gate
            {
                Id = Guid.NewGuid(),
                Name = "Gate 1",
                StatusGate = StatusGateEnum.ACTIVE
            };

            var vehicleType = new VehicleType
            {
                Id = Guid.NewGuid(),
                Name = "Car",
                StatusVehicleType = StatusVehicleType.ACTIVE
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = new Card(), Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(req.GateInId))
                .ReturnsAsync(new Return<Gate> { IsSuccess = true, Data = gate, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId))
                .ReturnsAsync(new Return<VehicleType> { IsSuccess = true, Data = vehicleType, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByGateIdAsync(req.GateInId))
                .ReturnsAsync(new Return<ParkingArea> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            // Act
            var result = await _sessionService.CheckInForGuestAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PARKING_AREA_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInForGuestAsync_ShouldReturnFailure_WhenImageUploadFails()
        {
            // Arrange
            var req = new CheckInForGuestReqDto
            {
                CardNumber = "123456789",
                PlateNumber = "99L999999",
                GateInId = Guid.NewGuid(),
                VehicleTypeId = Guid.NewGuid(),
                ImageIn = new FormFile(null, 0, 0, null, "plate.jpg"),
                ImageBody = new FormFile(null, 0, 0, null, "body.jpg")
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var gate = new Gate
            {
                Id = Guid.NewGuid(),
                Name = "Gate 1",
                StatusGate = StatusGateEnum.ACTIVE
            };

            var vehicleType = new VehicleType
            {
                Id = Guid.NewGuid(),
                Name = "Car",
                StatusVehicleType = StatusVehicleType.ACTIVE
            };

            var parking = new ParkingArea
            {
                Id = Guid.NewGuid(),
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
                Name = "FPTU 1"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = new Card(), Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(req.GateInId))
                .ReturnsAsync(new Return<Gate> { IsSuccess = true, Data = gate, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId))
                .ReturnsAsync(new Return<VehicleType> { IsSuccess = true, Data = vehicleType, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByGateIdAsync(req.GateInId))
                .ReturnsAsync(new Return<ParkingArea> { IsSuccess = true, Data = parking, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
                .ReturnsAsync(new Return<ReturnObjectUrlResDto> { IsSuccess = false, Data = null, Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED });

            // Act
            var result = await _sessionService.CheckInForGuestAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.UPLOAD_IMAGE_FAILED, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckInForGuestAsync_ShouldReturnFailure_WhenSessionCreationFails()
        {
            // Arrange
            var req = new CheckInForGuestReqDto
            {
                CardNumber = "123456789",
                PlateNumber = "99L999999",
                GateInId = Guid.NewGuid(),
                VehicleTypeId = Guid.NewGuid(),
                ImageIn = new FormFile(null, 0, 0, null, "plate.jpg"),
                ImageBody = new FormFile(null, 0, 0, null, "body.jpg")
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            var gate = new Gate
            {
                Id = Guid.NewGuid(),
                Name = "Gate 1",
                StatusGate = StatusGateEnum.ACTIVE
            };

            var vehicleType = new VehicleType
            {
                Id = Guid.NewGuid(),
                Name = "Car",
                StatusVehicleType = StatusVehicleType.ACTIVE
            };

            var parking = new ParkingArea
            {
                Id = Guid.NewGuid(),
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
                Name = "FPTU 1"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = new Card(), Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(req.GateInId))
                .ReturnsAsync(new Return<Gate> { IsSuccess = true, Data = gate, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId))
                .ReturnsAsync(new Return<VehicleType> { IsSuccess = true, Data = vehicleType, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByGateIdAsync(req.GateInId))
                .ReturnsAsync(new Return<ParkingArea> { IsSuccess = true, Data = parking, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
                .ReturnsAsync(new Return<ReturnObjectUrlResDto> { IsSuccess = true, Data = new ReturnObjectUrlResDto { ObjUrl = "test-url" }, Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY });

            _sessionRepositoryMock.Setup(x => x.CreateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session> { IsSuccess = false, Message = ErrorEnumApplication.SERVER_ERROR });

            // Act
            var result = await _sessionService.CheckInForGuestAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdatePaymentSessionAsync
        // Successful
        [Fact]
        public async Task UpdatePaymentSessionAsync_ShouldReturnSuccess()
        {
            // Arrange
            string cardNumber = "123456789";

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Status = CardStatusEnum.ACTIVE,
                CardNumber = cardNumber
            };

            var session = new Session
            {
                Status = SessionEnum.PARKED,
                CardId = card.Id,
                Id = Guid.NewGuid(),
                TimeIn = DateTime.Now,
                TimeOut = DateTime.Now.AddHours(1),
                Mode = "MODE1",
                Block = 30,
                ImageInUrl = "link",
                ImageOutUrl = "link",
                PlateNumber = "99L999999"
            };

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDescription = "transaction",
                TransactionStatus = StatusTransactionEnum.SUCCEED,
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(cardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = card, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = session, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY });

            _transactionRepositoryMock.Setup(x => x.GetTransactionBySessionIdAsync(session.Id))
                .ReturnsAsync(new Return<Transaction> { IsSuccess = true, Data = transaction, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _transactionRepositoryMock.Setup(x => x.UpdateTransactionAsync(It.IsAny<Transaction>()))
                .ReturnsAsync(new Return<Transaction> { IsSuccess = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY });

            // Act
            var result = await _sessionService.UpdatePaymentSessionAsync(cardNumber);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePaymentSessionAsync_ShouldReturnFailure_WhenUserValidationFails()
        {
            // Arrange
            string cardNumber = "123456789";

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHENTICATION });

            // Act
            var result = await _sessionService.UpdatePaymentSessionAsync(cardNumber);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePaymentSessionAsync_ShouldReturnFailure_WhenCardNotFound()
        {
            // Arrange
            string cardNumber = "123456789";

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(cardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            // Act
            var result = await _sessionService.UpdatePaymentSessionAsync(cardNumber);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePaymentSessionAsync_ShouldReturnFailure_WhenSessionNotFound()
        {
            // Arrange
            string cardNumber = "123456789";
            var card = new Card { Id = Guid.NewGuid() };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(cardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = card, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            // Act
            var result = await _sessionService.UpdatePaymentSessionAsync(cardNumber);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePaymentSessionAsync_ShouldReturnFailure_WhenSessionNotParked()
        {
            // Arrange
            string cardNumber = "123456789";
            var card = new Card { Id = Guid.NewGuid() };

            var session = new Session
            {
                Status = SessionEnum.CLOSED,
                CardId = card.Id,
                Id = Guid.NewGuid(),
                TimeIn = DateTime.Now,
                TimeOut = DateTime.Now.AddHours(1),
                Mode = "MODE1",
                Block = 30,
                ImageInUrl = "link",
                ImageOutUrl = "link",
                PlateNumber = "99L999999"
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(cardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = card, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = session, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            // Act
            var result = await _sessionService.UpdatePaymentSessionAsync(cardNumber);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePaymentSessionAsync_ShouldReturnFailure_WhenSessionUpdateFails()
        {
            // Arrange
            string cardNumber = "123456789";

            var card = new Card { Id = Guid.NewGuid() };

            var session = new Session
            {
                Status = SessionEnum.PARKED,
                CardId = card.Id,
                Id = Guid.NewGuid(),
                TimeIn = DateTime.Now,
                TimeOut = DateTime.Now.AddHours(1),
                Mode = "MODE1",
                Block = 30,
                ImageInUrl = "link",
                ImageOutUrl = "link",
                PlateNumber = "99L999999"
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(cardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = card, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = session, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session> { IsSuccess = false, Message = ErrorEnumApplication.SERVER_ERROR });

            // Act
            var result = await _sessionService.UpdatePaymentSessionAsync(cardNumber);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePaymentSessionAsync_ShouldReturnFailure_WhenTransactionNotFound()
        {
            // Arrange
            string cardNumber = "123456789";

            var card = new Card { Id = Guid.NewGuid() };

            var session = new Session
            {
                Status = SessionEnum.PARKED,
                CardId = card.Id,
                Id = Guid.NewGuid(),
                TimeIn = DateTime.Now,
                TimeOut = DateTime.Now.AddHours(1),
                Mode = "MODE1",
                Block = 30,
                ImageInUrl = "link",
                ImageOutUrl = "link",
                PlateNumber = "99L999999"
            };

            var staff = new User
            {
                Id = Guid.NewGuid(),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = "",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = staff,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(cardNumber))
                .ReturnsAsync(new Return<Card> { IsSuccess = true, Data = card, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = session, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY });

            _transactionRepositoryMock.Setup(x => x.GetTransactionBySessionIdAsync(session.Id))
                .ReturnsAsync(new Return<Transaction> { IsSuccess = false, Data = null, Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            // Act
            var result = await _sessionService.UpdatePaymentSessionAsync(cardNumber);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // StatisticSessionAppAsync
        // Successful
        [Fact]
        public async Task StatisticSessionAppAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var expectedResult = new Return<IEnumerable<StatisticSessionAppResDto>>
            {
                IsSuccess = true,
                Data = new List<StatisticSessionAppResDto>
                {
                    new StatisticSessionAppResDto
                    {
                        Date = DateTime.Now,
                        TotalSession = 10,
                    }
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Id = Guid.NewGuid(),
                        StatusUser = StatusUserEnum.ACTIVE,
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.StatisticSessionAppAsync())
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sessionService.StatisticSessionAppAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task StatisticSessionAppAsync_ShouldReturnsFailure_WhenAuthenticationFails()
        {
            // Arrange

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                    IsSuccess = false,
                    Data = null
                });

            // Act
            var result = await _sessionService.StatisticSessionAppAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task StatisticSessionAppAsync_ShouldReturnsFailure_WhenServerError()
        {
            // Arrange
            var expectedResult = new Return<IEnumerable<StatisticSessionAppResDto>>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.SERVER_ERROR,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Id = Guid.NewGuid(),
                        StatusUser = StatusUserEnum.ACTIVE,
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.StatisticSessionAppAsync())
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sessionService.StatisticSessionAppAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetListSessionByUserAsync
        // Successful
        [Fact]
        public async Task GetListSessionByUserAsync_ShouldReturnSuccess_WhenRecordFound()
        {
            // Arrange
            var req = new GetListObjectWithFillerAttributeAndDateReqDto();
            var listSession = new List<Session>
            {
                new Session
                {
                    Card = new Card { CardNumber = "123" },
                    Mode = "Test",
                    ImageOutUrl = "out.jpg",
                    ImageInUrl = "in.jpg",
                    GateOut = new Gate
                    {
                        Name = "Out",
                        ParkingArea = new ParkingArea
                        {
                            Name = "Area",
                            Block = 30,
                            Mode = "MODE1",
                            StatusParkingArea = StatusParkingEnum.ACTIVE
                        },
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    GateIn = new Gate
                    {
                        Name = "In",
                        ParkingArea = new ParkingArea
                        {
                            Name = "Area",
                            Block = 30,
                            Mode = "MODE1",
                            StatusParkingArea = StatusParkingEnum.ACTIVE
                        },
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    PlateNumber = "ABC123",
                    TimeIn = DateTime.Now,
                    Customer = new Customer
                    {
                        Email = "customer@test.com",
                        FullName = "Customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    PaymentMethod = new PaymentMethod { Name = "Cash" },
                    Status = SessionEnum.CLOSED,
                    TimeOut = DateTime.Now.AddHours(1),
                    VehicleType = new VehicleType { Name = "Car" },
                    Block = 30
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@test.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionAsync(req))
                .ReturnsAsync(new Return<IEnumerable<Session>>
                {
                    IsSuccess = true,
                    Data = listSession,
                    TotalRecord = 1,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.GetListSessionByUserAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListSessionByUserAsync_ShouldReturnsFailure_WhenAuthenticationFails()
        {
            // Arrange
            var req = new GetListObjectWithFillerAttributeAndDateReqDto();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                    IsSuccess = false,
                    Data = null
                });

            // Act
            var result = await _sessionService.GetListSessionByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListSessionByUserAsync_ShouldReturnsFailure_WhenServerError()
        {
            // Arrange
            var req = new GetListObjectWithFillerAttributeAndDateReqDto();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@test.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionAsync(req))
                .ReturnsAsync(new Return<IEnumerable<Session>>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _sessionService.GetListSessionByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetListSessionByUserAsync_ShouldReturnSuccess_WhenRecordNotFound()
        {
            // Arrange
            var req = new GetListObjectWithFillerAttributeAndDateReqDto();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@test.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionAsync(req))
                .ReturnsAsync(new Return<IEnumerable<Session>>
                {
                    IsSuccess = true,
                    Data = null,
                    TotalRecord = 0,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.GetListSessionByUserAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // GetSessionBySessionIdAsync
        // Successful
        [Fact]
        public async Task GetSessionBySessionIdAsync_SuccessfulExecution()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var mockSession = new Session
            {
                Card = new Card { CardNumber = "123" },
                Mode = "Test",
                ImageOutUrl = "out.jpg",
                ImageInUrl = "in.jpg",
                GateOut = new Gate
                {
                    Name = "Out",
                    ParkingArea = new ParkingArea
                    {
                        Name = "Area",
                        Block = 30,
                        Mode = "MODE1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE
                    },
                    StatusGate = StatusGateEnum.ACTIVE
                },
                GateIn = new Gate
                {
                    Name = "In",
                    ParkingArea = new ParkingArea
                    {
                        Name = "Area",
                        Block = 30,
                        Mode = "MODE1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE
                    },
                    StatusGate = StatusGateEnum.ACTIVE
                },
                PlateNumber = "ABC123",
                TimeIn = DateTime.Now,
                Customer = new Customer
                {
                    Email = "customer@test.com",
                    FullName = "Customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                },
                PaymentMethod = new PaymentMethod { Name = "Cash" },
                Status = SessionEnum.CLOSED,
                TimeOut = DateTime.Now.AddHours(1),
                VehicleType = new VehicleType { Name = "Car" },
                Block = 30
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@test.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = mockSession,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.GetSessionBySessionIdAsync(sessionId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetSessionBySessionIdAsync_ShouldReturnsFailure_WhenAuthenticationFails()
        {
            // Arrange
            var sessionId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _sessionService.GetSessionBySessionIdAsync(sessionId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetSessionBySessionIdAsync_ShouldReturnsFailure_WhenServerError()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
            .ReturnsAsync(new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    Email = "user@test.com",
                    FullName = "User",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            });

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(new Return<Session>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                    Data = null
                });

            // Act
            var result = await _sessionService.GetSessionBySessionIdAsync(sessionId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CancleSessionByIdAsync
        // Successful
        [Fact]
        public async Task CancleSessionByIdAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var sessionId = Guid.NewGuid();

            var mockSession = new Session
            {
                Card = new Card { CardNumber = "123" },
                Mode = "Test",
                ImageOutUrl = "out.jpg",
                ImageInUrl = "in.jpg",
                GateOut = new Gate
                {
                    Name = "Out",
                    ParkingArea = new ParkingArea
                    {
                        Name = "Area",
                        Block = 30,
                        Mode = "MODE1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE
                    },
                    StatusGate = StatusGateEnum.ACTIVE
                },
                GateIn = new Gate
                {
                    Name = "In",
                    ParkingArea = new ParkingArea
                    {
                        Name = "Area",
                        Block = 30,
                        Mode = "MODE1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE
                    },
                    StatusGate = StatusGateEnum.ACTIVE
                },
                PlateNumber = "ABC123",
                TimeIn = DateTime.Now,
                Customer = new Customer
                {
                    Email = "customer@test.com",
                    FullName = "Customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                },
                PaymentMethod = new PaymentMethod { Name = "Cash" },
                Status = SessionEnum.PARKED,
                TimeOut = DateTime.Now.AddHours(1),
                VehicleType = new VehicleType { Name = "Car" },
                Block = 30
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@test.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = mockSession, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Status = SessionEnum.CANCELLED,
                        Block = 30,
                        Mode = "MODE1",
                        ImageInUrl = "in.jpg",
                        PlateNumber = "99L999999",
                        TimeIn = DateTime.Now,
                    },
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            // Act
            var result = await _sessionService.CancleSessionByIdAsync(sessionId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CancleSessionByIdAsync_ShouldReturnFailure_WhenUpdateFail()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var mockSession = new Session
            {
                Card = new Card { CardNumber = "123" },
                Mode = "Test",
                ImageOutUrl = "out.jpg",
                ImageInUrl = "in.jpg",
                GateOut = new Gate
                {
                    Name = "Out",
                    ParkingArea = new ParkingArea
                    {
                        Name = "Area",
                        Block = 30,
                        Mode = "MODE1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE
                    },
                    StatusGate = StatusGateEnum.ACTIVE
                },
                GateIn = new Gate
                {
                    Name = "In",
                    ParkingArea = new ParkingArea
                    {
                        Name = "Area",
                        Block = 30,
                        Mode = "MODE1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE
                    },
                    StatusGate = StatusGateEnum.ACTIVE
                },
                PlateNumber = "ABC123",
                TimeIn = DateTime.Now,
                Customer = new Customer
                {
                    Email = "customer@test.com",
                    FullName = "Customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                },
                PaymentMethod = new PaymentMethod { Name = "Cash" },
                Status = SessionEnum.PARKED,
                TimeOut = DateTime.Now.AddHours(1),
                VehicleType = new VehicleType { Name = "Car" },
                Block = 30
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@test.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(new Return<Session> { IsSuccess = true, Data = mockSession, Message = SuccessfullyEnumServer.FOUND_OBJECT });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _sessionService.CancleSessionByIdAsync(sessionId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CancleSessionByIdAsync_ShouldReturnsFailure_WhenAuthenticationFails()
        {
            // Arrange
            var sessionId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _sessionService.CancleSessionByIdAsync(sessionId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CancleSessionByIdAsync_ShouldReturnsFailure_WhenSessionNotParked()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var mockSession = new Session
            {
                Card = new Card { CardNumber = "123" },
                Mode = "Test",
                ImageOutUrl = "out.jpg",
                ImageInUrl = "in.jpg",
                GateOut = new Gate
                {
                    Name = "Out",
                    ParkingArea = new ParkingArea
                    {
                        Name = "Area",
                        Block = 30,
                        Mode = "MODE1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE
                    },
                    StatusGate = StatusGateEnum.ACTIVE
                },
                GateIn = new Gate
                {
                    Name = "In",
                    ParkingArea = new ParkingArea
                    {
                        Name = "Area",
                        Block = 30,
                        Mode = "MODE1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE
                    },
                    StatusGate = StatusGateEnum.ACTIVE
                },
                PlateNumber = "ABC123",
                TimeIn = DateTime.Now,
                Customer = new Customer
                {
                    Email = "customer@test.com",
                    FullName = "Customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                },
                PaymentMethod = new PaymentMethod { Name = "Cash" },
                Status = SessionEnum.CLOSED,
                TimeOut = DateTime.Now.AddHours(1),
                VehicleType = new VehicleType { Name = "Car" },
                Block = 30
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@test.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = mockSession,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CancleSessionByIdAsync(sessionId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHORITY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CancleSessionByIdAsync_ShouldReturnsFailure_WhenSessionNotFound()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@test.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CancleSessionByIdAsync(sessionId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // GetTotalSessionParkedAsync
        // Successful
        [Fact]
        public async Task GetTotalSessionParkedAsync_SuccessfulExecution_ReturnsSuccessResult()
        {
            // Arrange
            int expectedTotal = 10;

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@test.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetTotalSessionParkedAsync())
                .ReturnsAsync(new Return<int>
                {
                    IsSuccess = true,
                    Data = expectedTotal,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.GetTotalSessionParkedAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetTotalSessionParkedAsync_ShouldReturnsFailure_WhenAuthenticationFails()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _sessionService.GetTotalSessionParkedAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetTotalSessionParkedAsync_ShouldReturnsFailure_WhenServerError()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@test.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetTotalSessionParkedAsync())
                .ReturnsAsync(new Return<int>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _sessionService.GetTotalSessionParkedAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetAverageSessionDurationPerDayAsync
        // Successful
        [Fact]
        public async Task GetAverageSessionDurationPerDayAsync_ShouldReturnsSuccess()
        {
            // Arrange
            double expectedAverage = 120.5;

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@test.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetAverageSessionDurationPerDayAsync())
                .ReturnsAsync(new Return<double>
                {
                    IsSuccess = true,
                    Data = expectedAverage,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.GetAverageSessionDurationPerDayAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAverageSessionDurationPerDayAsync_ShouldReturnsFailure_WhenAuthenticationFails()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _sessionService.GetAverageSessionDurationPerDayAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // GetStatisticCheckInCheckOutAsync
        // Successful
        [Fact]
        public async Task GetStatisticCheckInCheckOutAsync_ShouldReturnSuccess()
        {
            // Arrange
            var expectedData = new StatisticCheckInCheckOutResDto
            {
                TotalCheckInToday = 10,
                TotalCheckOutToday = 10,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@test.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetStatisticCheckInCheckOutAsync())
                .ReturnsAsync(new Return<StatisticCheckInCheckOutResDto>
                {
                    IsSuccess = true,
                    Data = expectedData,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                });

            // Act
            var result = await _sessionService.GetStatisticCheckInCheckOutAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetStatisticCheckInCheckOutAsync_ShouuldReturnFailure_WhenAuthenticationFailure()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _sessionService.GetStatisticCheckInCheckOutAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetStatisticCheckInCheckOutAsync_ShouuldReturnFailure_WhenGetFailure()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@test.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetStatisticCheckInCheckOutAsync())
                .ReturnsAsync(new Return<StatisticCheckInCheckOutResDto>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _sessionService.GetStatisticCheckInCheckOutAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetStatisticCheckInCheckOutInParkingAreaAsync
        // Successful
        [Fact]
        public async Task GetStatisticCheckInCheckOutInParkingAreaAsync_ShouldReturnSuccess()
        {
            // Arrange
            var parkingId = Guid.NewGuid();
            var expectedData = new StatisticSessionTodayResDto
            {
                TotalCheckInToday = 10,
                TotalCheckOutToday = 10,
                TotalLot = 100,
                TotalVehicleParked = 20,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetStatisticCheckInCheckOutInParkingAreaAsync(parkingId))
                .ReturnsAsync(new Return<StatisticSessionTodayResDto>
                {
                    IsSuccess = true,
                    Data = expectedData,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                });

            // Act
            var result = await _sessionService.GetStatisticCheckInCheckOutInParkingAreaAsync(parkingId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetStatisticCheckInCheckOutInParkingAreaAsync_ShouldReturnFailure_WhenAuthenticationFailure()
        {
            // Arrange
            var parkingId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                });

            // Act
            var result = await _sessionService.GetStatisticCheckInCheckOutInParkingAreaAsync(parkingId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        [Fact]
        public async Task GetStatisticCheckInCheckOutInParkingAreaAsync_ShouldReturnFailure_WhenGetFail()
        {
            // Arrange
            var parkingId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetStatisticCheckInCheckOutInParkingAreaAsync(parkingId))
                .ReturnsAsync(new Return<StatisticSessionTodayResDto>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _sessionService.GetStatisticCheckInCheckOutInParkingAreaAsync(parkingId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetAllSessionByCardNumberAndPlateNumberAsync
        // Successful
        [Fact]
        public async Task GetAllSessionByCardNumberAndPlateNumberAsync_ShouldReturnSuccess()
        {
            // Arrange
            var parkingId = Guid.NewGuid();
            var pageSize = 10;
            var pageIndex = 1;

            var expectedData = new List<Session>
            {
                new Session
                {
                    Id = Guid.NewGuid(),
                    Block = 30,
                    Mode = "MODE1",
                    ImageInUrl = "in.jpg",
                    PlateNumber = "99L999999",
                    TimeIn = DateTime.Now,
                    Status = SessionEnum.CLOSED,
                    Card = new Card { CardNumber = "12345" },
                    ImageInBodyUrl = "in_body.jpg",
                    ImageOutBodyUrl = "out_body.jpg"
                }
            };

            _helpperServiceMock
                .Setup(h => h.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock
                .Setup(r => r.GetAllSessionByCardNumberAndPlateNumberAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(new Return<IEnumerable<Session>>
                {
                    IsSuccess = true,
                    Data = expectedData,
                    TotalRecord = 1,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.GetAllSessionByCardNumberAndPlateNumberAsync(parkingId, null, null, null, pageIndex, pageSize, null, null);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllSessionByCardNumberAndPlateNumberAsync_ShouldReturnFailure_WhenAuthenticationFailure()
        {
            // Arrange
            var parkingId = Guid.NewGuid();
            var pageSize = 10;
            var pageIndex = 1;

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                });

            // Act
            var result = await _sessionService.GetAllSessionByCardNumberAndPlateNumberAsync(parkingId, null, null, null, pageIndex, pageSize, null, null);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllSessionByCardNumberAndPlateNumberAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var parkingId = Guid.NewGuid();
            var pageSize = 10;
            var pageIndex = 1;

            _helpperServiceMock
                .Setup(h => h.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock
                .Setup(r => r.GetAllSessionByCardNumberAndPlateNumberAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(new Return<IEnumerable<Session>>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _sessionService.GetAllSessionByCardNumberAndPlateNumberAsync(parkingId, null, null, null, pageIndex, pageSize, null, null);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetNewestSessionByCardNumberAsync
        // Successful
        [Fact]
        public async Task GetNewestSessionByCardNumberAsync_ShouldReturnsSuccess()
        {
            // Arrange
            string cardNumber = "123456789";
            DateTime timeOut = DateTime.Now.AddHours(2);

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardNumberAsync(cardNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.Now,
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        ImageInUrl = "in.jpg",
                        PlateNumber = "99L999999",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                        StatusVehicleType = StatusVehicleType.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                        new PriceTable
                        {
                            Name = "PriceTable",
                            Priority = 1,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                        new PriceItem
                        {
                            BlockPricing = 1000,
                            MinPrice = 1000,
                            MaxPrice = 10000
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.GetNewestSessionByCardNumberAsync(cardNumber, timeOut);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByCardNumberAsync_ShouldReturnsFailure_WhenInvalidUser()
        {
            // Arrange
            string cardNumber = "123456789";
            DateTime timeOut = DateTime.Now.AddHours(2);

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _sessionService.GetNewestSessionByCardNumberAsync(cardNumber, timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByCardNumberAsync_ShouldReturnsFailure_WhenSessionNotExist()
        {
            // Arrange
            string cardNumber = "123456789";
            DateTime timeOut = DateTime.Now.AddHours(2);

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardNumberAsync(cardNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_SESSION_WITH_PLATE_NUMBER
                });

            // Act
            var result = await _sessionService.GetNewestSessionByCardNumberAsync(cardNumber, timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_SESSION_WITH_PLATE_NUMBER, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByCardNumberAsync_ShouldReturnsFailure_WhenSessionClosed()
        {
            // Arrange
            string cardNumber = "123456789";
            DateTime timeOut = DateTime.Now.AddHours(2);

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardNumberAsync(cardNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Status = SessionEnum.CLOSED,
                        TimeIn = DateTime.Now,
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        ImageInUrl = "in.jpg",
                        PlateNumber = "99L999999",
                    },
                    Message = ErrorEnumApplication.SESSION_CLOSE
                });

            // Act
            var result = await _sessionService.GetNewestSessionByCardNumberAsync(cardNumber, timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SESSION_CLOSE, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByCardNumberAsync_ShouldReturnsFailure_WhenSessionCancelled()
        {
            // Arrange
            string cardNumber = "123456789";
            DateTime timeOut = DateTime.Now.AddHours(2);

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardNumberAsync(cardNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Status = SessionEnum.CANCELLED,
                        TimeIn = DateTime.Now,
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        ImageInUrl = "in.jpg",
                        PlateNumber = "99L999999",
                    },
                    Message = ErrorEnumApplication.SESSION_CANCELLED
                });

            // Act
            var result = await _sessionService.GetNewestSessionByCardNumberAsync(cardNumber, timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SESSION_CANCELLED, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByCardNumberAsync_ShouldReturnsFailure_When()
        {
            // Arrange
            string cardNumber = "123456789";
            DateTime timeOut = DateTime.Now.AddHours(2);

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardNumberAsync(cardNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.Now,
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        ImageInUrl = "in.jpg",
                        PlateNumber = "99L999999",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.GetNewestSessionByCardNumberAsync(cardNumber, timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByCardNumberAsync_ShouldReturnsFailure_WhenTimeOutLessThanTimeIn()
        {
            // Arrange
            string cardNumber = "123456";
            DateTime timeIn = DateTime.Now;
            DateTime timeOut = timeIn.AddHours(-1);

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardNumberAsync(cardNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        TimeIn = timeIn,
                        TimeOut = timeOut,
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        ImageInUrl = "in.jpg",
                        PlateNumber = "99L999999",
                    },
                    Message = ErrorEnumApplication.TIME_OUT_IS_MUST_BE_GREATER_TIME_IN
                });

            // Act
            var result = await _sessionService.GetNewestSessionByCardNumberAsync(cardNumber, timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.TIME_OUT_IS_MUST_BE_GREATER_TIME_IN, result.Message);
        }

        // UpdatePlateNumberInSessionAsync
        // Successful
        [Fact]
        public async Task UpdatePlateNumberInSessionAsync_ShouldReturnSuccess()
        {
            // Arrange
            var req = new UpdatePlateNumberInSessionReqDto
            {
                SessionId = Guid.NewGuid(),
                PlateNumber = "99L999999"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(req.SessionId))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Id = req.SessionId,
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.Now,
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        ImageInUrl = "in.jpg",
                        PlateNumber = "99L999999",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Vehicle>
                {
                    IsSuccess = true,
                    Data = new Vehicle
                    {
                        StatusVehicle = StatusVehicleEnum.ACTIVE,
                        PlateNumber = req.PlateNumber,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        StatusCustomer = StatusCustomerEnum.ACTIVE,
                        FullName = "Customer",
                        Email = "customer@gmail.com"
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Id = req.SessionId,
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.Now,
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        ImageInUrl = "in.jpg",
                        PlateNumber = req.PlateNumber,
                    },
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            // Act
            var result = await _sessionService.UpdatePlateNumberInSessionAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePlateNumberInSessionAsync_ShouldReturnFailure_WhenInvalidAuth()
        {
            // Arrange
            var req = new UpdatePlateNumberInSessionReqDto
            {
                SessionId = Guid.NewGuid(),
                PlateNumber = "ABC123"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _sessionService.UpdatePlateNumberInSessionAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePlateNumberInSessionAsync_ShouldReturnFailure_WhenSessionNotFound()
        {
            // Arrange
            var req = new UpdatePlateNumberInSessionReqDto
            {
                SessionId = Guid.NewGuid(),
                PlateNumber = "ABC123"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(req.SessionId))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.UpdatePlateNumberInSessionAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePlateNumberInSessionAsync_ShouldReturnFailure_WhenSessionClosed()
        {
            // Arrange
            var req = new UpdatePlateNumberInSessionReqDto
            {
                SessionId = Guid.NewGuid(),
                PlateNumber = "99L999999"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(req.SessionId))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Id = req.SessionId,
                        Status = SessionEnum.CLOSED,
                        TimeIn = DateTime.Now,
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        ImageInUrl = "in.jpg",
                        PlateNumber = "99L999999",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.UpdatePlateNumberInSessionAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SESSION_CLOSE, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePlateNumberInSessionAsync_ShouldReturnFailure_WhenPlateNumberBelongsToAnotherSession()
        {
            // Arrange
            var req = new UpdatePlateNumberInSessionReqDto
            {
                SessionId = Guid.NewGuid(),
                PlateNumber = "99L999999"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(req.SessionId))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Id = req.SessionId,
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.Now,
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        ImageInUrl = "in.jpg",
                        PlateNumber = "99L999999",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session>
                {
                    Data = new Session
                    {
                        Id = Guid.NewGuid(),
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.Now,
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        ImageInUrl = "in.jpg",
                        PlateNumber = "99L999999",
                    },
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.UpdatePlateNumberInSessionAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PLATE_NUMBER_IS_BELONG_TO_ANOTHER_SESSION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePlateNumberInSessionAsync_ShouldReturnFailure_WhenVehiclePending()
        {
            // Arrange
            var req = new UpdatePlateNumberInSessionReqDto
            {
                SessionId = Guid.NewGuid(),
                PlateNumber = "ABC123"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(req.SessionId))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Id = req.SessionId,
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.Now,
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        ImageInUrl = "in.jpg",
                        PlateNumber = "99L999999",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Vehicle>
                {
                    IsSuccess = true,
                    Data = new Vehicle
                    {
                        StatusVehicle = StatusVehicleEnum.PENDING,
                        PlateNumber = req.PlateNumber,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.UpdatePlateNumberInSessionAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_IS_PENDING, result.Message);
        }

        // GetCustomerTypeByPlateNumberAsync
        // Successful
        [Fact]
        public async Task GetCustomerTypeByPlateNumberAsync_ShouldReturnSuccess_WhenRegistered()
        {
            // Arrange
            var req = new GetCheckInInformationReqDto
            {
                PlateNumber = "51F123456",
                CardNumber = "123456789"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        Status = CardStatusEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Vehicle>
                {
                    IsSuccess = true,
                    Data = new Vehicle
                    {
                        PlateNumber = "99L99999",
                        StatusVehicle = StatusVehicleEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        StatusCustomer = StatusCustomerEnum.ACTIVE,
                        CustomerType = new CustomerType
                        {
                            Name = CustomerTypeEnum.PAID,
                            Description = "Paid",
                        },
                        FullName = "Customer",
                        Email = "customer@gmail.com"
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.GetCustomerTypeByPlateNumberAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetCustomerTypeByPlateNumberAsync_ShouldReturnSuccess_WhenGuestCustomer()
        {
            // Arrange
            var req = new GetCheckInInformationReqDto
            {
                PlateNumber = "51F123456",
                CardNumber = "123456789"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        Status = CardStatusEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(new Return<Vehicle>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.GetCustomerTypeByPlateNumberAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CustomerTypeEnum.GUEST, result.Data.CustomerType);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetCustomerTypeByPlateNumberAsync_ShouldReturnSuccess_WhenInvalidAuth()
        {
            // Arrange
            var req = new GetCheckInInformationReqDto
            {
                PlateNumber = "51F12345",
                CardNumber = "CARD123"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _sessionService.GetCustomerTypeByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetCustomerTypeByPlateNumberAsync_ShouldReturnSuccess_WhenInvalidPlateNumber()
        {
            // Arrange
            var req = new GetCheckInInformationReqDto
            {
                PlateNumber = "INVALID",
                CardNumber = "123456789"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.GetCustomerTypeByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_A_PLATE_NUMBER, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetCustomerTypeByPlateNumberAsync_ShouldReturnSuccess_WhenCardNotExist()
        {
            // Arrange
            var req = new GetCheckInInformationReqDto
            {
                PlateNumber = "51F12345",
                CardNumber = "123456789"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(req.CardNumber))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.CARD_NOT_EXIST
                });

            // Act
            var result = await _sessionService.GetCustomerTypeByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_NOT_EXIST, result.Message);
        }

        // GetListSessionByCustomerAsync
        // Failure
        [Fact]
        public async Task GetListSessionByCustomerAsync_ShouldReturnFailure_WhenCustomerInvalid()
        {
            // Arrange
            var req = new GetListObjectWithFillerDateReqDto();

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _sessionService.GetListSessionByCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetListSessionByCustomerAsync_ShouldReturnSuccess_WhenRecordFound()
        {
            // Arrange
            var req = new GetListObjectWithFillerDateReqDto();

            var sessions = new List<Session>
            {
                new Session
                {
                    Id = Guid.NewGuid(),
                    Status = SessionEnum.PARKED,
                    TimeIn = DateTime.UtcNow,
                    Block = 60,
                    ImageInUrl = "in.jpg",
                    Mode = ModeEnum.MODE1,
                    PlateNumber = "99L999999",
                    VehicleTypeId = Guid.NewGuid()
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = Guid.NewGuid(),
                        Email = "customer@gmail.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionByCustomerIdAsync(It.IsAny<Guid>(), null, null, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new Return<IEnumerable<Session>>
                {
                    IsSuccess = true,
                    Data = sessions,
                    TotalRecord = 1,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentBySessionIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Payment>
                {
                    IsSuccess = true,
                    Data = new Payment { TotalPrice = 5000 },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                        new PriceTable
                        {
                            Name = "Price Table",
                            Priority = 2,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    }
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                        new PriceItem
                        {
                            ApplyFromHour = 0,
                            ApplyToHour = 23,
                            BlockPricing = 1000,
                            MinPrice = 1000,
                            MaxPrice = 10000
                        }
                    }
                });

            // Act
            var result = await _sessionService.GetListSessionByCustomerAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.TotalRecord);
        }

        // Failure
        [Fact]
        public async Task GetListSessionByCustomerAsync_ShouldReturnSuccess_WhenRecordNotFound()
        {
            // Arrange
            var req = new GetListObjectWithFillerDateReqDto();

            var sessions = new List<Session>
            {
                new Session
                {
                    Id = Guid.NewGuid(),
                    Status = SessionEnum.PARKED,
                    TimeIn = DateTime.UtcNow,
                    Block = 60,
                    ImageInUrl = "in.jpg",
                    Mode = ModeEnum.MODE1,
                    PlateNumber = "99L999999",
                    VehicleTypeId = Guid.NewGuid()
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = Guid.NewGuid(),
                        Email = "customer@gmail.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionByCustomerIdAsync(It.IsAny<Guid>(), null, null, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new Return<IEnumerable<Session>>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.GetListSessionByCustomerAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListSessionByCustomerAsync_ShouldReturnFailure_WhenPaymentFetchFails()
        {
            // Arrange
            var req = new GetListObjectWithFillerDateReqDto();

            var sessions = new List<Session>
            {
                new Session
                {
                    Id = Guid.NewGuid(),
                    Status = SessionEnum.PARKED,
                    TimeIn = DateTime.UtcNow,
                    Block = 60,
                    ImageInUrl = "in.jpg",
                    Mode = ModeEnum.MODE1,
                    PlateNumber = "99L999999",
                    VehicleTypeId = Guid.NewGuid()
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = Guid.NewGuid(),
                        Email = "customer@gmail.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionByCustomerIdAsync(It.IsAny<Guid>(), null, null, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new Return<IEnumerable<Session>>
                {
                    IsSuccess = true,
                    Data = sessions,
                    TotalRecord = 1,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentBySessionIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Payment>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _sessionService.GetListSessionByCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // 
        [Fact]
        public async Task GetListSessionByCustomerAsync_ShouldReturnFailure_WhenGetVehicleTypeFail()
        {
            // Arrange
            var req = new GetListObjectWithFillerDateReqDto();

            var sessions = new List<Session>
            {
                new Session
                {
                    Id = Guid.NewGuid(),
                    Status = SessionEnum.PARKED,
                    TimeIn = DateTime.UtcNow,
                    Block = 60,
                    ImageInUrl = "in.jpg",
                    Mode = ModeEnum.MODE1,
                    PlateNumber = "99L999999",
                    VehicleTypeId = Guid.NewGuid()
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = Guid.NewGuid(),
                        Email = "customer@gmail.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionByCustomerIdAsync(It.IsAny<Guid>(), null, null, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new Return<IEnumerable<Session>>
                {
                    IsSuccess = true,
                    Data = sessions,
                    TotalRecord = 1,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentBySessionIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Payment>
                {
                    IsSuccess = true,
                    Data = new Payment { TotalPrice = 5000 },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _sessionService.GetListSessionByCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListSessionByCustomerAsync_ShouldReturnFailure_WhenPriceTableFetchFails()
        {
            // Arrange
            var req = new GetListObjectWithFillerDateReqDto();

            var sessions = new List<Session>
            {
                new Session
                {
                    Id = Guid.NewGuid(),
                    Status = SessionEnum.PARKED,
                    TimeIn = DateTime.UtcNow,
                    Block = 60,
                    ImageInUrl = "in.jpg",
                    Mode = ModeEnum.MODE1,
                    PlateNumber = "99L999999",
                    VehicleTypeId = Guid.NewGuid()
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = Guid.NewGuid(),
                        Email = "customer@gmail.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionByCustomerIdAsync(It.IsAny<Guid>(), null, null, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new Return<IEnumerable<Session>>
                {
                    IsSuccess = true,
                    Data = sessions,
                    TotalRecord = 1,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentBySessionIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Payment>
                {
                    IsSuccess = true,
                    Data = new Payment { TotalPrice = 5000 },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                });

            // Act
            var result = await _sessionService.GetListSessionByCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListSessionByCustomerAsync_ShouldReturnFailure_WhenPriceItemsFetchFails()
        {
            // Arrange
            var req = new GetListObjectWithFillerDateReqDto();

            var sessions = new List<Session>
            {
                new Session
                {
                    Id = Guid.NewGuid(),
                    Status = SessionEnum.PARKED,
                    TimeIn = DateTime.UtcNow,
                    Block = 60,
                    ImageInUrl = "in.jpg",
                    Mode = ModeEnum.MODE1,
                    PlateNumber = "99L999999",
                    VehicleTypeId = Guid.NewGuid()
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = Guid.NewGuid(),
                        Email = "customer@gmail.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionByCustomerIdAsync(It.IsAny<Guid>(), null, null, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new Return<IEnumerable<Session>>
                {
                    IsSuccess = true,
                    Data = sessions,
                    TotalRecord = 1,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentBySessionIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Payment>
                {
                    IsSuccess = true,
                    Data = new Payment { TotalPrice = 5000 },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                        new PriceTable
                        {
                            Name = "Price Table",
                            Priority = 2,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    }
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                });

            // Act
            var result = await _sessionService.GetListSessionByCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetNewestSessionByPlateNumberAsync
        // Successful
        [Fact]
        public async Task GetNewestSessionByPlateNumberAsync_ShouldReturnSuccess_WhenSessionStatusIsParked()
        {
            // Arrange
            var timeIn = DateTime.Now.AddHours(-2);
            var timeOut = DateTime.Now;
            var session = new Session
            {
                Status = SessionEnum.PARKED,
                TimeIn = timeIn,
                Mode = ModeEnum.MODE1,
                Block = 60,
                VehicleTypeId = Guid.NewGuid(),
                ImageInUrl = "in.jpg",
                PlateNumber = "99L999999",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = session,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    { new PriceTable
                        {
                            Name = "Price Table",
                            Priority = 2,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                        new PriceItem
                        {
                            BlockPricing = 1000,
                            MinPrice = 1000,
                            MaxPrice = 10000
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.GetNewestSessionByPlateNumberAsync("99L999999", timeOut);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByPlateNumberAsync_ShouldReturnFailure_WhenStatusIsClosed()
        {
            // Arrange
            var timeIn = DateTime.Now.AddHours(-2);
            var timeOut = DateTime.Now;
            var session = new Session
            {
                Status = SessionEnum.CLOSED,
                TimeIn = timeIn,
                Mode = ModeEnum.MODE1,
                Block = 60,
                VehicleTypeId = Guid.NewGuid(),
                ImageInUrl = "in.jpg",
                PlateNumber = "99L999999",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = session,
                    Message = ErrorEnumApplication.SESSION_CLOSE
                });

            // Act
            var result = await _sessionService.GetNewestSessionByPlateNumberAsync("99L999999", timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SESSION_CLOSE, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByPlateNumberAsync_ShouldReturnFailure_WhenStatusIsCancelled()
        {
            // Arrange
            var timeIn = DateTime.Now.AddHours(-2);
            var timeOut = DateTime.Now;
            var session = new Session
            {
                Status = SessionEnum.CANCELLED,
                TimeIn = timeIn,
                Mode = ModeEnum.MODE1,
                Block = 60,
                VehicleTypeId = Guid.NewGuid(),
                ImageInUrl = "in.jpg",
                PlateNumber = "99L999999",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = session,
                    Message = ErrorEnumApplication.SESSION_CLOSE
                });

            // Act
            var result = await _sessionService.GetNewestSessionByPlateNumberAsync("99L999999", timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SESSION_CANCELLED, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByPlateNumberAsync_ShouldReturnFailure_WhenAuthenticationFailed()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _sessionService.GetNewestSessionByPlateNumberAsync("99L999999", DateTime.Now);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByPlateNumberAsync_ShouldReturnFailure_WhenSessionNotFound()
        {
            // Arrange
            var timeIn = DateTime.Now.AddHours(-2);
            var timeOut = DateTime.Now;

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_SESSION_WITH_PLATE_NUMBER
                });

            // Act
            var result = await _sessionService.GetNewestSessionByPlateNumberAsync("ABC123", DateTime.Now);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_SESSION_WITH_PLATE_NUMBER, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByPlateNumberAsync_ShouldReturnFailure_WhenTimeOutLessThanTimeIn()
        {
            // Arrange
            var timeIn = DateTime.Now.AddHours(-2);
            var timeOut = DateTime.Now;
            var session = new Session
            {
                Status = SessionEnum.PARKED,
                TimeIn = timeIn,
                Mode = ModeEnum.MODE1,
                Block = 60,
                VehicleTypeId = Guid.NewGuid(),
                ImageInUrl = "in.jpg",
                PlateNumber = "99L999999",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = ErrorEnumApplication.TIME_OUT_IS_MUST_BE_GREATER_TIME_IN
                });

            // Act
            var result = await _sessionService.GetNewestSessionByPlateNumberAsync("ABC123", timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.TIME_OUT_IS_MUST_BE_GREATER_TIME_IN, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByPlateNumberAsync_ShouldReturnFailure_WhenVehicleNotFound()
        {
            // Arrange
            var timeIn = DateTime.Now.AddHours(-2);
            var timeOut = DateTime.Now;
            var session = new Session
            {
                Status = SessionEnum.PARKED,
                TimeIn = timeIn,
                Mode = ModeEnum.MODE1,
                Block = 60,
                VehicleTypeId = Guid.NewGuid(),
                ImageInUrl = "in.jpg",
                PlateNumber = "99L999999",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = session,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _sessionService.GetNewestSessionByPlateNumberAsync("99L999999", timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByPlateNumberAsync_ShouldReturnSuccess_WhenPriceTableNotFound()
        {
            // Arrange
            var timeIn = DateTime.Now.AddHours(-2);
            var timeOut = DateTime.Now;
            var session = new Session
            {
                Status = SessionEnum.PARKED,
                TimeIn = timeIn,
                Mode = ModeEnum.MODE1,
                Block = 60,
                VehicleTypeId = Guid.NewGuid(),
                ImageInUrl = "in.jpg",
                PlateNumber = "99L999999",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = session,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    Data = null,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _sessionService.GetNewestSessionByPlateNumberAsync("99L999999", timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByPlateNumberAsync_ShouldReturnSuccess_WhenPriceTableIsDefault()
        {
            // Arrange
            var timeIn = DateTime.Now.AddHours(-2);
            var timeOut = DateTime.Now;
            var session = new Session
            {
                Status = SessionEnum.PARKED,
                TimeIn = timeIn,
                Mode = ModeEnum.MODE1,
                Block = 60,
                VehicleTypeId = Guid.NewGuid(),
                ImageInUrl = "in.jpg",
                PlateNumber = "99L999999",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = session,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    Data = new List<PriceTable>
                    { new PriceTable
                        {
                            Name = "Price Table",
                            Priority = 1,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    },
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _sessionService.GetNewestSessionByPlateNumberAsync("99L999999", timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByPlateNumberAsync_ShouldReturnFailure_WhenPriceItemNotFound()
        {
            // Arrange
            var timeIn = DateTime.Now.AddHours(-2);
            var timeOut = DateTime.Now;
            var session = new Session
            {
                Status = SessionEnum.PARKED,
                TimeIn = timeIn,
                Mode = ModeEnum.MODE1,
                Block = 60,
                VehicleTypeId = Guid.NewGuid(),
                ImageInUrl = "in.jpg",
                PlateNumber = "99L999999",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = session,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    { new PriceTable
                        {
                            Name = "Price Table",
                            Priority = 2,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    Data = null,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _sessionService.GetNewestSessionByPlateNumberAsync("99L999999", timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetNewestSessionByPlateNumberAsync_ShouldReturnFailure_WhenPriceItemGreaterThanOrEqualToTimeInAndApplyToLessThanOrEqualToTimeIn()
        {
            // Arrange
            var timeIn = DateTime.Now.AddHours(-2);
            var timeOut = DateTime.Now;
            var session = new Session
            {
                Status = SessionEnum.PARKED,
                TimeIn = timeIn,
                Mode = ModeEnum.MODE1,
                Block = 60,
                VehicleTypeId = Guid.NewGuid(),
                ImageInUrl = "in.jpg",
                PlateNumber = "99L999999",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = session,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    { new PriceTable
                        {
                            Name = "Price Table",
                            Priority = 2,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    Data = null,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _sessionService.GetNewestSessionByPlateNumberAsync("99L999999", timeOut);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CheckOutSessionByPlateNumberAsync
        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnFailure_WhenAuthenticationFailed()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            var req = new CheckOutSessionByPlateNumberReqDto();

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Successful
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnSuccess_WhenSuccessfulCheckoutForFreeCustomer()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        Customer = new Customer
                        {
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.FREE,
                                Description = "Free Customer",
                            },
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                        },
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            var req = new CheckOutSessionByPlateNumberReqDto
            {
                PlateNumber = "99L999999",
                CheckOutTime = DateTime.Now
            };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successful
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnSuccess_WhenSuccessfulCheckoutForPaidCustomer()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        Customer = new Customer
                        {
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Free Customer",
                            },
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                        },
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                        new PriceTable
                        {
                            Name = "Price Table",
                            Priority = 2,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                        new PriceItem
                        {
                            BlockPricing = 1000,
                            MinPrice = 1000,
                            MaxPrice = 10000
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentMethodByNameAsync(PaymentMethods.CASH))
                .ReturnsAsync(new Return<PaymentMethod>
                {
                    IsSuccess = true,
                    Data = new PaymentMethod
                    {
                        Name = PaymentMethods.WALLET,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            _paymentRepositoryMock.Setup(x => x.CreatePaymentAsync(It.IsAny<Payment>()))
                .ReturnsAsync(new Return<Payment>
                {
                    IsSuccess = true,
                    Data = new Payment
                    {
                        TotalPrice = 1000,
                    },
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                });

            _transactionRepositoryMock.Setup(x => x.CreateTransactionAsync(It.IsAny<FUParkingModel.Object.Transaction>()))
                .ReturnsAsync(new Return<Transaction>
                {
                    IsSuccess = true,
                    Data = new Transaction
                    {
                        TransactionDescription = "Transaction",
                        TransactionStatus = StatusTransactionEnum.SUCCEED,
                    },
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                });

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        CardNumber = "123456789",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            var req = new CheckOutSessionByPlateNumberReqDto
            {
                GateId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                CheckOutTime = DateTime.Now
            };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnsFailure_WhenGateNotExist()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            var req = new CheckOutSessionByPlateNumberReqDto { GateId = Guid.NewGuid() };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.GATE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnsFailure_WhenSessionNotFound()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null,
                    IsSuccess = false
                });

            var req = new CheckOutSessionByPlateNumberReqDto { GateId = Guid.NewGuid(), PlateNumber = "ABC123" };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_SESSION_WITH_PLATE_NUMBER, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnsFailure_WhenSessionClosed()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.CLOSED,
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            var req = new CheckOutSessionByPlateNumberReqDto { GateId = Guid.NewGuid(), PlateNumber = "ABC123" };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SESSION_CLOSE, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnsFailure_WhenSessionCancelled()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.CANCELLED,
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            var req = new CheckOutSessionByPlateNumberReqDto { GateId = Guid.NewGuid(), PlateNumber = "ABC123" };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SESSION_CANCELLED, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnFailure_WhenVehicleNotFound()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        Customer = new Customer
                        {
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Free Customer",
                            },
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                        },
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            var req = new CheckOutSessionByPlateNumberReqDto
            {
                GateId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                CheckOutTime = DateTime.Now
            };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnFailure_WhenPriceTableNotFound()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        Customer = new Customer
                        {
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Free Customer",
                            },
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                        },
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            var req = new CheckOutSessionByPlateNumberReqDto
            {
                GateId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                CheckOutTime = DateTime.Now
            };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnfailure_WhenPriceTableIsDefault()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        Customer = new Customer
                        {
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Free Customer",
                            },
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                        },
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                        new PriceTable
                        {
                            Name = "Price Table",
                            Priority = 1,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            var req = new CheckOutSessionByPlateNumberReqDto
            {
                GateId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                CheckOutTime = DateTime.Now
            };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnFailure_WhenPriceItemNotFound()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        Customer = new Customer
                        {
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Free Customer",
                            },
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                        },
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                new PriceTable
                {
                    Name = "Price Table",
                    Priority = 2,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            var req = new CheckOutSessionByPlateNumberReqDto
            {
                GateId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                CheckOutTime = DateTime.Now
            };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnFailure_WhenPriceItemGreaterThanOrEqualToTimeInAndApplyToLessThanOrEqualToTimeIn()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        Customer = new Customer
                        {
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Free Customer",
                            },
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                        },
                        TimeIn = DateTime.Now,
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                        new PriceTable
                        {
                            Name = "Price Table",
                            Priority = 2,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                        new PriceItem
                        {
                            BlockPricing = 1000,
                            MinPrice = 1000,
                            MaxPrice = 10000,
                            ApplyFromHour = 2,
                            ApplyToHour = 3
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            var req = new CheckOutSessionByPlateNumberReqDto
            {
                GateId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                CheckOutTime = DateTime.Now
            };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnFailure_WhenPaymentMethodNotFound()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        Customer = new Customer
                        {
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Free Customer",
                            },
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                        },
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                        new PriceTable
                        {
                            Name = "Price Table",
                            Priority = 2,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                        new PriceItem
                        {
                            BlockPricing = 1000,
                            MinPrice = 1000,
                            MaxPrice = 10000
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentMethodByNameAsync(PaymentMethods.CASH))
                .ReturnsAsync(new Return<PaymentMethod>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            var req = new CheckOutSessionByPlateNumberReqDto
            {
                GateId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                CheckOutTime = DateTime.Now
            };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnFailure_WhenUpdateSessionFail()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        Customer = new Customer
                        {
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Free Customer",
                            },
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                        },
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                        new PriceTable
                        {
                            Name = "Price Table",
                            Priority = 2,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                        new PriceItem
                        {
                            BlockPricing = 1000,
                            MinPrice = 1000,
                            MaxPrice = 10000
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentMethodByNameAsync(PaymentMethods.CASH))
                .ReturnsAsync(new Return<PaymentMethod>
                {
                    IsSuccess = true,
                    Data = new PaymentMethod
                    {
                        Name = PaymentMethods.WALLET,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            var req = new CheckOutSessionByPlateNumberReqDto
            {
                GateId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                CheckOutTime = DateTime.Now
            };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnFailure_WhenCreatePaymentFail()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        Customer = new Customer
                        {
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Free Customer",
                            },
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                        },
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                new PriceTable
                {
                    Name = "Price Table",
                    Priority = 2,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                new PriceItem
                {
                    BlockPricing = 1000,
                    MinPrice = 1000,
                    MaxPrice = 10000
                }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentMethodByNameAsync(PaymentMethods.CASH))
                .ReturnsAsync(new Return<PaymentMethod>
                {
                    IsSuccess = true,
                    Data = new PaymentMethod
                    {
                        Name = PaymentMethods.WALLET,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            _paymentRepositoryMock.Setup(x => x.CreatePaymentAsync(It.IsAny<Payment>()))
                .ReturnsAsync(new Return<Payment>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            var req = new CheckOutSessionByPlateNumberReqDto
            {
                GateId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                CheckOutTime = DateTime.Now
            };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnFailure_WhenCreateTransactionFail()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        Customer = new Customer
                        {
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Free Customer",
                            },
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                        },
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                new PriceTable
                {
                    Name = "Price Table",
                    Priority = 2,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                new PriceItem
                {
                    BlockPricing = 1000,
                    MinPrice = 1000,
                    MaxPrice = 10000
                }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentMethodByNameAsync(PaymentMethods.CASH))
                .ReturnsAsync(new Return<PaymentMethod>
                {
                    IsSuccess = true,
                    Data = new PaymentMethod
                    {
                        Name = PaymentMethods.WALLET,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            _paymentRepositoryMock.Setup(x => x.CreatePaymentAsync(It.IsAny<Payment>()))
                .ReturnsAsync(new Return<Payment>
                {
                    IsSuccess = true,
                    Data = new Payment
                    {
                        TotalPrice = 1000,
                    },
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                });

            _transactionRepositoryMock.Setup(x => x.CreateTransactionAsync(It.IsAny<FUParkingModel.Object.Transaction>()))
                .ReturnsAsync(new Return<Transaction>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            var req = new CheckOutSessionByPlateNumberReqDto
            {
                GateId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                CheckOutTime = DateTime.Now
            };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnFailure_WhenGetCardFail()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        Customer = new Customer
                        {
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Free Customer",
                            },
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                        },
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                new PriceTable
                {
                    Name = "Price Table",
                    Priority = 2,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                new PriceItem
                {
                    BlockPricing = 1000,
                    MinPrice = 1000,
                    MaxPrice = 10000
                }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentMethodByNameAsync(PaymentMethods.CASH))
                .ReturnsAsync(new Return<PaymentMethod>
                {
                    IsSuccess = true,
                    Data = new PaymentMethod
                    {
                        Name = PaymentMethods.WALLET,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            _paymentRepositoryMock.Setup(x => x.CreatePaymentAsync(It.IsAny<Payment>()))
                .ReturnsAsync(new Return<Payment>
                {
                    IsSuccess = true,
                    Data = new Payment
                    {
                        TotalPrice = 1000,
                    },
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                });

            _transactionRepositoryMock.Setup(x => x.CreateTransactionAsync(It.IsAny<FUParkingModel.Object.Transaction>()))
                .ReturnsAsync(new Return<Transaction>
                {
                    IsSuccess = true,
                    Data = new Transaction
                    {
                        TransactionDescription = "Transaction",
                        TransactionStatus = StatusTransactionEnum.SUCCEED,
                    },
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                });

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            var req = new CheckOutSessionByPlateNumberReqDto
            {
                GateId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                CheckOutTime = DateTime.Now
            };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutSessionByPlateNumberAsync_ShouldReturnFailure_WhenUpdateCardFail()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = "",
                        StatusUser = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Session
                    {
                        Status = SessionEnum.PARKED,
                        Customer = new Customer
                        {
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Free Customer",
                            },
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                        },
                        TimeIn = DateTime.Now.AddHours(-1),
                        Block = 60,
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        ImageInUrl = "in.jpg",
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                new PriceTable
                {
                    Name = "Price Table",
                    Priority = 2,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                new PriceItem
                {
                    BlockPricing = 1000,
                    MinPrice = 1000,
                    MaxPrice = 10000
                }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentMethodByNameAsync(PaymentMethods.CASH))
                .ReturnsAsync(new Return<PaymentMethod>
                {
                    IsSuccess = true,
                    Data = new PaymentMethod
                    {
                        Name = PaymentMethods.WALLET,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            _paymentRepositoryMock.Setup(x => x.CreatePaymentAsync(It.IsAny<Payment>()))
                .ReturnsAsync(new Return<Payment>
                {
                    IsSuccess = true,
                    Data = new Payment
                    {
                        TotalPrice = 1000,
                    },
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                });

            _transactionRepositoryMock.Setup(x => x.CreateTransactionAsync(It.IsAny<FUParkingModel.Object.Transaction>()))
                .ReturnsAsync(new Return<Transaction>
                {
                    IsSuccess = true,
                    Data = new Transaction
                    {
                        TransactionDescription = "Transaction",
                        TransactionStatus = StatusTransactionEnum.SUCCEED,
                    },
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                });

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        CardNumber = "123456789",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            var req = new CheckOutSessionByPlateNumberReqDto
            {
                GateId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                CheckOutTime = DateTime.Now
            };

            // Act
            var result = await _sessionService.CheckOutSessionByPlateNumberAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CheckOutAsync
        [Fact]
        public async Task CheckOutAsync_ShouldReturnSuccess_WhenCustomerIsFree()
        {
            // Arrange
            var req = new CheckOutAsyncReqDto
            {
                CardNumber = "123456",
                GateOutId = Guid.NewGuid(),
                TimeOut = DateTime.UtcNow,
                PlateNumber = "51F12345",
                ImageOut = new FormFile(null, 0, 0, "test", "test.jpg"),
                ImageBody = new FormFile(null, 0, 0, "test", "test.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        CardNumber = "123456789",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        Customer = new Customer
                        {
                            Email = "Customer@gmail.com",
                            FullName = "Customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.FREE,
                                Description = "Free Customer",
                            }
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1)
                    }
                });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
               .ReturnsAsync(new Return<ReturnObjectUrlResDto>
               {
                   IsSuccess = true,
                   Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY,
                   Data = new ReturnObjectUrlResDto
                   {
                       ObjUrl = "ImageOutUrl"
                   }
               });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
                .ReturnsAsync(new Return<ReturnObjectUrlResDto>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY,
                    Data = new ReturnObjectUrlResDto
                    {
                        ObjUrl = "ImageBodyOutUrl"
                    }
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.CLOSED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        TimeOut = DateTime.UtcNow,
                        ImageOutUrl = req.ImageOut.FileName,
                        ImageOutBodyUrl = req.ImageBody.FileName,
                        GateOutId = Guid.NewGuid()
                    }
                });

            // Act
            var result = await _sessionService.CheckOutAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutAsync_ShouldReturnFailure_WhenInvalidUser()
        {
            // Arrange
            var req = new CheckOutAsyncReqDto
            {
                CardNumber = "123456",
                GateOutId = Guid.NewGuid(),
                TimeOut = DateTime.UtcNow,
                PlateNumber = "51F12345",
                ImageOut = new FormFile(null, 0, 0, "test", "test.jpg"),
                ImageBody = new FormFile(null, 0, 0, "test", "test.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _sessionService.CheckOutAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutAsync_ShouldReturnFailure_WhenCardNotFound()
        {
            // Arrange
            var req = new CheckOutAsyncReqDto
            {
                CardNumber = "123456",
                GateOutId = Guid.NewGuid(),
                TimeOut = DateTime.UtcNow,
                PlateNumber = "51F12345",
                ImageOut = new FormFile(null, 0, 0, "test", "test.jpg"),
                ImageBody = new FormFile(null, 0, 0, "test", "test.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CheckOutAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutAsync_ShouldReturnFailure_WhenNewsetSessionNotFound()
        {
            // Arrange
            var req = new CheckOutAsyncReqDto
            {
                CardNumber = "123456",
                GateOutId = Guid.NewGuid(),
                TimeOut = DateTime.UtcNow,
                PlateNumber = "51F12345",
                ImageOut = new FormFile(null, 0, 0, "test", "test.jpg"),
                ImageBody = new FormFile(null, 0, 0, "test", "test.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        CardNumber = "123456789",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CheckOutAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutAsync_ShouldReturnFailure_WhenGateOutNotFound()
        {
            // Arrange
            var req = new CheckOutAsyncReqDto
            {
                CardNumber = "123456",
                GateOutId = Guid.NewGuid(),
                TimeOut = DateTime.UtcNow,
                PlateNumber = "51F12345",
                ImageOut = new FormFile(null, 0, 0, "test", "test.jpg"),
                ImageBody = new FormFile(null, 0, 0, "test", "test.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        CardNumber = "123456789",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        Customer = new Customer
                        {
                            Email = "Customer@gmail.com",
                            FullName = "Customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.FREE,
                                Description = "Free Customer",
                            }
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CheckOutAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutAsync_ShouldReturnFailure_WhenNotAPlateNumber()
        {
            // Arrange
            var req = new CheckOutAsyncReqDto
            {
                CardNumber = "123456",
                GateOutId = Guid.NewGuid(),
                TimeOut = DateTime.UtcNow,
                PlateNumber = "INVALID",
                ImageOut = new FormFile(null, 0, 0, "test", "test.jpg"),
                ImageBody = new FormFile(null, 0, 0, "test", "test.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        CardNumber = "123456789",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        Customer = new Customer
                        {
                            Email = "Customer@gmail.com",
                            FullName = "Customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.FREE,
                                Description = "Free Customer",
                            }
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CheckOutAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_A_PLATE_NUMBER, result.Message);
        }
        // Failure
        [Fact]
        public async Task CheckOutAsync_ShouldReturnFailure_WhenPlateNumberIsBelongToAnotherSession()
        {
            // Arrange
            var req = new CheckOutAsyncReqDto
            {
                CardNumber = "123456",
                GateOutId = Guid.NewGuid(),
                TimeOut = DateTime.UtcNow,
                PlateNumber = "99L999999",
                ImageOut = new FormFile(null, 0, 0, "test", "test.jpg"),
                ImageBody = new FormFile(null, 0, 0, "test", "test.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        CardNumber = "123456789",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "66L666666",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        Customer = new Customer
                        {
                            Email = "Customer@gmail.com",
                            FullName = "Customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.FREE,
                                Description = "Free Customer",
                            }
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<String>()))
               .ReturnsAsync(new Return<Session>
               {
                   Message = SuccessfullyEnumServer.FOUND_OBJECT,
                   IsSuccess = true,
                   Data = new Session
                   {
                       Block = 60,
                       ImageInUrl = "in.jpg",
                       Mode = ModeEnum.MODE1,
                       PlateNumber = "99L999999",
                       Status = SessionEnum.PARKED,
                       TimeIn = DateTime.UtcNow.AddHours(-1)
                   }
               });

            // Act
            var result = await _sessionService.CheckOutAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PLATE_NUMBER_IS_BELONG_TO_ANOTHER_SESSION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutAsync_ShouldReturnFailure_WhenUploadImgFail()
        {
            // Arrange
            var req = new CheckOutAsyncReqDto
            {
                CardNumber = "123456",
                GateOutId = Guid.NewGuid(),
                TimeOut = DateTime.UtcNow,
                PlateNumber = "51F12345",
                ImageOut = new FormFile(null, 0, 0, "test", "test.jpg"),
                ImageBody = new FormFile(null, 0, 0, "test", "test.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        CardNumber = "123456789",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        Customer = new Customer
                        {
                            Email = "Customer@gmail.com",
                            FullName = "Customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.FREE,
                                Description = "Free Customer",
                            }
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1)
                    }
                });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
               .ReturnsAsync(new Return<ReturnObjectUrlResDto>
               {
                   IsSuccess = false,
                   Message = ErrorEnumApplication.UPLOAD_IMAGE_FAILED,
               });

            // Act
            var result = await _sessionService.CheckOutAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.UPLOAD_IMAGE_FAILED, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckOutAsync_ShouldReturnFailure_WhenUpdateSessionFail()
        {
            // Arrange
            var req = new CheckOutAsyncReqDto
            {
                CardNumber = "123456",
                GateOutId = Guid.NewGuid(),
                TimeOut = DateTime.UtcNow,
                PlateNumber = "51F12345",
                ImageOut = new FormFile(null, 0, 0, "test", "test.jpg"),
                ImageBody = new FormFile(null, 0, 0, "test", "test.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        CardNumber = "123456789",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        Customer = new Customer
                        {
                            Email = "Customer@gmail.com",
                            FullName = "Customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.FREE,
                                Description = "Free Customer",
                            }
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1)
                    }
                });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
               .ReturnsAsync(new Return<ReturnObjectUrlResDto>
               {
                   IsSuccess = true,
                   Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY,
                   Data = new ReturnObjectUrlResDto
                   {
                       ObjUrl = "ImageOutUrl"
                   }
               });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
                .ReturnsAsync(new Return<ReturnObjectUrlResDto>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY,
                    Data = new ReturnObjectUrlResDto
                    {
                        ObjUrl = "ImageBodyOutUrl"
                    }
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                });

            // Act
            var result = await _sessionService.CheckOutAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Successful
        [Fact]
        public async Task CheckOutAsync_ShouldReturnSuccess_WhenCustomerIsPaidByExtraWallet()
        {
            // Arrange
            var price = 5000;
            var cusId = Guid.NewGuid();

            var req = new CheckOutAsyncReqDto
            {
                CardNumber = "123456",
                GateOutId = Guid.NewGuid(),
                TimeOut = DateTime.UtcNow,
                PlateNumber = "51F12345",
                ImageOut = new FormFile(null, 0, 0, "test", "test.jpg"),
                ImageBody = new FormFile(null, 0, 0, "test", "test.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        CardNumber = "123456789",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        CustomerId = cusId,
                        Customer = new Customer
                        {
                            Id = cusId,
                            Email = "Customer@gmail.com",
                            FullName = "Customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Paid Customer",
                            }
                        },
                        GateIn = new Gate
                        {
                            Name = "FPTU",
                            StatusGate = StatusGateEnum.ACTIVE,
                            ParkingArea = new ParkingArea
                            {
                                Block = 60,
                                Mode = ModeEnum.MODE1,
                                Name = "FPTU1",
                                StatusParkingArea = StatusParkingEnum.ACTIVE
                            }
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1)
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    }
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                        new PriceTable
                        {
                            Name = "Price table",
                            Priority = 2,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    }
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                        new PriceItem
                        {
                            BlockPricing = 1000,
                            MinPrice = 1000,
                            MaxPrice = 10000
                        }
                    }
                });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
              .ReturnsAsync(new Return<ReturnObjectUrlResDto>
              {
                  IsSuccess = true,
                  Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY,
                  Data = new ReturnObjectUrlResDto
                  {
                      ObjUrl = "ImageOutUrl"
                  }
              });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
                .ReturnsAsync(new Return<ReturnObjectUrlResDto>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY,
                    Data = new ReturnObjectUrlResDto
                    {
                        ObjUrl = "ImageBodyOutUrl"
                    }
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet
                    {
                        Balance = 1000,
                        CustomerId = Guid.NewGuid(),
                        WalletType = WalletType.MAIN,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetExtraWalletByCustomerId(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet
                    {
                        Balance = 6000,
                        CustomerId = Guid.NewGuid(),
                        WalletType = WalletType.EXTRA,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentMethodByNameAsync(PaymentMethods.WALLET))
                .ReturnsAsync(new Return<PaymentMethod>
                {
                    IsSuccess = true,
                    Data = new PaymentMethod
                    {
                        Name = PaymentMethods.WALLET,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.CreatePaymentAsync(It.IsAny<Payment>()))
                .ReturnsAsync(new Return<Payment>
                {
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new Payment
                    {
                        TotalPrice = price,
                    }
                });

            _transactionRepositoryMock.Setup(x => x.CreateTransactionAsync(It.IsAny<FUParkingModel.Object.Transaction>()))
                .ReturnsAsync(new Return<Transaction>
                {
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new Transaction
                    {
                        TransactionDescription = "Transaction",
                        TransactionStatus = StatusTransactionEnum.SUCCEED,
                    }
                });

            _walletRepositoryMock.Setup(x => x.UpdateWalletAsync(It.IsAny<Wallet>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.CLOSED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        TimeOut = DateTime.UtcNow,
                        ImageOutUrl = req.ImageOut.FileName,
                        ImageOutBodyUrl = req.ImageBody.FileName,
                        GateOutId = Guid.NewGuid()
                    }
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = cusId,
                        Email = "Customer@gmail.com",
                        FullName = "Customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE,
                        CustomerType = new CustomerType
                        {
                            Name = CustomerTypeEnum.PAID,
                            Description = "Paid Customer",
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CheckOutAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successful
        [Fact]
        public async Task CheckOutAsync_ShouldReturnSuccess_WhenCustomerIsPaidByMainWallet()
        {
            // Arrange
            var price = 5000;
            var cusId = Guid.NewGuid();

            var req = new CheckOutAsyncReqDto
            {
                CardNumber = "123456",
                GateOutId = Guid.NewGuid(),
                TimeOut = DateTime.UtcNow,
                PlateNumber = "51F12345",
                ImageOut = new FormFile(null, 0, 0, "test", "test.jpg"),
                ImageBody = new FormFile(null, 0, 0, "test", "test.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        CardNumber = "123456789",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        CustomerId = cusId,
                        Customer = new Customer
                        {
                            Id = cusId,
                            Email = "Customer@gmail.com",
                            FullName = "Customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Paid Customer",
                            }
                        },
                        GateIn = new Gate
                        {
                            Name = "FPTU",
                            StatusGate = StatusGateEnum.ACTIVE,
                            ParkingArea = new ParkingArea
                            {
                                Block = 60,
                                Mode = ModeEnum.MODE1,
                                Name = "FPTU1",
                                StatusParkingArea = StatusParkingEnum.ACTIVE
                            }
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1)
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    }
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                        new PriceTable
                        {
                            Name = "Price table",
                            Priority = 2,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    }
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                        new PriceItem
                        {
                            BlockPricing = 1000,
                            MinPrice = 1000,
                            MaxPrice = 10000
                        }
                    }
                });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
              .ReturnsAsync(new Return<ReturnObjectUrlResDto>
              {
                  IsSuccess = true,
                  Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY,
                  Data = new ReturnObjectUrlResDto
                  {
                      ObjUrl = "ImageOutUrl"
                  }
              });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
                .ReturnsAsync(new Return<ReturnObjectUrlResDto>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY,
                    Data = new ReturnObjectUrlResDto
                    {
                        ObjUrl = "ImageBodyOutUrl"
                    }
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet
                    {
                        Balance = 6000,
                        CustomerId = Guid.NewGuid(),
                        WalletType = WalletType.MAIN,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetExtraWalletByCustomerId(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet
                    {
                        Balance = 100,
                        CustomerId = Guid.NewGuid(),
                        WalletType = WalletType.EXTRA,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentMethodByNameAsync(PaymentMethods.WALLET))
                .ReturnsAsync(new Return<PaymentMethod>
                {
                    IsSuccess = true,
                    Data = new PaymentMethod
                    {
                        Name = PaymentMethods.WALLET,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.CreatePaymentAsync(It.IsAny<Payment>()))
                .ReturnsAsync(new Return<Payment>
                {
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new Payment
                    {
                        TotalPrice = price,
                    }
                });

            _transactionRepositoryMock.Setup(x => x.CreateTransactionAsync(It.IsAny<FUParkingModel.Object.Transaction>()))
                .ReturnsAsync(new Return<Transaction>
                {
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new Transaction
                    {
                        TransactionDescription = "Transaction",
                        TransactionStatus = StatusTransactionEnum.SUCCEED,
                    }
                });

            _walletRepositoryMock.Setup(x => x.UpdateWalletAsync(It.IsAny<Wallet>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.CLOSED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        TimeOut = DateTime.UtcNow,
                        ImageOutUrl = req.ImageOut.FileName,
                        ImageOutBodyUrl = req.ImageBody.FileName,
                        GateOutId = Guid.NewGuid()
                    }
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = cusId,
                        Email = "Customer@gmail.com",
                        FullName = "Customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE,
                        CustomerType = new CustomerType
                        {
                            Name = CustomerTypeEnum.PAID,
                            Description = "Paid Customer",
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CheckOutAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successful
        [Fact]
        public async Task CheckOutAsync_ShouldReturnSuccess_WhenCustomerIsPaidByCash()
        {
            // Arrange
            var price = 5000;
            var cusId = Guid.NewGuid();

            var req = new CheckOutAsyncReqDto
            {
                CardNumber = "123456",
                GateOutId = Guid.NewGuid(),
                TimeOut = DateTime.UtcNow,
                PlateNumber = "51F12345",
                ImageOut = new FormFile(null, 0, 0, "test", "test.jpg"),
                ImageBody = new FormFile(null, 0, 0, "test", "test.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        CardNumber = "123456789",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        CustomerId = cusId,
                        Customer = new Customer
                        {
                            Id = cusId,
                            Email = "Customer@gmail.com",
                            FullName = "Customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Paid Customer",
                            }
                        },
                        GateIn = new Gate
                        {
                            Name = "FPTU",
                            StatusGate = StatusGateEnum.ACTIVE,
                            ParkingArea = new ParkingArea
                            {
                                Block = 60,
                                Mode = ModeEnum.MODE1,
                                Name = "FPTU1",
                                StatusParkingArea = StatusParkingEnum.ACTIVE
                            }
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1)
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    }
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                        new PriceTable
                        {
                            Name = "Price table",
                            Priority = 2,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    }
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                        new PriceItem
                        {
                            BlockPricing = 1000,
                            MinPrice = 1000,
                            MaxPrice = 10000
                        }
                    }
                });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
              .ReturnsAsync(new Return<ReturnObjectUrlResDto>
              {
                  IsSuccess = true,
                  Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY,
                  Data = new ReturnObjectUrlResDto
                  {
                      ObjUrl = "ImageOutUrl"
                  }
              });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
                .ReturnsAsync(new Return<ReturnObjectUrlResDto>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY,
                    Data = new ReturnObjectUrlResDto
                    {
                        ObjUrl = "ImageBodyOutUrl"
                    }
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet
                    {
                        Balance = 1000,
                        CustomerId = Guid.NewGuid(),
                        WalletType = WalletType.MAIN,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetExtraWalletByCustomerId(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet
                    {
                        Balance = 6000,
                        CustomerId = Guid.NewGuid(),
                        WalletType = WalletType.EXTRA,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentMethodByNameAsync(PaymentMethods.WALLET))
                .ReturnsAsync(new Return<PaymentMethod>
                {
                    IsSuccess = true,
                    Data = new PaymentMethod
                    {
                        Name = PaymentMethods.WALLET,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.CreatePaymentAsync(It.IsAny<Payment>()))
                .ReturnsAsync(new Return<Payment>
                {
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new Payment
                    {
                        TotalPrice = price,
                    }
                });

            _transactionRepositoryMock.Setup(x => x.CreateTransactionAsync(It.IsAny<FUParkingModel.Object.Transaction>()))
                .ReturnsAsync(new Return<Transaction>
                {
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new Transaction
                    {
                        TransactionDescription = "Transaction",
                        TransactionStatus = StatusTransactionEnum.SUCCEED,
                    }
                });

            _walletRepositoryMock.Setup(x => x.UpdateWalletAsync(It.IsAny<Wallet>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.CLOSED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        TimeOut = DateTime.UtcNow,
                        ImageOutUrl = req.ImageOut.FileName,
                        ImageOutBodyUrl = req.ImageBody.FileName,
                        GateOutId = Guid.NewGuid()
                    }
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = cusId,
                        Email = "Customer@gmail.com",
                        FullName = "Customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE,
                        CustomerType = new CustomerType
                        {
                            Name = CustomerTypeEnum.PAID,
                            Description = "Paid Customer",
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CheckOutAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successful
        [Fact]
        public async Task CheckOutAsync_ShouldReturnSuccess_WhenCustomerIsPaidByExtraAndMainWallet()
        {
            // Arrange
            var price = 5000;
            var cusId = Guid.NewGuid();

            var req = new CheckOutAsyncReqDto
            {
                CardNumber = "123456",
                GateOutId = Guid.NewGuid(),
                TimeOut = DateTime.UtcNow,
                PlateNumber = "51F12345",
                ImageOut = new FormFile(null, 0, 0, "test", "test.jpg"),
                ImageBody = new FormFile(null, 0, 0, "test", "test.jpg")
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Card>
                {
                    IsSuccess = true,
                    Data = new Card
                    {
                        CardNumber = "123456789",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        CustomerId = cusId,
                        Customer = new Customer
                        {
                            Id = cusId,
                            Email = "Customer@gmail.com",
                            FullName = "Customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE,
                            CustomerType = new CustomerType
                            {
                                Name = CustomerTypeEnum.PAID,
                                Description = "Paid Customer",
                            }
                        },
                        GateIn = new Gate
                        {
                            Name = "FPTU",
                            StatusGate = StatusGateEnum.ACTIVE,
                            ParkingArea = new ParkingArea
                            {
                                Block = 60,
                                Mode = ModeEnum.MODE1,
                                Name = "FPTU1",
                                StatusParkingArea = StatusParkingEnum.ACTIVE
                            }
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.GetGateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = "FPTU",
                        StatusGate = StatusGateEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<String>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.PARKED,
                        TimeIn = DateTime.UtcNow.AddHours(-1)
                    }
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Name = "Car",
                    }
                });

            _priceRepositoryMock.Setup(x => x.GetListPriceTableActiveByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new List<PriceTable>
                    {
                        new PriceTable
                        {
                            Name = "Price table",
                            Priority = 2,
                            StatusPriceTable = StatusPriceTableEnum.ACTIVE
                        }
                    }
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new List<PriceItem>
                    {
                        new PriceItem
                        {
                            BlockPricing = 1000,
                            MinPrice = 1000,
                            MaxPrice = 10000
                        }
                    }
                });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
              .ReturnsAsync(new Return<ReturnObjectUrlResDto>
              {
                  IsSuccess = true,
                  Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY,
                  Data = new ReturnObjectUrlResDto
                  {
                      ObjUrl = "ImageOutUrl"
                  }
              });

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>()))
                .ReturnsAsync(new Return<ReturnObjectUrlResDto>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY,
                    Data = new ReturnObjectUrlResDto
                    {
                        ObjUrl = "ImageBodyOutUrl"
                    }
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet
                    {
                        Balance = 3000,
                        CustomerId = Guid.NewGuid(),
                        WalletType = WalletType.MAIN,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetExtraWalletByCustomerId(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet
                    {
                        Balance = 3000,
                        CustomerId = Guid.NewGuid(),
                        WalletType = WalletType.EXTRA,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.GetPaymentMethodByNameAsync(PaymentMethods.WALLET))
                .ReturnsAsync(new Return<PaymentMethod>
                {
                    IsSuccess = true,
                    Data = new PaymentMethod
                    {
                        Name = PaymentMethods.WALLET,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock.Setup(x => x.CreatePaymentAsync(It.IsAny<Payment>()))
                .ReturnsAsync(new Return<Payment>
                {
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new Payment
                    {
                        TotalPrice = price,
                    }
                });

            _transactionRepositoryMock.Setup(x => x.CreateTransactionAsync(It.IsAny<FUParkingModel.Object.Transaction>()))
                .ReturnsAsync(new Return<Transaction>
                {
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new Transaction
                    {
                        TransactionDescription = "Transaction",
                        TransactionStatus = StatusTransactionEnum.SUCCEED,
                    }
                });

            _walletRepositoryMock.Setup(x => x.UpdateWalletAsync(It.IsAny<Wallet>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            _sessionRepositoryMock.Setup(x => x.UpdateSessionAsync(It.IsAny<Session>()))
                .ReturnsAsync(new Return<Session>
                {
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new Session
                    {
                        Block = 60,
                        ImageInUrl = "in.jpg",
                        Mode = ModeEnum.MODE1,
                        PlateNumber = "99L999999",
                        Status = SessionEnum.CLOSED,
                        TimeIn = DateTime.UtcNow.AddHours(-1),
                        TimeOut = DateTime.UtcNow,
                        ImageOutUrl = req.ImageOut.FileName,
                        ImageOutBodyUrl = req.ImageBody.FileName,
                        GateOutId = Guid.NewGuid()
                    }
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = cusId,
                        Email = "Customer@gmail.com",
                        FullName = "Customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE,
                        CustomerType = new CustomerType
                        {
                            Name = CustomerTypeEnum.PAID,
                            Description = "Paid Customer",
                        }
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _sessionService.CheckOutAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }
    }
}