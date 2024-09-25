using Castle.Core.Resource;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
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
    public class WalletServiceTesting
    {
        private readonly Mock<IWalletRepository> _walletRepositoryMock = new();
        private readonly Mock<IHelpperService> _helpperServiceMock = new();
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock = new();
        private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
        private readonly WalletService _walletService;

        public WalletServiceTesting()
        {
            _helpperServiceMock = new Mock<IHelpperService>();
            _walletRepositoryMock = new Mock<IWalletRepository>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _walletService = new WalletService(_walletRepositoryMock.Object, _helpperServiceMock.Object, _transactionRepositoryMock.Object, _customerRepositoryMock.Object);
        }

        // GetTransactionWalletExtraAsync
        // Successful
        [Fact]
        public async Task GetTransactionWalletExtraAsync_ShouldReturnSuccess()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var walletId = Guid.NewGuid();

            var request = new GetListObjectWithFillerDateReqDto();

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer> 
                { 
                    IsSuccess = true, 
                    Data = new Customer
                    {   
                        Id = customerId,
                        Email = "customer@gmail.com",
                        FullName = "Customer",  
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetExtraWalletByCustomerId(customerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Wallet 
                    { 
                        Id = walletId,
                        Balance = 1000,
                        WalletType = WalletType.MAIN,
                    }
                });

            var transactions = new List<Transaction>
            {
                new() { 
                    Amount = 100, 
                    TransactionDescription = "Test", 
                    TransactionStatus = StatusTransactionEnum.SUCCEED,
                },
            };

            _transactionRepositoryMock.Setup(x => x.GetTransByWalletIdAsync(walletId, request.PageSize, request.PageIndex, request.StartDate, request.EndDate))
                .ReturnsAsync(new Return<IEnumerable<Transaction>>
                {
                    IsSuccess = true,
                    Data = transactions,
                    TotalRecord = transactions.Count,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _walletService.GetTransactionWalletExtraAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetTransactionWalletExtraAsync_ShouldReturnFailure_WhenAuthenticationFailed()
        {
            // Arrange
            var request = new GetListObjectWithFillerDateReqDto();

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _walletService.GetTransactionWalletExtraAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetTransactionWalletExtraAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var walletId = Guid.NewGuid();

            var request = new GetListObjectWithFillerDateReqDto();

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = customerId,
                        Email = "customer@gmail.com",
                        FullName = "Customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetExtraWalletByCustomerId(customerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Wallet
                    {
                        Id = walletId,
                        Balance = 1000,
                        WalletType = WalletType.MAIN,
                    }
                });

            _transactionRepositoryMock.Setup(x => x.GetTransByWalletIdAsync(walletId, request.PageSize, request.PageIndex, request.StartDate, request.EndDate))
                .ReturnsAsync(new Return<IEnumerable<Transaction>>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _walletService.GetTransactionWalletExtraAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetTransactionWalletMainAsync
        // Successful
        [Fact]
        public async Task GetTransactionWalletMainAsync_ShouldReturnSuccess_WhenDataIsValid()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var req = new GetListObjectWithFillerDateReqDto
            {
                PageIndex = 1,
                PageSize = 10,
                StartDate = DateTime.Now.AddDays(-30),
                EndDate = DateTime.Now
            };

            var transactions = new Return<IEnumerable<Transaction>>
            {
                Data =
                [
                    new Transaction
                    {
                        Id = Guid.NewGuid(),
                        Amount = 1000,
                        TransactionDescription = "Deposit",
                        TransactionStatus = "Success",
                        CreatedDate = DateTime.Now.AddDays(-1),
                        DepositId = Guid.NewGuid()
                    }
                ],
                IsSuccess = true,
                TotalRecord = 1,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    { 
                        Id = customerId,
                        Email = "customer@gmail.com",
                        FullName = "Customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(customerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet 
                    { 
                        Id = walletId,
                        WalletType = WalletType.MAIN,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _transactionRepositoryMock.Setup(x => x.GetTransByWalletIdAsync(walletId, req.PageSize, req.PageIndex, req.StartDate, req.EndDate))
                .ReturnsAsync(transactions);

            // Act
            var result = await _walletService.GetTransactionWalletMainAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetTransactionWalletMainAsync_ShouldReturnFailure_WhenAuthenticationFails()
        {
            // Arrange
            var req = new GetListObjectWithFillerDateReqDto();

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _walletService.GetTransactionWalletMainAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetTransactionWalletMainAsync_ShouldReturnFailure_WhenWalletRetrievalFails()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var req = new GetListObjectWithFillerDateReqDto
            {
                PageIndex = 1,
                PageSize = 10,
                StartDate = DateTime.Now.AddDays(-30),
                EndDate = DateTime.Now
            };

            var transactions = new Return<IEnumerable<Transaction>>
            {
                Data = null,
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = customerId,
                        Email = "customer@gmail.com",
                        FullName = "Customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(customerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet
                    {
                        Id = walletId,
                        WalletType = WalletType.MAIN,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _transactionRepositoryMock.Setup(x => x.GetTransByWalletIdAsync(walletId, req.PageSize, req.PageIndex, req.StartDate, req.EndDate))
                .ReturnsAsync(transactions);

            // Act
            var result = await _walletService.GetTransactionWalletMainAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetBalanceWalletMainAsync
        // Successful
        [Fact]
        public async Task GetBalanceWalletMainAsync_ShouldReturnSuccess()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var balance = 1000;
            var walletId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = customerId,
                        Email = "customer@gmail.com",
                        FullName = "Customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(customerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Wallet
                    {
                        Id = walletId,
                        WalletType = WalletType.MAIN,
                        Balance = balance   
                    },
                });

            // Act
            var result = await _walletService.GetBalanceWalletMainAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetBalanceWalletMainAsync_ShouldReturnFailure_WhenAuthenticationFails()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _walletService.GetBalanceWalletMainAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetBalanceWalletMainAsync_ShouldReturnFailure_WhenWalletRetrievalFails()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = customerId,
                        Email = "customer@gmail.com",
                        FullName = "Customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(customerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                });

            // Act
            var result = await _walletService.GetBalanceWalletMainAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetBalanceWalletExtraAsync
        // Successful
        [Fact]
        public async Task GetBalanceWalletExtraAsync_ShouldReturnSuccess_WhenWalletExists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var balance = 2000;
            var expDate = DateTime.UtcNow.AddDays(30);

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = customerId,
                        Email = "customer@gmail.com",
                        FullName = "Customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetExtraWalletByCustomerId(customerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Wallet 
                    { 
                        Balance = balance, 
                        EXPDate = expDate,
                        WalletType = WalletType.EXTRA   
                    }
                });

            // Act
            var result = await _walletService.GetBalanceWalletExtraAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetBalanceWalletExtraAsync_ShouldReturnFailure_WhenAuthenticationFails()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _walletService.GetBalanceWalletExtraAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetBalanceWalletExtraAsync_ShouldReturnFailure_WhenWalletRetrievalFails()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            //var balance = 2000;
            var expDate = DateTime.UtcNow.AddDays(30);

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Id = customerId,
                        Email = "customer@gmail.com",
                        FullName = "Customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetExtraWalletByCustomerId(customerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    Data = null
                });

            // Act
            var result = await _walletService.GetBalanceWalletExtraAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetBalanceWalletMainExtraAsync
        // Failure
        [Fact]
        public async Task GetBalanceWalletMainExtraAsync_ShouldReturnFailure_WhenAuthorizationFails()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                });

            // Act
            var result = await _walletService.GetBalanceWalletMainExtraAsync(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetBalanceWalletMainExtraAsync_ShouldReturnFailure_WhenCustomerNotFound()
        {
            // Arrange
            var customerId = Guid.NewGuid();

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

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _walletService.GetBalanceWalletMainExtraAsync(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetBalanceWalletMainExtraAsync_ShouldReturnFailure_WhenCustomerIsNotPaidType()
        {
            // Arrange
            var customerId = Guid.NewGuid();

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

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer 
                    { 
                        CustomerType = new CustomerType 
                        { 
                            Name = CustomerTypeEnum.FREE,
                            Description = "Free Customer"
                        },
                        StatusCustomer = StatusCustomerEnum.ACTIVE,
                        Email = "customer@gmail.com",
                        FullName = "Customer",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _walletService.GetBalanceWalletMainExtraAsync(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.MUST_BE_CUSTOMER_PAID, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetBalanceWalletMainExtraAsync_ShouldReturnFailure_WhenMainWalletNotFound()
        {
            // Arrange
            var customerId = Guid.NewGuid();

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

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        CustomerType = new CustomerType
                        {
                            Name = CustomerTypeEnum.PAID,
                            Description = "Paid Customer"
                        },
                        StatusCustomer = StatusCustomerEnum.ACTIVE,
                        Email = "customer@gmail.com",
                        FullName = "Customer",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(customerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _walletService.GetBalanceWalletMainExtraAsync(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetBalanceWalletMainExtraAsync_ShouldReturnFailure_WhenExtraWalletNotFound()
        {
            // Arrange
            var customerId = Guid.NewGuid();

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

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        CustomerType = new CustomerType
                        {
                            Name = CustomerTypeEnum.PAID,
                            Description = "Paid Customer"
                        },
                        StatusCustomer = StatusCustomerEnum.ACTIVE,
                        Email = "customer@gmail.com",
                        FullName = "Customer",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(customerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet 
                    { 
                        Balance = 1000,
                        WalletType = WalletType.MAIN
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetExtraWalletByCustomerId(customerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _walletService.GetBalanceWalletMainExtraAsync(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetBalanceWalletMainExtraAsync_ShouldReturnSuccess_WhenWalletBalancesRetrievedSuccessfully()
        {
            // Arrange
            var customerId = Guid.NewGuid();

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

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        CustomerType = new CustomerType
                        {
                            Name = CustomerTypeEnum.PAID,
                            Description = "Paid Customer"
                        },
                        StatusCustomer = StatusCustomerEnum.ACTIVE,
                        Email = "customer@gmail.com",
                        FullName = "Customer",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetMainWalletByCustomerId(customerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet 
                    { 
                        Balance = 1000,
                        WalletType = WalletType.MAIN
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _walletRepositoryMock.Setup(x => x.GetExtraWalletByCustomerId(customerId))
                .ReturnsAsync(new Return<Wallet>
                {
                    IsSuccess = true,
                    Data = new Wallet 
                    { 
                        Balance = 500, 
                        EXPDate = DateTime.Now.AddYears(1),
                        WalletType = WalletType.EXTRA
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _walletService.GetBalanceWalletMainExtraAsync(customerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }
    }
}
