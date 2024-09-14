using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ResponseObject.Payment;
using FUParkingModel.ResponseObject.Statistic;
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
    public class PaymentServiceTesting
    {
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();
        private readonly Mock<IHelpperService> _helpperServiceMock = new();

        private readonly PaymentService _paymentService;

        public PaymentServiceTesting()
        {
            _helpperServiceMock = new Mock<IHelpperService>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _paymentService = new PaymentService(_paymentRepositoryMock.Object, _helpperServiceMock.Object);
        }

        // StatisticPaymentByCustomerAsync
        // Successful
        [Fact]
        public async Task StatisticPaymentByCustomerAsync_ShouldReturnSuccess()
        {
            // Arrange
            var statisticReturn = new Return<StatisticPaymentByCustomerResDto>
            {
                IsSuccess = true,
                Data = new StatisticPaymentByCustomerResDto(),
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock
                .Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer> 
                { 
                    IsSuccess = true, 
                    Data = new Customer 
                    { 
                        Email = "customer@localhost.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    } ,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock
                .Setup(x => x.StatisticPaymentByCustomerAsync(It.IsAny<Guid>()))
                .ReturnsAsync(statisticReturn);

            // Act
            var result = await _paymentService.StatisticPaymentByCustomerAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(statisticReturn, result);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task StatisticPaymentByCustomerAsync_ShouldReturnFailure_WhenAuthenticationFailed()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer> 
                { 
                    IsSuccess = false, 
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _paymentService.StatisticPaymentByCustomerAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        [Fact]
        public async Task StatisticPaymentByCustomerAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var statisticReturn = new Return<StatisticPaymentByCustomerResDto>
            {
                IsSuccess = true,
                Data = new StatisticPaymentByCustomerResDto(),
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock
                .Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            _paymentRepositoryMock
                .Setup(x => x.StatisticPaymentByCustomerAsync(It.IsAny<Guid>()))
                .ReturnsAsync(statisticReturn);

            // Act
            var result = await _paymentService.StatisticPaymentByCustomerAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // StatisticSessionPaymentMethodByCustomerAsync
        // Successful
        [Fact]
        public async Task StatisticSessionPaymentMethodByCustomerAsync_ShouldReturnSuccess()
        {
            // Arrange
            var statisticReturn = new Return<IEnumerable<StatisticSessionPaymentMethodResDto>>
            {
                IsSuccess = true,
                Data = new List<StatisticSessionPaymentMethodResDto>(),
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock
                .Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Email = "customer@localhost.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock
                .Setup(x => x.StatisticSessionPaymentMethodByCustomerAsync(It.IsAny<Guid>()))
                .ReturnsAsync(statisticReturn);

            // Act
            var result = await _paymentService.StatisticSessionPaymentMethodByCustomerAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task StatisticSessionPaymentMethodByCustomerAsync_ShouldReturnFailure_WhenAuthenticationFailed()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _paymentService.StatisticSessionPaymentMethodByCustomerAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task StatisticSessionPaymentMethodByCustomerAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var statisticReturn = new Return<IEnumerable<StatisticSessionPaymentMethodResDto>>
            {
                IsSuccess = true,
                Data = new List<StatisticSessionPaymentMethodResDto>(),
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock
                .Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            _paymentRepositoryMock
                .Setup(x => x.StatisticSessionPaymentMethodByCustomerAsync(It.IsAny<Guid>()))
                .ReturnsAsync(statisticReturn);

            // Act
            var result = await _paymentService.StatisticSessionPaymentMethodByCustomerAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // StatisticSessionPaymentMethodAsync
        // Successful
        [Fact]
        public async Task StatisticSessionPaymentMethodAsync_ShouldReturnSuccess()
        {
            // Arrange
            var statisticReturn = new Return<IEnumerable<StatisticSessionPaymentMethodResDto>>
            {
                IsSuccess = true,
                Data = new List<StatisticSessionPaymentMethodResDto>(),
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@localhost.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock
                .Setup(x => x.StatisticSessionPaymentMethodAsync())
                .ReturnsAsync(statisticReturn);

            // Act
            var result = await _paymentService.StatisticSessionPaymentMethodAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task StatisticSessionPaymentMethodAsync_ShouldReturnFailure_WhenAuthenticationFailed()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _paymentService.StatisticSessionPaymentMethodAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }
        
        [Fact]
        public async Task StatisticSessionPaymentMethodAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var statisticReturn = new Return<IEnumerable<StatisticSessionPaymentMethodResDto>>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@localhost.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock
                .Setup(x => x.StatisticSessionPaymentMethodAsync())
                .ReturnsAsync(statisticReturn);

            // Act
            var result = await _paymentService.StatisticSessionPaymentMethodAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetStatisticPaymentTodayForGateAsync
        // Successful
        [Fact]
        public async Task GetStatisticPaymentTodayForGateAsync_ShouldReturnSuccess()
        {
            // Arrange
            var expectedStatistic = new StatisticPaymentTodayResDto
            {
                TotalCashPayment = 20,
                TotalWalletPayment = 30,
            };

            var statisticReturn = new Return<StatisticPaymentTodayResDto>
            {
                IsSuccess = true,
                Data = expectedStatistic,
                Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
            };

            var gateId = Guid.NewGuid();
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(1);

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@localhost.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock
                .Setup(x => x.GetStatisticPaymentTodayForGateAsync(gateId, startDate, endDate))
                .ReturnsAsync(statisticReturn);

            // Act
            var result = await _paymentService.GetStatisticPaymentTodayForGateAsync(gateId, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);

            Assert.Equal(expectedStatistic.TotalCashPayment, result.Data.TotalCashPayment);
            Assert.Equal(expectedStatistic.TotalWalletPayment, result.Data.TotalWalletPayment);
        }

        // Failure
        [Fact]
        public async Task GetStatisticPaymentTodayForGateAsync_ShouldReturnFailure_WhenAuthenticationFailed()
        {
            // Arrange
            var gateId = Guid.NewGuid();
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(1);

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _paymentService.GetStatisticPaymentTodayForGateAsync(gateId, startDate, endDate);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetStatisticPaymentTodayForGateAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var statisticReturn = new Return<StatisticPaymentTodayResDto>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            var gateId = Guid.NewGuid();
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(1);

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@localhost.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _paymentRepositoryMock
                .Setup(x => x.GetStatisticPaymentTodayForGateAsync(gateId, startDate, endDate))
                .ReturnsAsync(statisticReturn);

            // Act
            var result = await _paymentService.GetStatisticPaymentTodayForGateAsync(gateId, startDate, endDate);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }
    }
}
