using FUParkingRepository.Interface;
using FUParkingService.Interface;
using FUParkingService;
using Moq;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using Xunit;
using FUParkingModel.RequestObject.Customer;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ResponseObject.Session;
using Castle.Core.Resource;
using FirebaseService;
using FUParkingService.MailService;
using Microsoft.Extensions.Logging;

namespace FUParkingTesting
{
    public class CustomerServiceTesting
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IHelpperService> _helpperServiceMock;
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
        private readonly Mock<IWalletRepository> _walletRepositoryMock;
        private readonly Mock<IFirebaseService> _firebaseService;
        private readonly Mock<IMailService> _mailService;
        private readonly Mock<ILogger<CustomerService>> _logger;
        private readonly CustomerService _customerService;

        public CustomerServiceTesting()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _helpperServiceMock = new Mock<IHelpperService>();
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();
            _walletRepositoryMock = new Mock<IWalletRepository>();
            _firebaseService = new Mock<IFirebaseService>();
            _mailService = new Mock<IMailService>();
            _logger = new Mock<ILogger<CustomerService>>();
            _customerService = new CustomerService(_customerRepositoryMock.Object, _helpperServiceMock.Object, _vehicleRepositoryMock.Object, _walletRepositoryMock.Object, _firebaseService.Object, _mailService.Object, _logger.Object);
        }

        // ChangeStatusCustomerAsync
        // Successfull
        [Fact]
        public async Task ChangeStatusCustomerAsync_ShouldReturnSuccess_WhenCustomerIsActive()
        {
            // Arrange
            var customerEmail = "customer@localhost.com";
            var customerName = "customer";
            var customer = new Customer
            {
                StatusCustomer = StatusCustomerEnum.INACTIVE,
                Email = customerEmail,
                FullName = customerName,
            };

            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };

            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };
            var userReturn = new Return<User> { IsSuccess = true, Data = user, Message = SuccessfullyEnumServer.FOUND_OBJECT };
            var customerReturn = new Return<Customer> { IsSuccess = true, Data = customer, Message = SuccessfullyEnumServer.FOUND_OBJECT };
            var updateReturn = new Return<Customer> { IsSuccess = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
            var req = new ChangeStatusCustomerReqDto { CustomerId = customer.Id, IsActive = true };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(req.CustomerId)).ReturnsAsync(customerReturn);
            _customerRepositoryMock.Setup(x => x.UpdateCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _customerService.ChangeStatusCustomerAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task ChangeStatusCustomerAsync_ShouldReturnError_WhenCustomerIsActive()
        {
            // Arrange
            var customerEmail = "customer@localhost.com";
            var customerName = "Customer";
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                StatusCustomer = StatusCustomerEnum.ACTIVE,
                Email = customerEmail,
                FullName = customerName,
            };
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "Supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };
            var userReturn = new Return<User> { IsSuccess = true, Data = user, Message = SuccessfullyEnumServer.FOUND_OBJECT };
            var customerReturn = new Return<Customer> { IsSuccess = true, Data = customer, Message = SuccessfullyEnumServer.FOUND_OBJECT };
            var req = new ChangeStatusCustomerReqDto { CustomerId = customer.Id, IsActive = true };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(req.CustomerId)).ReturnsAsync(customerReturn);

            // Act
            var result = await _customerService.ChangeStatusCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.STATUS_IS_ALREADY_APPLY, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task ChangeStatusCustomerAsync_ShouldReturnError_WhenUserValidationFails()
        {
            // Arrange
            var userEmail = "user@localhost.com";
            var userName = "User";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = Guid.NewGuid(),
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION
            };

            var req = new ChangeStatusCustomerReqDto
            {
                CustomerId = Guid.NewGuid(),
                IsActive = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            // Act
            var result = await _customerService.ChangeStatusCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);

            // Verify the customer repository methods were not called
            _customerRepositoryMock.Verify(x => x.GetCustomerByIdAsync(It.IsAny<Guid>()), Times.Never);
            _customerRepositoryMock.Verify(x => x.UpdateCustomerAsync(It.IsAny<Customer>()), Times.Never);
        }

        // ReturnError
        [Fact]
        public async Task ChangeStatusCustomerAsync_ShouldReturnError_WhenCustomerNotExist()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var supervisorEmail = "supervisor@localhost.com";
            var supervisorName = "Supervisor";
            var supervisor = new User
            {
                Email = supervisorEmail,
                FullName = supervisorName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var supervisorReturn = new Return<User>
            {
                IsSuccess = true,
                Data = supervisor,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var nonExistentCustomerId = Guid.NewGuid();
            var req = new ChangeStatusCustomerReqDto
            {
                CustomerId = nonExistentCustomerId,
                IsActive = true
            };

            var customerNotFoundReturn = new Return<Customer>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.CUSTOMER_NOT_EXIST
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(supervisorReturn);

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(nonExistentCustomerId))
                .ReturnsAsync(customerNotFoundReturn);

            // Act
            var result = await _customerService.ChangeStatusCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CUSTOMER_NOT_EXIST, result.Message);

            // Verify the customer update method was not called
            _customerRepositoryMock.Verify(x => x.UpdateCustomerAsync(It.IsAny<Customer>()), Times.Never);
        }

        // ReturnError
        [Fact]
        public async Task ChangeStatusCustomerAsync_ShouldReturnError_WhenCustomerIsInactiveAndRequestInactive()
        {
            // Arrange
            var customerEmail = "customer@localhost.com";
            var customerName = "Customer";
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                StatusCustomer = StatusCustomerEnum.INACTIVE,
                Email = customerEmail,
                FullName = customerName,
            };
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "Supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };
            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };
            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var req = new ChangeStatusCustomerReqDto
            {
                CustomerId = customer.Id,
                IsActive = false
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(req.CustomerId)).ReturnsAsync(customerReturn);

            // Act
            var result = await _customerService.ChangeStatusCustomerAsync(req);

            // Assert
            Assert.Equal(ErrorEnumApplication.STATUS_IS_ALREADY_APPLY, result.Message);
        }

        // Successfull
        [Fact]
        public async Task ChangeStatusCustomerAsync_ShouldReturnSuccess_WhenCustomerIsInactive()
        {
            // Arrange
            var customerEmail = "customer@localhost.com";
            var customerName = "customer";
            var customer = new Customer
            {
                StatusCustomer = StatusCustomerEnum.INACTIVE,
                Email = customerEmail,
                FullName = customerName,
            };
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };
            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };
            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };
            var updateReturn = new Return<Customer>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };
            var req = new ChangeStatusCustomerReqDto
            {
                CustomerId = customer.Id,
                IsActive = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(req.CustomerId)).ReturnsAsync(customerReturn);
            _customerRepositoryMock.Setup(x => x.UpdateCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _customerService.ChangeStatusCustomerAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // CreateCustomerAsync
        // ReturnError
        [Fact]
        public async Task CreateCustomerAsync_ShouldReturnError_WhenUserValidationFails()
        {
            // Arrange
            var req = new CustomerReqDto
            {
                Name = "Customer",
                Email = "customer@localhost.com"
            };

            var userEmail = "user@localhost.com";
            var userName = "User";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = Guid.NewGuid(),
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            // Act
            var result = await _customerService.CreateCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task CreateCustomerAsync_ShouldReturnError_WhenEmailExists()
        {
            // Arrange
            var req = new CustomerReqDto
            {
                Name = "Customer",
                Email = "existing@email.com"
            };

            var existingCustomer = new Customer
            {
                Email = "existing@email.com",
                FullName = "Customer",
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var emailExistsReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = existingCustomer,
                Message = ErrorEnumApplication.EMAIL_IS_EXIST
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR))
               .ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerByEmailAsync(req.Email))
               .ReturnsAsync(emailExistsReturn);


            // Act
            var result = await _customerService.CreateCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.EMAIL_IS_EXIST, result.Message);
        }

        // Successfull
        [Fact]
        public async Task CreateCustomerAsync_ShouldReturnSuccess()
        {
            // Arrange
            var customerReq = new CustomerReqDto
            {
                Name = "Customer",
                Email = "customer@localhost.com"
            };

            var customerFree = new CustomerType
            {
                Id = Guid.NewGuid(),
                Name = CustomerTypeEnum.FREE,
                Description = "Free Customer"
            };

            var existingCustomer = new Customer
            {
                Email = "existing@email.com",
                FullName = "Customer",
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            var newCustomer = new Customer
            {
                Email = customerReq.Email,
                FullName = customerReq.Name,
                StatusCustomer = StatusCustomerEnum.ACTIVE,
                CustomerTypeId = customerFree.Id
            };

            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerNotExistReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = existingCustomer,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
            };

            var customerFreeReturn = new Return<CustomerType>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = customerFree
            };

            var createReturn = new Return<Customer>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                Data = newCustomer
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR))
               .ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerByEmailAsync(customerReq.Email))
               .ReturnsAsync(customerNotExistReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerTypeByNameAsync(CustomerTypeEnum.FREE))
                .ReturnsAsync(customerFreeReturn);

            _customerRepositoryMock.Setup(c => c.CreateNewCustomerAsync(It.IsAny<Customer>()))
                .ReturnsAsync(createReturn);

            // Act
            var result = await _customerService.CreateCustomerAsync(customerReq);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task CreateCustomerAsync_ShouldReturnFailure_WhenFreeCustomerTypeNotFound()
        {
            // Arrange
            var customerReq = new CustomerReqDto
            {
                Name = "Customer",
                Email = "customer@localhost.com"
            };

            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerNotFoundReturn = new Return<Customer>
            {
                IsSuccess = true,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var customerTypeFreeNotFoundReturn = new Return<CustomerType>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.GetCustomerByEmailAsync(customerReq.Email))
                .ReturnsAsync(customerNotFoundReturn);
            _customerRepositoryMock.Setup(x => x.GetCustomerTypeByNameAsync(CustomerTypeEnum.FREE))
                .ReturnsAsync(customerTypeFreeNotFoundReturn);

            // Act
            var result = await _customerService.CreateCustomerAsync(customerReq);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task CreateCustomerAsync_ShouldReturnError_WhenCustomerTypeNotFound()
        {
            // Arrange
            var customerReq = new CustomerReqDto
            {
                Email = "newcustomer@gmail.com",
                Name = "New Customer"
            };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };
            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(x => x.GetCustomerByEmailAsync(customerReq.Email))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _customerRepositoryMock.Setup(x => x.GetCustomerTypeByNameAsync(CustomerTypeEnum.FREE))
                .ReturnsAsync(new Return<CustomerType>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _customerService.CreateCustomerAsync(customerReq);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }


        // GetCustomerByIdAsync
        // ReturnFailure
        [Fact]
        public async Task GetCustomerByIdAsync_ShouldReturnFailure_WhenUserIsNotValid()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var user = new User
            {
                Email = "abc@localhost.com",
                FullName = "abc",
                RoleId = Guid.NewGuid(),
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var invalidUserReturn = new Return<User>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                Data = user
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(invalidUserReturn);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        //Successfull
        [Fact]
        public async Task GetCustomerByIdAsync_ShouldReturnSuccess()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var customer = new Customer
            {
                Id = customerId,
                Email = "customer@localhost.com",
                FullName = "Customer",
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR))
               .ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(customerId))
               .ReturnsAsync(customerReturn);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert 
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task GetCustomerByIdAsync_ShouldReturnFailure_WhenGetCustomerFails()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR))
               .ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(customerId))
               .ReturnsAsync(customerReturn);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(It.IsAny<Guid>());

            // Assert 
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetListCustomerAsync
        // ReturnFailure
        [Fact]
        public async Task GetListCustomerAsync_ShouldReturnFailure_WhenUserIsNotValid()
        {
            // Arrange
            var userEmail = "user@localhost.com";
            var userName = "User";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = Guid.NewGuid(),
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
            };

            var req = new GetCustomersWithFillerReqDto();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            // Act
            var result = await _customerService.GetListCustomerAsync(req);

            // Assert 
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task GetListCustomerAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange 
            var listCustomerReturn = new Return<IEnumerable<Customer>>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            var req = new GetCustomersWithFillerReqDto();

            _customerRepositoryMock.Setup(r => r.GetListCustomerAsync(It.IsAny<GetCustomersWithFillerReqDto>()))
               .ReturnsAsync(listCustomerReturn);

            // Act
            var result = await _customerService.GetListCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Successfull
        [Fact]
        public async Task GetListCustomerAsync_ShouldReturnSuccess()
        {
            // Arrange
            var listCustomer = new List<Customer>
            {
                new() {
                    Id = Guid.NewGuid(),
                    Email = "customer@localhost.com",
                    FullName = "Customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            };

            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var listCustomerReturn = new Return<IEnumerable<Customer>>
            {
                IsSuccess = true,
                Data = listCustomer,
                Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
            };

            var req = new GetCustomersWithFillerReqDto();

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.GetListCustomerAsync(It.IsAny<GetCustomersWithFillerReqDto>()))
                .ReturnsAsync(listCustomerReturn);

            // Act
            var result = await _customerService.GetListCustomerAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Successfull
        [Fact]
        public async Task GetListCustomerAsync_ShouldReturnFailure_WhenNoCustomerFound()
        {
            // Arrange

            var listCustomer = new List<Customer>();

            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var listCustomerReturn = new Return<IEnumerable<Customer>>
            {
                Data = listCustomer,
                Message = ErrorEnumApplication.SERVER_ERROR,
            };

            var req = new GetCustomersWithFillerReqDto();

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.GetListCustomerAsync(It.IsAny<GetCustomersWithFillerReqDto>()))
                .ReturnsAsync(listCustomerReturn);

            // Act
            var result = await _customerService.GetListCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // StatisticCustomerAsync
        //Successfull
        [Fact]
        public async Task StatisticCustomerAsync_ShouldReturnFailure_WhenUserIsNotValid()
        {
            // Arrange
            var userEmail = "user@localhost.com";
            var userName = "User";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = Guid.NewGuid(),
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            // Act
            var result = await _customerService.StatisticCustomerAsync();

            // Assert 
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Successfull
        [Fact]
        public async Task StatisticCustomerAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var statisticData = new StatisticCustomerResDto();

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var statisticCustomerReturn = new Return<StatisticCustomerResDto>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR,
                Data = statisticData
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.StatisticCustomerAsync())
                .ReturnsAsync(statisticCustomerReturn);

            // Act
            var result = await _customerService.StatisticCustomerAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Successfull
        [Fact]
        public async Task StatisticCustomerAsync_ShouldReturnSuccess()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var statisticCustomerReturn = new Return<StatisticCustomerResDto>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.StatisticCustomerAsync())
                .ReturnsAsync(statisticCustomerReturn);

            // Act
            var result = await _customerService.StatisticCustomerAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // UpdateCustomerAccountAsync
        // ReturnFailure
        [Fact]
        public async Task UpdateCustomerAccountAsync_ShouldReturnFailure_WhenUserIsNotValid()
        {
            // Arrange
            var customer = new Customer
            {
                Email = "abc@localhost.com",
                FullName = "abc",
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            var userReturn = new Return<Customer>
            {
                IsSuccess = false,
                Data = customer,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
            };

            var req = new UpdateCustomerAccountReqDto
            {
                FullName = "John Doe"
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(userReturn);

            // Act
            var result = await _customerService.UpdateCustomerAccountAsync(req);

            // Assert 
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task UpdateCustomerAccountAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var customer = new Customer
            {
                Email = "abc@localhost.com",
                FullName = "abc",
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            var userReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var updateCustomerReturn = new Return<Customer>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR,
            };

            var req = new UpdateCustomerAccountReqDto
            {
                FullName = ""
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.UpdateCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(updateCustomerReturn);

            // Act
            var result = await _customerService.UpdateCustomerAccountAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Successfull
        [Fact]
        public async Task UpdateCustomerAccountAsync_ShouldReturnSuccess()
        {
            // Arrange
            var customerEmail = "customer@localhost.com";
            var customerName = "Customer";
            var newCustomerName = "New Customer";

            var account = new Customer
            {
                Email = customerEmail,
                FullName = customerName,
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            var userReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = account,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var updateReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = account,
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            var req = new UpdateCustomerAccountReqDto 
            { 
                FullName = newCustomerName 
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.UpdateCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _customerService.UpdateCustomerAccountAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // CreateNonPaidCustomerAsync
        // ReturnFailure
        [Fact]
        public async Task CreateNonPaidCustomerAsync_ShouldReturnFailure_WhenInValidUser()
        {
            // Arrange
            var userEmail = "user@localhost.com";
            var userName = "User";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = Guid.NewGuid(),
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Data = user,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION
            };

            var req = new CreateNonPaidCustomerReqDto
            {
                Name = "Customer",
                Email = "customer@localhost.com",
                Vehicles =
                [
                    new CreateVehiclesNonPriceResDto
                    {
                        PlateNumber = "29A12345",
                        VehicleTypeId = Guid.NewGuid()
                    }
                ]
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            // Act
            var result = await _customerService.CreateNonPaidCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Successfull
        [Fact]
        public async Task CreateNonPaidCustomerAsync_ShouldReturnSuccess_WhenCustomerCreatedSuccessfully()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var freeCustomerType = new CustomerType
            {
                Name = CustomerTypeEnum.FREE,
                Description = "Free Customer"
            };

            var existingCustomerReturn = new Return<Customer>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerFreeReturn = new Return<CustomerType>
            {
                IsSuccess = true,
                Data = freeCustomerType,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var req = new CreateNonPaidCustomerReqDto
            {
                Name = "Customer",
                Email = "customer@localhost.com",
                Vehicles =
                [
                    new CreateVehiclesNonPriceResDto
                    {
                        PlateNumber = "29A12345",
                        VehicleTypeId = Guid.NewGuid()
                    }
                ]
            };

            var newVehicleReturn = new Return<Vehicle>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
            };

            var existingVehicleReturn = new Return<Vehicle>
            {
                IsSuccess = false,
                Data = new Vehicle
                {
                    PlateNumber = req.Vehicles[0].PlateNumber,
                    StatusVehicle = StatusCustomerEnum.ACTIVE,
                },
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var createReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = new Customer
                {
                    Email = req.Email,
                    FullName = req.Name,
                    StatusCustomer = StatusUserEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.GetCustomerByEmailAsync(req.Email)).ReturnsAsync(existingCustomerReturn);
            _customerRepositoryMock.Setup(r => r.GetCustomerTypeByNameAsync(CustomerTypeEnum.FREE)).ReturnsAsync(customerFreeReturn);
            _customerRepositoryMock.Setup(x => x.CreateNewCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(createReturn);
            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(It.IsAny<String>())).ReturnsAsync(existingVehicleReturn);
            _vehicleRepositoryMock.Setup(x => x.CreateVehicleAsync(It.IsAny<Vehicle>())).ReturnsAsync(newVehicleReturn);

            // Act
            var result = await _customerService.CreateNonPaidCustomerAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task CreateNonPaidCustomerAsync_ShouldReturnFailure_WhenExistingEmail()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var req = new CreateNonPaidCustomerReqDto
            {
                Name = "Customer",
                Email = "customer@localhost.com",
                Vehicles =
                [
                    new CreateVehiclesNonPriceResDto
                    {
                        PlateNumber = "29A12345",
                        VehicleTypeId = Guid.NewGuid()
                    }
                ]
            };

            var existingCustomer = new Customer
            {
                Email = req.Email,
                FullName = "Existing Customer",
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            var existingCustomerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = existingCustomer
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.GetCustomerByEmailAsync(req.Email)).ReturnsAsync(existingCustomerReturn);

            // Act
            var result = await _customerService.CreateNonPaidCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.EMAIL_IS_EXIST, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task CreateNonPaidCustomerAsync_ShouldReturnFailure_WhenCustomerTypeNotFound()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var existingCustomerReturn = new Return<Customer>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerFreeReturn = new Return<CustomerType>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var req = new CreateNonPaidCustomerReqDto
            {
                Name = "Customer",
                Email = "customer@localhost.com",
                Vehicles =
                [
                    new CreateVehiclesNonPriceResDto
                    {
                        PlateNumber = "29A12345",
                        VehicleTypeId = Guid.NewGuid()
                    }
                ]
            }; 

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.GetCustomerByEmailAsync(req.Email)).ReturnsAsync(existingCustomerReturn);
            _customerRepositoryMock.Setup(r => r.GetCustomerTypeByNameAsync(CustomerTypeEnum.FREE)).ReturnsAsync(customerFreeReturn);
            // Act
            var result = await _customerService.CreateNonPaidCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task CreateNonPaidCustomerAsync_ShouldReturnFailure_WhenExistingPlateNumber()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var freeCustomerType = new CustomerType
            {
                Name = CustomerTypeEnum.FREE,
                Description = "Free customer"
            };

            var req = new CreateNonPaidCustomerReqDto
            {
                Name = "Customer",
                Email = "customer@localhost.com",
                Vehicles =
                [
                    new CreateVehiclesNonPriceResDto
                    {
                        PlateNumber = "29A12345",
                        VehicleTypeId = Guid.NewGuid()
                    }
                ]
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var existingCustomerReturn = new Return<Customer>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var customerTypeReturn = new Return<CustomerType>
            {
                IsSuccess = true,
                Data = freeCustomerType,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var createReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = new Customer
                {
                    Email = req.Email,
                    FullName = req.Name,
                    StatusCustomer = StatusUserEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
            };

            var existingVehicleReturn = new Return<Vehicle>
            {
                IsSuccess = true,
                Data = new Vehicle
                {
                    PlateNumber = req.Vehicles[0].PlateNumber,
                    StatusVehicle = StatusCustomerEnum.ACTIVE,
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(x => x.GetCustomerByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(existingCustomerReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerTypeByNameAsync(CustomerTypeEnum.FREE))
                .ReturnsAsync(customerTypeReturn);

            _customerRepositoryMock.Setup(x => x.CreateNewCustomerAsync(It.IsAny<Customer>()))
                .ReturnsAsync(createReturn);

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.Vehicles[0].PlateNumber))
                .ReturnsAsync(existingVehicleReturn);

            // Act
            var result = await _customerService.CreateNonPaidCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PLATE_NUMBER_IS_EXIST, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task CreateNonPaidCustomerAsync_ShouldReturnFailure_WhenCustomerInputNotAPlateNumber()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var freeCustomerType = new CustomerType
            {
                Name = CustomerTypeEnum.FREE,
                Description = "Free customer"
            };

            var req = new CreateNonPaidCustomerReqDto
            {
                Name = "Customer",
                Email = "customer@localhost.com",
                Vehicles =
                [
                    new CreateVehiclesNonPriceResDto
                    {
                        PlateNumber = "INVALID",
                        VehicleTypeId = Guid.NewGuid()
                    }
                ]
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerTypeReturn = new Return<CustomerType>
            {
                IsSuccess = true,
                Data = freeCustomerType,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var existingCustomerReturn = new Return<Customer>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var createReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = new Customer
                {
                    Email = req.Email,
                    FullName = req.Name,
                    StatusCustomer = StatusUserEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(x => x.GetCustomerByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(existingCustomerReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerTypeByNameAsync(CustomerTypeEnum.FREE))
                .ReturnsAsync(customerTypeReturn);

            _customerRepositoryMock.Setup(x => x.CreateNewCustomerAsync(It.IsAny<Customer>()))
                .ReturnsAsync(createReturn);

            // Act
            var result = await _customerService.CreateNonPaidCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_A_PLATE_NUMBER, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task CreateNonPaidCustomerAsync_ShouldReturnFailure_WhenCreateCustomerUnSuccessfully()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var freeCustomerType = new CustomerType
            {
                Name = CustomerTypeEnum.FREE,
                Description = "Free customer"
            };

            var req = new CreateNonPaidCustomerReqDto
            {
                Name = "",
                Email = "",
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerTypeReturn = new Return<CustomerType>
            {
                IsSuccess = true,
                Data = freeCustomerType,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var existingCustomerReturn = new Return<Customer>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var createReturn = new Return<Customer>
            {
                IsSuccess = false,
                Data = new Customer
                {
                    Email = req.Email,
                    FullName = req.Name,
                    StatusCustomer = StatusUserEnum.ACTIVE
                },
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(x => x.GetCustomerByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(existingCustomerReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerTypeByNameAsync(CustomerTypeEnum.FREE))
                .ReturnsAsync(customerTypeReturn);

            _customerRepositoryMock.Setup(x => x.CreateNewCustomerAsync(It.IsAny<Customer>()))
                .ReturnsAsync(createReturn);

            // Act
            var result = await _customerService.CreateNonPaidCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task CreateNonPaidCustomerAsync_ShouldReturnFailure_WhenCreateVehicleUnSuccessfully()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var freeCustomerType = new CustomerType
            {
                Name = CustomerTypeEnum.FREE,
                Description = "Free Customer"
            };

            var existingCustomerReturn = new Return<Customer>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerFreeReturn = new Return<CustomerType>
            {
                IsSuccess = true,
                Data = freeCustomerType,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var req = new CreateNonPaidCustomerReqDto
            {
                Name = "Customer",
                Email = "customer@localhost.com",
                Vehicles =
                [
                    new CreateVehiclesNonPriceResDto
                    {
                        PlateNumber = "29A12345",
                        VehicleTypeId = Guid.NewGuid()
                    }
                ]
            };

            var newVehicleReturn = new Return<Vehicle>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            var existingVehicleReturn = new Return<Vehicle>
            {
                IsSuccess = false,
                Data = new Vehicle
                {
                    PlateNumber = req.Vehicles[0].PlateNumber,
                    StatusVehicle = StatusCustomerEnum.ACTIVE,
                },
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var createCustomerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = new Customer
                {
                    Email = req.Email,
                    FullName = req.Name,
                    StatusCustomer = StatusUserEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.GetCustomerByEmailAsync(req.Email)).ReturnsAsync(existingCustomerReturn);
            _customerRepositoryMock.Setup(r => r.GetCustomerTypeByNameAsync(CustomerTypeEnum.FREE)).ReturnsAsync(customerFreeReturn);
            _customerRepositoryMock.Setup(x => x.CreateNewCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(createCustomerReturn);
            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(It.IsAny<String>())).ReturnsAsync(existingVehicleReturn);
            _vehicleRepositoryMock.Setup(x => x.CreateVehicleAsync(It.IsAny<Vehicle>())).ReturnsAsync(newVehicleReturn);

            // Act
            var result = await _customerService.CreateNonPaidCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetCustomerTypeByPlateNumberAsync
        // ReturnFailure 
        [Fact]
        public async Task GetCustomerTypeByPlateNumberAsync_ShouldReturnFailure_WhenInValidUser()
        {
            // Arrange
            var userEmail = "user@localhost.com";
            var userName = "User";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = Guid.NewGuid(),
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Data = user,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);

            // Act
            var result = await _customerService.GetCustomerTypeByPlateNumberAsync(It.IsAny<string>());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Successfull
        [Fact]
        public async Task GetCustomerTypeByPlateNumberAsync_ShouldReturnSuccess_WhenCustomerIsPaid()
        {
            // Arrange
            var plateNumber = "29A12345";

            var role = new Role { Name = RoleEnum.STAFF.ToString() };
            var userEmail = "staff@localhost.com";
            var userName = "staff";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var paid = new CustomerType
            {
                Name = CustomerTypeEnum.PAID.ToString(),
                Description = "Paid Customer"
            };

            var customer = new Customer
            {
                Email = "customer@localhost.com",
                FullName = "Customer",
                StatusCustomer = StatusCustomerEnum.ACTIVE,
                CustomerType = paid
            };

            var vehicle = new Vehicle
            {
                PlateNumber = plateNumber,
                StatusVehicle = StatusCustomerEnum.ACTIVE,
                Customer = customer
            };

            var vehicleReturn = new Return<Vehicle>
            {
                IsSuccess = true,
                Data = vehicle,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var dataReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer, 
                Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.GetCustomerByPlateNumberAsync(plateNumber)).ReturnsAsync(dataReturn);

            // Act
            var result = await _customerService.GetCustomerTypeByPlateNumberAsync(plateNumber);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
            Assert.Equal(CustomerTypeEnum.PAID, result.Data?.CustomerType);
        }

        // Successfull
        [Fact]
        public async Task GetCustomerTypeByPlateNumberAsync_ShouldReturnSuccess_WhenCustomerIsFree()
        {
            // Arrange
            var plateNumber = "29A12345";

            var role = new Role { Name = RoleEnum.STAFF.ToString() };
            var userEmail = "staff@localhost.com";
            var userName = "staff";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var free = new CustomerType
            {
                Name = CustomerTypeEnum.FREE.ToString(),
                Description = "Free Customer"
            };

            var customer = new Customer
            {
                Email = "customer@localhost.com",
                FullName = "Customer",
                StatusCustomer = StatusCustomerEnum.ACTIVE,
                CustomerType = free
            };

            var vehicle = new Vehicle
            {
                PlateNumber = plateNumber,
                StatusVehicle = StatusCustomerEnum.ACTIVE,
                Customer = customer
            };

            var vehicleReturn = new Return<Vehicle>
            {
                IsSuccess = true,
                Data = vehicle,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var dataReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.GetCustomerByPlateNumberAsync(plateNumber)).ReturnsAsync(dataReturn);

            // Act
            var result = await _customerService.GetCustomerTypeByPlateNumberAsync(plateNumber);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
            Assert.Equal(CustomerTypeEnum.FREE, result.Data?.CustomerType);
        }

        // Successfull
        [Fact]
        public async Task GetCustomerTypeByPlateNumberAsync_ShouldReturnSuccess_WhenCustomerIsGuest()
        {
            // Arrange
            var plateNumber = "29A12345";

            var role = new Role { Name = RoleEnum.STAFF.ToString() };
            var userEmail = "staff@localhost.com";
            var userName = "staff";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var dataReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = null,
                Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);
            _customerRepositoryMock.Setup(x => x.GetCustomerByPlateNumberAsync(plateNumber)).ReturnsAsync(dataReturn);

            // Act
            var result = await _customerService.GetCustomerTypeByPlateNumberAsync(plateNumber);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task GetCustomerTypeByPlateNumberAsync_ShouldReturnFailure_WhenInvalidPlateNumber()
        {
            // Arrange
            var plateNumber = "INVALID";

            var role = new Role { Name = RoleEnum.STAFF.ToString() };
            var userEmail = "staff@localhost.com";
            var userName = "staff";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);

            // Act
            var result = await _customerService.GetCustomerTypeByPlateNumberAsync(plateNumber);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_A_PLATE_NUMBER, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task GetCustomerTypeByPlateNumberAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var plateNumber = "29A12345";
            //var role = new Role { Name = RoleEnum.STAFF.ToString() };
            //var userEmail = "staff@localhost.com";
            //var userName = "staff";
            //var user = new User
            //{
            //    Email = userEmail,
            //    FullName = userName,
            //    RoleId = role.Id,
            //    PasswordHash = "",
            //    PasswordSalt = "",
            //    StatusUser = StatusUserEnum.ACTIVE
            //};
            // Act
            var result = await _customerService.GetCustomerTypeByPlateNumberAsync(plateNumber);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdateCustomerFCMTokenAsync
        // Successfull
        [Fact]
        public async Task UpdateCustomerFCMTokenAsync_ShouldReturnSuccess_WhenFCMTokenIsSame()
        {
            // Arrange
            var fcmToken = "testToken";
            var customer = new Customer 
            {
                Email = "",
                FullName = "",
                StatusCustomer = StatusCustomerEnum.ACTIVE,
                FCMToken = fcmToken 
            };
            var checkAuthReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(checkAuthReturn);

            // Act
            var result = await _customerService.UpdateCustomerFCMTokenAsync(fcmToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successfull
        [Fact]
        public async Task UpdateCustomerFCMTokenAsync_ShouldReturnSuccess_WhenTokenUpdatedSuccessfully()
        {
            // Arrange
            var fcmToken = "newToken";
            var customer = new Customer
            {
                Email = "",
                FullName = "",
                StatusCustomer = StatusCustomerEnum.ACTIVE,
                FCMToken = "oldToken"
            };

            var checkAuthReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var updateReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(checkAuthReturn);
            _customerRepositoryMock.Setup(x => x.UpdateCustomerAsync(customer)).ReturnsAsync(updateReturn);

            // Act
            var result = await _customerService.UpdateCustomerFCMTokenAsync(fcmToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task UpdateCustomerFCMTokenAsync_ShouldReturnFailure_WhenCustomerValidationFails()
        {
            // Arrange
            var fcmToken = "testToken"; 
            var customer = new Customer
            {
                Email = "",
                FullName = "",
                StatusCustomer = StatusCustomerEnum.ACTIVE,
                FCMToken = fcmToken
            };

            var checkAuthReturn = new Return<Customer>
            {
                IsSuccess = false,
                Data = customer,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(checkAuthReturn);

            // Act
            var result = await _customerService.UpdateCustomerFCMTokenAsync(fcmToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task UpdateCustomerFCMTokenAsync_ShouldReturnFailure_WhenUpdateFails()
        {
            // Arrange
            var fcmToken = "newToken";
            var customer = new Customer
            {
                Email = "",
                FullName = "",
                StatusCustomer = StatusCustomerEnum.ACTIVE,
                FCMToken = "oldToken"
            };

            var checkAuthReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var updateReturn = new Return<Customer>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(checkAuthReturn);
            _customerRepositoryMock.Setup(x => x.UpdateCustomerAsync(customer)).ReturnsAsync(updateReturn);

            // Act
            var result = await _customerService.UpdateCustomerFCMTokenAsync(fcmToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // DeleteCustomerByStaff
        // Successfull
        [Fact]
        public async Task DeleteCustomerByStaff_ShouldReturnSuccess_WhenAllOperationsAreSuccessful()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                Id = customerId,
                Email = "customer@localhost.com",
                FullName = "Customer",
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            var customerVehicle = new List<Vehicle>
                {
                    new() {
                        PlateNumber = "29A12345",
                        StatusVehicle = StatusCustomerEnum.ACTIVE
                    }
                };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var vehicleReturn = new Return<IEnumerable<Vehicle>>
            {
                IsSuccess = true,
                Data = customerVehicle,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var updateCustomerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            var updateVehicleReturn = new Return<Vehicle>
            {
                IsSuccess = true,
                Data = customerVehicle[0],
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(customerId)).ReturnsAsync(customerReturn);

            _vehicleRepositoryMock.Setup(r => r.GetAllCustomerVehicleByCustomerIdAsync(customerId)).ReturnsAsync(vehicleReturn);

            _customerRepositoryMock.Setup(r => r.UpdateCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(updateCustomerReturn);

            _vehicleRepositoryMock.Setup(r => r.UpdateVehicleAsync(It.IsAny<Vehicle>())).ReturnsAsync(updateVehicleReturn);

            // Act
            var result = await _customerService.DeleteCustomerByStaff(customerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task DeleteCustomerByStaff_ShouldReturnFailure_WhenUserIsNotAuthorized()
        {
            // Arrange
            var userEmail = "user@localhost.com";
            var userName = "user";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = Guid.NewGuid(),
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var customerId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Data = user,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            // Act
            var result = await _customerService.DeleteCustomerByStaff(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task DeleteCustomerByStaff_ShouldReturnFailure_WhenCustomerDoesNotExist()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                Id = customerId,
                Email = "customer@localhost.com",
                FullName = "Customer",
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            var customerVehicle = new List<Vehicle>
                {
                    new() {
                        PlateNumber = "29A12345",
                        StatusVehicle = StatusCustomerEnum.ACTIVE
                    }
                };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = false,
                Data = customer,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(customerId)).ReturnsAsync(customerReturn);

            // Act
            var result = await _customerService.DeleteCustomerByStaff(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CUSTOMER_NOT_EXIST, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task DeleteCustomerByStaff_ShouldReturnFailure_WhenUpdatingVehicleFails()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                Id = customerId,
                Email = "customer@localhost.com",
                FullName = "Customer",
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            var customerVehicle = new List<Vehicle>
                {
                    new() {
                        PlateNumber = "29A12345",
                        StatusVehicle = StatusCustomerEnum.ACTIVE
                    }
                };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var vehicleReturn = new Return<IEnumerable<Vehicle>>
            {
                IsSuccess = true,
                Data = customerVehicle,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var updateCustomerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            var updateVehicleReturn = new Return<Vehicle>
            {
                IsSuccess = false,
                Data = customerVehicle[0],
                Message = ErrorEnumApplication.SERVER_ERROR
            };


            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(customerId)).ReturnsAsync(customerReturn);

            _vehicleRepositoryMock.Setup(r => r.GetAllCustomerVehicleByCustomerIdAsync(customerId)).ReturnsAsync(vehicleReturn);

            _vehicleRepositoryMock.Setup(r => r.UpdateVehicleAsync(It.IsAny<Vehicle>())).ReturnsAsync(updateVehicleReturn);

            // Act
            var result = await _customerService.DeleteCustomerByStaff(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        //UpdateInformationCustomerByStaff
        // Successfull
        [Fact]
        public async Task UpdateInformationCustomerByStaff_ShouldReturnSuccess()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var customerId = Guid.NewGuid();

            var existingCustomer = new Customer
            {
                FullName = "NewCustomer",
                Email = "newcustomer@localhost.com",
                StatusCustomer = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = new Customer
                {
                    Id = customerId,
                    Email = "oldcustomer@localhost.com",
                    FullName = "OldCustomer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerTypeReturn = new Return<CustomerType>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var existingCustomerReturn = new Return<Customer>
            {
                IsSuccess = false,
                Data = existingCustomer,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var req = new UpdateInformationCustomerResDto
            {
                CustomerId = customerId,
                FullName = "NewCustomer",
                Email = "newcustomer@localhost.com"
            };

            var updateReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = new Customer
                {
                    Id = customerId,
                    Email = req.Email,
                    FullName = req.FullName,
                    StatusCustomer = StatusUserEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(customerId)).ReturnsAsync(customerReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerTypeByIdAsync(It.IsAny<Guid>())).ReturnsAsync(customerTypeReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerByEmailAsync(req.Email)).ReturnsAsync(existingCustomerReturn);

            _customerRepositoryMock.Setup(r => r.UpdateCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _customerService.UpdateInformationCustomerByStaff(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task UpdateInformationCustomerByStaff_ShouldReturnFailure_WhenUserIsNotAuthorized()
        {
            // Arrange
            var userEmail = "user@localhost.com";
            var userName = "user";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = Guid.NewGuid(),
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Data = user,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION
            };

            var request = new UpdateInformationCustomerResDto
            {
                CustomerId = Guid.NewGuid()
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            // Act
            var result = await _customerService.UpdateInformationCustomerByStaff(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task UpdateInformationCustomerByStaff_ShouldReturnFailure_WhenCustomerDoesNotExist()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var customerId = Guid.NewGuid();

            var existingCustomer = new Customer
            {
                FullName = "NewCustomer",
                Email = "newcustomer@localhost.com",
                StatusCustomer = StatusUserEnum.ACTIVE
            };

            var request = new UpdateInformationCustomerResDto
            {
                CustomerId = Guid.NewGuid()
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.CUSTOMER_NOT_EXIST,
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(request.CustomerId)).ReturnsAsync(customerReturn);

            // Act
            var result = await _customerService.UpdateInformationCustomerByStaff(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CUSTOMER_NOT_EXIST, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task UpdateInformationCustomerByStaff_ShouldReturnFailure_WhenEmailAlreadyExists()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var customerId = Guid.NewGuid();
            var fullname = "Customer";
            var email = "customer@localhost.com";

            var customer = new Customer
            {
                Id = customerId,
                Email = fullname,
                FullName = email,
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var req = new UpdateInformationCustomerResDto
            {
                CustomerId = customerId,
                FullName = fullname,
                Email = email
            };

            var customerTypeReturn = new Return<CustomerType>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var existingCustomerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = customer,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(customerId)).ReturnsAsync(customerReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerByEmailAsync(req.Email)).ReturnsAsync(existingCustomerReturn);

            // Act
            var result = await _customerService.UpdateInformationCustomerByStaff(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.EMAIL_IS_EXIST, result.Message);
        }

        // ReturnFailure
        [Fact]
        public async Task UpdateInformationCustomerByStaff_ShouldReturnFailure_WhenUpdateCustomerFails()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var customerId = Guid.NewGuid();
            var request = new UpdateInformationCustomerResDto
            {
                CustomerId = customerId,
                FullName = "Updated Name"
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = new Customer
                {
                    Email = "customer@localhost.com",
                    FullName = "Customer",
                    StatusCustomer = StatusUserEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var updateReturn = new Return<Customer>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(customerId)).ReturnsAsync(customerReturn);

            _customerRepositoryMock.Setup(r => r.UpdateCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _customerService.UpdateInformationCustomerByStaff(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetCustomerTypeOptionAsync
        // Successfull
        [Fact]
        public async Task GetCustomerTypeOptionAsync_ShouldReturnSuccess()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var customerTypes = new List<CustomerType>
            {
                new() { Id = Guid.NewGuid(), 
                    Name = CustomerTypeEnum.PAID, 
                    Description = "Paid customer" 
                },
                new() { 
                    Id = Guid.NewGuid(), 
                    Name = CustomerTypeEnum.FREE,
                    Description = "Free customer" 
                }
            };

            var repositoryResult = new Return<IEnumerable<CustomerType>>
            {
                IsSuccess = true,
                Data = customerTypes,
                TotalRecord = customerTypes.Count,
                Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
            };

            _customerRepositoryMock.Setup(x => x.GetAllCustomerTypeAsync()).ReturnsAsync(repositoryResult);

            // Act
            var result = await _customerService.GetCustomerTypeOptionAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
            Assert.Equal(customerTypes.Count, result.TotalRecord);
        }

        // ReturnFailure
        [Fact]
        public async Task GetCustomerTypeOptionAsync_ShouldReturnFailure_WhenUserIsNotAuthorized()
        {
            // Arrange
            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            // Act
            var result = await _customerService.GetCustomerTypeOptionAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
            Assert.Null(result.Data);
        }

        // Failure
        [Fact]
        public async Task GetCustomerTypeOptionAsync_ShouldReturnFailure_WhenGetFail()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.SUPERVISOR.ToString() };
            var userEmail = "supervisor@localhost.com";
            var userName = "supervisor";
            var user = new User
            {
                Email = userEmail,
                FullName = userName,
                RoleId = role.Id,
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(s => s.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);


            var repositoryResult = new Return<IEnumerable<CustomerType>>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _customerRepositoryMock.Setup(x => x.GetAllCustomerTypeAsync()).ReturnsAsync(repositoryResult);

            // Act
            var result = await _customerService.GetCustomerTypeOptionAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }
    }
}
