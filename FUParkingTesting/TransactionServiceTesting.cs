using FirebaseService;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Firebase;
using FUParkingModel.RequestObject.Transaction;
using FUParkingModel.ResponseObject.ParkingArea;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService;
using FUParkingService.Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;
using Transaction = FUParkingModel.Object.Transaction;

namespace FUParkingTesting
{
    public class TransactionServiceTesting
    {
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IHelpperService> _helpperServiceMock;
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IWalletRepository> _walletRepositoryMock;
        private readonly Mock<ILogger<TransactionService>> _loggerMock;
        private readonly Mock<IFirebaseService> _firebaseServiceMock;
        private readonly TransactionService _transactionService;

        public TransactionServiceTesting()
        {
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _helpperServiceMock = new Mock<IHelpperService>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _walletRepositoryMock = new Mock<IWalletRepository>();
            _loggerMock = new Mock<ILogger<TransactionService>>();
            _firebaseServiceMock = new Mock<IFirebaseService>();
            _transactionService = new TransactionService(_transactionRepositoryMock.Object, _helpperServiceMock.Object, _customerRepositoryMock.Object, _walletRepositoryMock.Object, _loggerMock.Object, _firebaseServiceMock.Object);
        }

        // GetListTransactionPaymentAsync
        // Failure
        [Fact]
        public async Task GetListTransactionPaymentAsync_ShouldReturnFailure_WhenAuthenticationFails()
        {
            // Arrange
            var req = new GetListObjectWithFillerAttributeAndDateReqDto();

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _transactionService.GetListTransactionPaymentAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetListTransactionPaymentAsync_ShouldReturnsSuccess_WhenRecordsFound()
        {
            // Arrange
            var req = new GetListObjectWithFillerAttributeAndDateReqDto();

            var transactions = new List<Transaction>
            {
                new() {
                    Amount = 100,
                    Wallet = new Wallet
                    {
                        Customer = new Customer
                        {
                            Email = "customer@example.com",
                            FullName = "customer",
                            StatusCustomer = StatusUserEnum.ACTIVE
                        },
                        WalletType = "Main"
                    },
                    Deposit = new Deposit
                    {
                        Package = new Package
                        {
                            Name = "Test Package",
                            PackageStatus = StatusPackageEnum.ACTIVE
                        }
                    },
                    Payment = new Payment
                    {
                        PaymentMethod = new PaymentMethod
                        {
                            Name = "WALLET"
                        },
                        TotalPrice = 2000
                    },
                    TransactionDescription = "Test Transaction",
                    TransactionStatus = StatusTransactionEnum.SUCCEED,
                }
            };

            var listReturn = new Return<IEnumerable<Transaction>>
            {
                Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                IsSuccess = true,
                Data = transactions,
                TotalRecord = 1
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

            _transactionRepositoryMock.Setup(x => x.GetListTransactionPaymentAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(listReturn);

            // Act
            var result = await _transactionService.GetListTransactionPaymentAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListTransactionPaymentAsync_ShouldReturnsFailure_WhenServerError()
        {
            // Arrange
            var req = new GetListObjectWithFillerAttributeAndDateReqDto();

            var listReturn = new Return<IEnumerable<Transaction>>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false,
                Data = null,
                TotalRecord = 0
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

            _transactionRepositoryMock.Setup(x => x.GetListTransactionPaymentAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(listReturn);

            // Act
            var result = await _transactionService.GetListTransactionPaymentAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetListTransactionPaymentAsync_ShouldReturnsSuccess_WhenNoRecordsFound()
        {
            // Arrange
            var req = new GetListObjectWithFillerAttributeAndDateReqDto();

            var transactions = new List<Transaction>();

            var listReturn = new Return<IEnumerable<Transaction>>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = true,
                Data = transactions,
                TotalRecord = 0
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

            _transactionRepositoryMock.Setup(x => x.GetListTransactionPaymentAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(listReturn);

            // Act
            var result = await _transactionService.GetListTransactionPaymentAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // GetRevenueTodayAsync
        // Successful
        [Fact]
        public async Task GetRevenueTodayAsync_ShouldReturnsSuccess_WhenRecordsFound()
        {
            // Arrange
            var revenueReturn = new Return<int>
            {
                Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                IsSuccess = true,
                Data = 1000
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

            _transactionRepositoryMock
                .Setup(x => x.GetRevenueTodayAsync())
                .ReturnsAsync(revenueReturn);

            // Act
            var result = await _transactionService.GetRevenueTodayAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetRevenueTodayAsync_ShouldReturnFailure_WhenAuthenticationFails()
        {
            // Arrange
            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _transactionService.GetRevenueTodayAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetRevenueTodayAsync_ShouldReturnsFailure_WhenServerError()
        {
            // Arrange
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

            _transactionRepositoryMock
                .Setup(x => x.GetRevenueTodayAsync())
                .ReturnsAsync(new Return<int>
                {
                    IsSuccess = false,
                    Data = 0,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _transactionService.GetRevenueTodayAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetRevenueTodayAsync_ShouldReturnsFailure_WhenRecordNotFound()
        {
            // Arrange
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

            _transactionRepositoryMock
                .Setup(x => x.GetRevenueTodayAsync())
                .ReturnsAsync(new Return<int>
                {
                    IsSuccess = false,
                    Data = 0,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _transactionService.GetRevenueTodayAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetListStatisticParkingAreaRevenueAsync
        // Successful
        [Fact]
        public async Task GetListStatisticParkingAreaRevenueAsync_ShouldReturnsSuccess_WhenRecordsFound()
        {
            // Arrange
            var list = new List<StatisticParkingAreaRevenueResDto>
            {
                new() {
                    ParkingArea = new GetParkingAreaOptionResDto
                    {
                        Name = "FPTU 1",
                        Description = "FPTU"
                    },
                    Revenue = 1000
                }
            };

            var listReturn = new Return<IEnumerable<StatisticParkingAreaRevenueResDto>>
            {
                Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                Data = list,
                IsSuccess = true
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

            _transactionRepositoryMock
                .Setup(X => X.GetListStatisticParkingAreaRevenueAsync())
                .ReturnsAsync(listReturn);

            // Act
            var result = await _transactionService.GetListStatisticParkingAreaRevenueAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListStatisticParkingAreaRevenueAsync_ShouldReturnFailure_WhenAuthenticationFails()
        {
            // Arrange
            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _transactionService.GetListStatisticParkingAreaRevenueAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure 
        [Fact]
        public async Task GetListStatisticParkingAreaRevenueAsync_ShouldReturnsFailure_WhenServerError()
        {
            // Arrange
            var listReturn = new Return<IEnumerable<StatisticParkingAreaRevenueResDto>>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                Data = null,
                IsSuccess = false
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

            _transactionRepositoryMock
                .Setup(X => X.GetListStatisticParkingAreaRevenueAsync())
                .ReturnsAsync(listReturn);

            // Act
            var result = await _transactionService.GetListStatisticParkingAreaRevenueAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetListStatisticRevenueParkingAreasDetailsAsync
        // Successful
        [Fact]
        public async Task GetListStatisticRevenueParkingAreasDetailsAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var req = new GetListObjectWithFillerDateAndSearchInputResDto();

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

            _transactionRepositoryMock
                .Setup(x => x.GetListStatisticRevenueParkingAreasDetailsAsync(req))
                .ReturnsAsync(new Return<IEnumerable<StatisticRevenueParkingAreasDetailsResDto>>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data =
                    [
                        new StatisticRevenueParkingAreasDetailsResDto
                        {
                            ParkingAreaName = "FPTU 1",
                            RevenueApp = 100,
                            RevenueOther = 100,
                            RevenueTotal = 100
                        }
                    ]
                });

            // Act
            var result = await _transactionService.GetListStatisticRevenueParkingAreasDetailsAsync(req);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListStatisticRevenueParkingAreasDetailsAsync_ShouldReturnFailure_WhenAuthenticationFails()
        {
            // Arrange
            var req = new GetListObjectWithFillerDateAndSearchInputResDto();

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _transactionService.GetListStatisticRevenueParkingAreasDetailsAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListStatisticRevenueParkingAreasDetailsAsync_ShouldReturnsFailure_WhenServerError()
        {
            // Arrange
            var req = new GetListObjectWithFillerDateAndSearchInputResDto();

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

            _transactionRepositoryMock
                .Setup(x => x.GetListStatisticRevenueParkingAreasDetailsAsync(req))
                .ReturnsAsync(new Return<IEnumerable<StatisticRevenueParkingAreasDetailsResDto>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    Data = null
                });

            // Act
            var result = await _transactionService.GetListStatisticRevenueParkingAreasDetailsAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // TopUpCustomerWalletByUserAsync
        // Successful
        [Fact]
        public async Task TopUpCustomerWalletByUserAsync_ShouldReturnSuccess()
        {
            // Arrange
            var req = new TopUpCustomerWalletByUserReqDto { CustomerId = Guid.NewGuid(), Amount = 1000 };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(req.CustomerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer 
                    { 
                        Id = req.CustomerId, 
                        FullName = "customer",
                        Email = "customer@gmail.com",
                        StatusCustomer = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(req.CustomerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet 
                    { 
                        Balance = 5000,
                        WalletType = WalletType.MAIN
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.UpdateWalletAsync(It.IsAny<Wallet>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            _transactionRepositoryMock.Setup(x => x.CreateTransactionAsync(It.IsAny<Transaction>()))
                .ReturnsAsync(new Return<Transaction>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                });


            // Act
            var result = await _transactionService.TopUpCustomerWalletByUserAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task TopUpCustomerWalletByUserAsync_ShouldReturnFailure_WhenTransactionCreationFails()
        {
            // Arrange
            var req = new TopUpCustomerWalletByUserReqDto { CustomerId = Guid.NewGuid(), Amount = 1000 };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(req.CustomerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = req.CustomerId,
                        FullName = "customer",
                        Email = "customer@gmail.com",
                        StatusCustomer = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(req.CustomerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet
                    {
                        Balance = 5000,
                        WalletType = WalletType.MAIN
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.UpdateWalletAsync(It.IsAny<Wallet>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            _transactionRepositoryMock.Setup(x => x.CreateTransactionAsync(It.IsAny<FUParkingModel.Object.Transaction>()))
                .ReturnsAsync(new Return<FUParkingModel.Object.Transaction>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _transactionService.TopUpCustomerWalletByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task TopUpCustomerWalletByUserAsync_ShouldReturnFailure_WhenWalletUpdateFails()
        {
            // Arrange
            var req = new TopUpCustomerWalletByUserReqDto { CustomerId = Guid.NewGuid(), Amount = 1000 };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(req.CustomerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = req.CustomerId,
                        FullName = "customer",
                        Email = "customer@gmail.com",
                        StatusCustomer = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(req.CustomerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet
                    {
                        Balance = 5000,
                        WalletType = WalletType.MAIN
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.UpdateWalletAsync(It.IsAny<Wallet>()))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _transactionService.TopUpCustomerWalletByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task TopUpCustomerWalletByUserAsync_ShouldReturnFailure_WhenWalletNotFound()
        {
            // Arrange
            var req = new TopUpCustomerWalletByUserReqDto { CustomerId = Guid.NewGuid(), Amount = 1000 };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(req.CustomerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = req.CustomerId,
                        FullName = "customer",
                        Email = "customer@gmail.com",
                        StatusCustomer = StatusUserEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(req.CustomerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _transactionService.TopUpCustomerWalletByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task TopUpCustomerWalletByUserAsync_ShouldReturnFailure_WhenCustomerNotFound()
        {
            // Arrange
            var req = new TopUpCustomerWalletByUserReqDto { CustomerId = Guid.NewGuid(), Amount = 1000 };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(req.CustomerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _transactionService.TopUpCustomerWalletByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task TopUpCustomerWalletByUserAsync_ShouldReturnFailure_WhenAuthorizationFails()
        {
            // Arrange
            var req = new TopUpCustomerWalletByUserReqDto { CustomerId = Guid.NewGuid(), Amount = 1000 };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                });

            // Act
            var result = await _transactionService.TopUpCustomerWalletByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // GetListStatisticRevenueOfParkingSystemAsync
        // Failure
        [Fact]
        public async Task GetListStatisticRevenueOfParkingSystemAsync_ShouldReturnFailure_WhenAuthorizationFails()
        {
            // Arrange
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                });

            // Act
            var result = await _transactionService.GetListStatisticRevenueOfParkingSystemAsync(startDate, endDate);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListStatisticRevenueOfParkingSystemAsync_ShouldReturnFailure_WhenGetFails()
        {
            // Arrange
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _transactionRepositoryMock.Setup(x => x.GetListStatisticRevenueOfParkingSystemAsync(startDate, endDate))
                .ReturnsAsync(new Return<IEnumerable<StatisticRevenueOfParkingSystemResDto>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                });

            // Act
            var result = await _transactionService.GetListStatisticRevenueOfParkingSystemAsync(startDate, endDate);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetListStatisticRevenueOfParkingSystemAsync_ShouldReturnSuccess_WhenNoStatisticsFound()
        {
            // Arrange
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _transactionRepositoryMock.Setup(x => x.GetListStatisticRevenueOfParkingSystemAsync(startDate, endDate))
                .ReturnsAsync(new Return<IEnumerable<StatisticRevenueOfParkingSystemResDto>>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _transactionService.GetListStatisticRevenueOfParkingSystemAsync(startDate, endDate);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetListStatisticRevenueOfParkingSystemAsync_ShouldReturnSuccess_WhenStatisticsFound()
        {
            // Arrange
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;
            var statistics = new List<StatisticRevenueOfParkingSystemResDto>
            {
                new StatisticRevenueOfParkingSystemResDto
                {
                    AverageRevenue = 10000,
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "user",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _transactionRepositoryMock.Setup(x => x.GetListStatisticRevenueOfParkingSystemAsync(startDate, endDate))
                .ReturnsAsync(new Return<IEnumerable<StatisticRevenueOfParkingSystemResDto>>
                {
                    IsSuccess = true,
                    Data = statistics,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _transactionService.GetListStatisticRevenueOfParkingSystemAsync(startDate, endDate);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

    }
}
