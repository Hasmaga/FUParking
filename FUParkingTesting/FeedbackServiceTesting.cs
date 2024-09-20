using Castle.Core.Resource;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService;
using FUParkingService.Interface;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FUParkingTesting
{
    public class FeedbackServiceTesting
    {
        private readonly Mock<IFeedbackRepository> _feedbackRepositoryMock = new();
        private readonly Mock<IHelpperService> _helpperServiceMock = new();
        private readonly Mock<IParkingAreaRepository> _parkingAreaRepositoryMock = new();
        private readonly Mock<ISessionRepository> _sessionRepositoryMock = new();
        private readonly FeedbackService _feedbackService;

        public FeedbackServiceTesting()
        {
            _feedbackService = new FeedbackService(_feedbackRepositoryMock.Object, _parkingAreaRepositoryMock.Object, _helpperServiceMock.Object, _sessionRepositoryMock.Object);
        }

        // CreateFeedbackAsync
        // Successsful
        [Fact]
        public async Task CreateFeedbackAsync_ShouldReturnSuccess()
        {
            // Arrange
            var request = new FeedbackReqDto
            {
                Title = "Great Service",
                Description = "I had a wonderful experience."
            };

            var session = new Session
            {
                Id = Guid.NewGuid(),
                Status = SessionEnum.PARKED,
                Customer = new Customer
                {
                    Email = "customer@localhost.com",
                    FullName = "Customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                },
                GateIn = new Gate
                {
                    Name = "Gate 1",
                    StatusGate = StatusGateEnum.ACTIVE
                },
                PlateNumber = "99L999999",
                TimeIn = DateTime.Now,
                VehicleType = new VehicleType
                {
                    Name = "Car"
                },
                ImageInUrl = "http://example.com/image.jpg",
                ImageInBodyUrl = "http://example.com/body.jpg",
                Mode = "MODE1",
                Block = 30
            };

            var parkingArea = new ParkingArea
            {
                Id = session.GateIn.ParkingAreaId,
                Name = "Parking Area 1",
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = session.Customer
            };

            var sessionReturn = new Return<Session>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = session
            };

            var parkingReturn = new Return<ParkingArea>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = parkingArea
            };

            var createReturn = new Return<Feedback>
            {
                IsSuccess = true,
                Data = new Feedback
                {
                    CustomerId = session.Customer.Id,
                    Description = request.Description,
                    ParkingAreaId = parkingArea.Id,
                    SessionId = session.Id,
                    Title = request.Title
                },
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(customerReturn);

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(request.SessionId)).ReturnsAsync(sessionReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(session.GateIn.ParkingAreaId)).ReturnsAsync(parkingReturn);

            _feedbackRepositoryMock.Setup(x => x.CreateFeedbackAsync(It.IsAny<Feedback>())).ReturnsAsync(createReturn);

            // Act
            var result = await _feedbackService.CreateFeedbackAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.SUCCESSFULLY, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task CreateFeedbackAsync_ShouldReturnFailure_WhenCustomerValidationFailed()
        {
            // Arrange
            var request = new FeedbackReqDto
            {
                Title = "Great Service",
                Description = "I had a wonderful experience."
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(customerReturn);

            // Act
            var result = await _feedbackService.CreateFeedbackAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task CreateFeedbackAsync_ShouldReturnFailure_WhenSessionNotFound()
        {
            // Arrange
            var request = new FeedbackReqDto
            {
                SessionId = Guid.NewGuid(),
                Title = "Great Service",
                Description = "I had a wonderful experience."
            };

            var customer = new Customer
            {
                Email = "customer@localhost.com",
                FullName = "Customer",
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = customer
            };

            var sessionReturn = new Return<Session>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(customerReturn);

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(request.SessionId)).ReturnsAsync(sessionReturn);

            // Act
            var result = await _feedbackService.CreateFeedbackAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task CreateFeedbackAsync_ShouldReturnFailure_WhenParkingAreaNotFound()
        {
            // Arrange
            var request = new FeedbackReqDto
            {
                SessionId = Guid.NewGuid(),
                Title = "Great Service",
                Description = "I had a wonderful experience."
            };

            var session = new Session
            {
                Id = Guid.NewGuid(),
                Status = SessionEnum.PARKED,
                Customer = new Customer
                {
                    Email = "customer@localhost.com",
                    FullName = "Customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                },
                GateIn = new Gate
                {
                    Name = "Gate 1",
                    StatusGate = StatusGateEnum.ACTIVE
                },
                PlateNumber = "99L999999",
                TimeIn = DateTime.Now,
                VehicleType = new VehicleType
                {
                    Name = "Car"
                },
                ImageInUrl = "http://example.com/image.jpg",
                ImageInBodyUrl = "http://example.com/body.jpg",
                Mode = "MODE1",
                Block = 30
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = session.Customer
            };

            var sessionReturn = new Return<Session>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = session
            };

            var parkingReturn = new Return<ParkingArea>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                Data = null
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(customerReturn);

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(request.SessionId))
                .ReturnsAsync(sessionReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(session.GateIn.ParkingAreaId))
                .ReturnsAsync(parkingReturn);

            // Act
            var result = await _feedbackService.CreateFeedbackAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task CreateFeedbackAsync_ShouldReturnFailure_WhenFeedbackCreationFailed()
        {
            // Arrange
            var request = new FeedbackReqDto
            {
                Title = "Great Service",
                Description = "I had a wonderful experience."
            };

            var session = new Session
            {
                Id = Guid.NewGuid(),
                Status = SessionEnum.PARKED,
                Customer = new Customer
                {
                    Email = "customer@localhost.com",
                    FullName = "Customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                },
                GateIn = new Gate
                {
                    Name = "Gate 1",
                    StatusGate = StatusGateEnum.ACTIVE
                },
                PlateNumber = "99L999999",
                TimeIn = DateTime.Now,
                VehicleType = new VehicleType
                {
                    Name = "Car"
                },
                ImageInUrl = "http://example.com/image.jpg",
                ImageInBodyUrl = "http://example.com/body.jpg",
                Mode = "MODE1",
                Block = 30
            };

            var parkingArea = new ParkingArea
            {
                Id = session.GateIn.ParkingAreaId,
                Name = "Parking Area 1",
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = session.Customer
            };

            var sessionReturn = new Return<Session>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = session
            };

            var parkingReturn = new Return<ParkingArea>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = parkingArea
            };

            var createReturn = new Return<Feedback>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(customerReturn);

            _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync(request.SessionId)).ReturnsAsync(sessionReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(session.GateIn.ParkingAreaId)).ReturnsAsync(parkingReturn);

            _feedbackRepositoryMock.Setup(x => x.CreateFeedbackAsync(It.IsAny<Feedback>())).ReturnsAsync(createReturn);

            // Act
            var result = await _feedbackService.CreateFeedbackAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetFeedbacksByCustomerIdAsync
        // Successsful
        [Fact]
        public async Task GetFeedbacksByCustomerIdAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var pageIndex = 1;
            var pageSize = 10;
            var customerId = Guid.NewGuid();

            var customer = new Customer
            {
                Id = customerId,
                FullName = "Customer",
                Email = "customer@localhost.com",
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var feedbacks = new List<Feedback>
            {
                new() {
                    Title = "Test",
                    Description = "Test Description",
                    ParkingArea = new ParkingArea
                    {
                        Name = "Test Area",
                        Block = 30,
                        Mode = "MODE1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE
                    }
                }
            };

            var feedbackReturn = new Return<IEnumerable<Feedback>>
            {
                IsSuccess = true,
                Data = feedbacks,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(customerReturn);

            _feedbackRepositoryMock.Setup(x => x.GetCustomerFeedbacksByCustomerIdAsync(customerId, pageIndex, pageSize)).ReturnsAsync(feedbackReturn);

            // Act
            var result = await _feedbackService.GetFeedbacksByCustomerIdAsync(pageSize, pageIndex);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task GetFeedbacksByCustomerIdAsync_ShouldReturnsFailure_WhenCustomerValidationFailed()
        {
            // Arrange
            var pageSize = 10;
            var pageIndex = 1;

            var customerReturn = new Return<Customer>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(customerReturn);

            // Act
            var result = await _feedbackService.GetFeedbacksByCustomerIdAsync(pageSize, pageIndex);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
            Assert.Null(result.Data);
        }
    }
}
