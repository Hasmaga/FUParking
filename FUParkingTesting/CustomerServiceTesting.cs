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
    }
}
