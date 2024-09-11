using FUParkingRepository.Interface;
using FUParkingService.Interface;
using FUParkingService;
using Moq;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using Xunit;

namespace FUParkingTesting
{
    public class CustomerServiceTesting
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IHelpperService> _helpperServiceMock;
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
        private readonly CustomerService _customerService;

        public CustomerServiceTesting()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _helpperServiceMock = new Mock<IHelpperService>();
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();
            _customerService = new CustomerService(_customerRepositoryMock.Object, _helpperServiceMock.Object, _vehicleRepositoryMock.Object);
        }

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

        [Fact]
        public async Task ChangeStatusCustomerAsync_ShouldReturnError_WhenUserIsNotSupervisor()
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


    }
}
