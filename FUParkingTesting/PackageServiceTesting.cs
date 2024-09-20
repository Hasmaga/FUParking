using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService;
using FUParkingService.Interface;
using Moq;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FUParkingTesting
{
    public class PackageServiceTesting
    {
        private readonly Mock<IPackageRepository> _packageRepositoryMock = new();
        private readonly Mock<IHelpperService> _helpperServiceMock = new();
        private readonly PackageService _packageService;

        public PackageServiceTesting()
        {
            _packageRepositoryMock = new Mock<IPackageRepository>();
            _helpperServiceMock = new Mock<IHelpperService>();
            _packageService = new PackageService(_packageRepositoryMock.Object, _helpperServiceMock.Object);
        }

        // GetPackageForUserAsync
        // Successful
        [Fact]
        public async Task GetPackageForUserAsync_ShouldReturnSuccess_WhenPackageFound()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var user = new User
            {
                Email = "supervisor@localhost.com",
                FullName = "Supervisor",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE,
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            var packageList = new List<Package>
            {
                new() {
                    Name = "Package 1",
                    CoinAmount = 100,
                    Price = 10,
                    PackageStatus = StatusPackageEnum.ACTIVE,
                },
            };

            var packageReturn = new Return<IEnumerable<Package>>
            {
                IsSuccess = true,
                Data = packageList,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                TotalRecord = 1,
            };

            _packageRepositoryMock.Setup(x => x.GetAllPackagesAsync(req))
                .ReturnsAsync(packageReturn);

            // Act
            var result = await _packageService.GetPackageForUserAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetPackageForUserAsync_ReturnsUnauthorized_UnauthorizedUser()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            // Act
            var result = await _packageService.GetPackageForUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetPackageForUserAsync_RepositoryError_ReturnsServerError()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var user = new User
            {
                Email = "supervisor@localhost.com",
                FullName = "Supervisor",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE,
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            var packageList = new List<Package>
            {
                new() {
                    Name = "Package 1",
                    CoinAmount = 100,
                    Price = 10,
                    PackageStatus = StatusPackageEnum.ACTIVE,
                },
            };

            var packageReturn = new Return<IEnumerable<Package>>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR,
            };

            _packageRepositoryMock.Setup(x => x.GetAllPackagesAsync(req))
                .ReturnsAsync(packageReturn);

            // Act
            var result = await _packageService.GetPackageForUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetPackageForUserAsync_ShouldReturnSuccess_WhenPackageNotFound()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var user = new User
            {
                Email = "supervisor@localhost.com",
                FullName = "Supervisor",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE,
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            var packageList = new List<Package>();

            var packageReturn = new Return<IEnumerable<Package>>
            {
                IsSuccess = true,
                Data = packageList,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                TotalRecord = 0,
            };

            _packageRepositoryMock.Setup(x => x.GetAllPackagesAsync(req))
                .ReturnsAsync(packageReturn);

            // Act
            var result = await _packageService.GetPackageForUserAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // GetPackagesByCustomerAsync
        // Successful
        [Fact]
        public async Task GetPackagesByCustomerAsync_ShouldReturnSuccess_WhenPackageFound()
        {
            // Arrange
            var req = new GetListObjectWithPageReqDto();

            var cus = new Customer
            {
                Email = "customer@localhost.com",
                FullName = "Supervisor",
                StatusCustomer = StatusCustomerEnum.ACTIVE,
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = cus,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(customerReturn);

            var packageList = new List<Package>
            {
                new() {
                    Name = "Package 1",
                    CoinAmount = 100,
                    Price = 10,
                    PackageStatus = StatusPackageEnum.ACTIVE,
                },
            };

            var packageReturn = new Return<IEnumerable<Package>>
            {
                IsSuccess = true,
                Data = packageList,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                TotalRecord = 1,
            };

            _packageRepositoryMock.Setup(x => x.GetPackageForCustomerAsync(req))
                .ReturnsAsync(packageReturn);

            // Act
            var result = await _packageService.GetPackagesByCustomerAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetPackagesByCustomerAsync_ShouldReturnsUnauthorized_WhenServerError()
        {
            // Arrange
            var req = new GetListObjectWithPageReqDto();

            var cus = new Customer
            {
                Email = "customer@localhost.com",
                FullName = "Supervisor",
                StatusCustomer = StatusCustomerEnum.ACTIVE,
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = cus,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(customerReturn);

            var packageReturn = new Return<IEnumerable<Package>>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.SERVER_ERROR,
                TotalRecord = 0,
            };

            _packageRepositoryMock.Setup(x => x.GetPackageForCustomerAsync(req))
                .ReturnsAsync(packageReturn);

            // Act
            var result = await _packageService.GetPackagesByCustomerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetPackagesByCustomerAsync_ShouldReturnSuccess_WhenPackageNotFound()
        {
            // Arrange
            var req = new GetListObjectWithPageReqDto();

            var cus = new Customer
            {
                Email = "customer@localhost.com",
                FullName = "Supervisor",
                StatusCustomer = StatusCustomerEnum.ACTIVE,
            };

            var customerReturn = new Return<Customer>
            {
                IsSuccess = true,
                Data = cus,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(customerReturn);

            var packageReturn = new Return<IEnumerable<Package>>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                TotalRecord = 0,
            };

            _packageRepositoryMock.Setup(x => x.GetPackageForCustomerAsync(req))
                .ReturnsAsync(packageReturn);

            // Act
            var result = await _packageService.GetPackagesByCustomerAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // CreateCoinPackage
        // Successful
        [Fact]
        public async Task CreateCoinPackage_ShouldReturnSuccess_WhenPackageDoesNotExist()
        {
            // Arrange
            var reqDto = new CreateCoinPackageReqDto
            {
                Name = "Package 1",
                CoinAmount = 100,
                Price = 10,
                ExtraCoin = 10,
                EXPPackage = 30
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "manager@localhost.com",
                FullName = "Manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var packageReturn = new Return<Package>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                Data = null,
                IsSuccess = false,
            };

            var createdPackage = new Package
            {
                Name = reqDto.Name,
                CoinAmount = reqDto.CoinAmount,
                Price = reqDto.Price,
                ExtraCoin = reqDto.ExtraCoin,
                EXPPackage = reqDto.EXPPackage,
                PackageStatus = StatusPackageEnum.ACTIVE,
                CreatedById = user.Id,
            };

            var createReturn = new Return<Package>
            {
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                Data = createdPackage,
                IsSuccess = true,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByNameAsync(reqDto.Name)).ReturnsAsync(packageReturn);
            _packageRepositoryMock.Setup(x => x.CreatePackageAsync(It.IsAny<Package>())).ReturnsAsync(createReturn);

            // Act
            var result = await _packageService.CreateCoinPackage(reqDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successful
        [Fact]
        public async Task CreateCoinPackage_ShouldReturnSuccess_WhenPackageExists()
        {
            // Arrange
            var reqDto = new CreateCoinPackageReqDto
            {
                Name = "Package 1",
                CoinAmount = 100,
                Price = 10,
                ExtraCoin = 10,
                EXPPackage = 30
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "manager@localhost.com",
                FullName = "Manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var existingPackage = new Package
            {
                Name = reqDto.Name,
                CoinAmount = reqDto.CoinAmount,
                Price = reqDto.Price,
                PackageStatus = StatusPackageEnum.ACTIVE
            };

            var packageReturn = new Return<Package>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = existingPackage,
                IsSuccess = true,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByNameAsync(reqDto.Name)).ReturnsAsync(packageReturn);

            // Act
            var result = await _packageService.CreateCoinPackage(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.OBJECT_EXISTED, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateCoinPackage_ShouldReturnFailure_WhenExceptionThrown()
        {
            // Arrange
            var reqDto = new CreateCoinPackageReqDto
            {
                Name = "Package 1",
                CoinAmount = 100,
                Price = 10,
                ExtraCoin = 10,
                EXPPackage = 30
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    FullName = "Customer",
                    PasswordHash = "",
                    PasswordSalt = "",
                    Email = "customer@gmail.com",
                    StatusUser = StatusCustomerEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var createReturn = new Return<Package>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _packageRepositoryMock.Setup(x => x.CreatePackageAsync(It.IsAny<Package>())).ReturnsAsync(createReturn);

            // Act
            var result = await _packageService.CreateCoinPackage(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
            Assert.NotNull(result.InternalErrorMessage);
        }

        // Failure
        [Fact]
        public async Task CreateCoinPackage_ShouldReturnFailure_WhenInvalidUser()
        {
            // Arrange
            var userReturn = new Return<User>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                IsSuccess = false
            };

            var reqDto = new CreateCoinPackageReqDto
            {
                Name = "Package 1",
                CoinAmount = 100,
                Price = 10,
                ExtraCoin = 10,
                EXPPackage = 30
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);

            // Act
            var result = await _packageService.CreateCoinPackage(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // UpdateCoinPackage
        // Successful
        [Fact]
        public async Task UpdateCoinPackage_ShouldReturnSuccess_WhenInActiveToActive()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "manager@localhost.com",
                FullName = "Manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var package = new Package
            {
                Id = packageId,
                Name = "Package 1",
                PackageStatus = StatusPackageEnum.INACTIVE,
            };

            var packageReturn = new Return<Package>
            {
                IsSuccess = true,
                Data = package,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var existPackageName = new Return<Package>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
            };

            var req = new UpdateCoinPackageReqDto
            {
                PackageId = packageId,
                Name = "UpdateName",
                IsActive = true,
            };

            var updateReturn = new Return<Package>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                IsSuccess = true,
                Data = new Package
                {
                    Name = req.Name,
                    PackageStatus = StatusPackageEnum.ACTIVE
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByPackageIdAsync(packageId)).ReturnsAsync(packageReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByNameAsync(req.Name)).ReturnsAsync(existPackageName);
            _packageRepositoryMock.Setup(x => x.UpdateCoinPackage(It.IsAny<Package>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _packageService.UpdateCoinPackage(req);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successful
        [Fact]
        public async Task UpdateCoinPackage_ShouldReturnSuccess_WhenActiveToInActive()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "manager@localhost.com",
                FullName = "Manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var package = new Package
            {
                Id = packageId,
                Name = "Package 1",
                PackageStatus = StatusPackageEnum.ACTIVE,
            };

            var packageReturn = new Return<Package>
            {
                IsSuccess = true,
                Data = package,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var existPackageName = new Return<Package>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
            };

            var req = new UpdateCoinPackageReqDto
            {
                PackageId = packageId,
                Name = "UpdateName",
                IsActive = false,
            };

            var updateReturn = new Return<Package>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                IsSuccess = true,
                Data = new Package
                {
                    Name = req.Name,
                    PackageStatus = StatusPackageEnum.INACTIVE
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByPackageIdAsync(packageId)).ReturnsAsync(packageReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByNameAsync(req.Name)).ReturnsAsync(existPackageName);
            _packageRepositoryMock.Setup(x => x.UpdateCoinPackage(It.IsAny<Package>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _packageService.UpdateCoinPackage(req);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateCoinPackage_ShouldReturnFailure_WhenInvalidUser()
        {
            // Arrange
            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
            };

            var packageId = Guid.NewGuid();

            var req = new UpdateCoinPackageReqDto
            {
                PackageId = packageId,
                Name = "UpdateName"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);

            // Act
            var result = await _packageService.UpdateCoinPackage(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Successful
        [Fact]
        public async Task UpdateCoinPackage_ShouldReturnSuccess_WhenNoChangeStatus()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "manager@localhost.com",
                FullName = "Manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var package = new Package
            {
                Id = packageId,
                Name = "Package 1",
                PackageStatus = StatusPackageEnum.ACTIVE,
            };

            var packageReturn = new Return<Package>
            {
                IsSuccess = true,
                Data = package,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var existPackageName = new Return<Package>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
            };

            var req = new UpdateCoinPackageReqDto
            {
                PackageId = packageId,
                Name = "UpdateName",
            };

            var updateReturn = new Return<Package>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                IsSuccess = true,
                Data = new Package
                {
                    Name = req.Name,
                    PackageStatus = StatusPackageEnum.ACTIVE
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByPackageIdAsync(packageId)).ReturnsAsync(packageReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByNameAsync(req.Name)).ReturnsAsync(existPackageName);
            _packageRepositoryMock.Setup(x => x.UpdateCoinPackage(It.IsAny<Package>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _packageService.UpdateCoinPackage(req);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateCoinPackage_ShouldReturnFailure_WhenPackageNotFound()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "manager@localhost.com",
                FullName = "Manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var packageReturn = new Return<Package>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
            };

            var req = new UpdateCoinPackageReqDto
            {
                PackageId = packageId,
                Name = "UpdateName",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByPackageIdAsync(packageId)).ReturnsAsync(packageReturn);

            // Act
            var result = await _packageService.UpdateCoinPackage(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PACKAGE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateCoinPackage_ShouldReturnFailure_WhenPackageNameExist()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "manager@localhost.com",
                FullName = "Manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var package = new Package
            {
                Id = packageId,
                Name = "Package 1",
                PackageStatus = StatusPackageEnum.ACTIVE,
            };

            var existPackage = new Package
            {
                Id = Guid.NewGuid(),
                Name = "Package 2",
                PackageStatus = StatusPackageEnum.ACTIVE,
            };

            var packageReturn = new Return<Package>
            {
                IsSuccess = true,
                Data = package,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var existPackageName = new Return<Package>
            {
                IsSuccess = true,
                Data = package,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var req = new UpdateCoinPackageReqDto
            {
                PackageId = packageId,
                Name = existPackage.Name,
            };

            var updateReturn = new Return<Package>
            {
                Message = ErrorEnumApplication.OBJECT_EXISTED,
                IsSuccess = false,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByPackageIdAsync(packageId)).ReturnsAsync(packageReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByNameAsync(req.Name)).ReturnsAsync(existPackageName);
            _packageRepositoryMock.Setup(x => x.UpdateCoinPackage(It.IsAny<Package>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _packageService.UpdateCoinPackage(req);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.OBJECT_EXISTED, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateCoinPackage_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "manager@localhost.com",
                FullName = "Manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var package = new Package
            {
                Id = packageId,
                Name = "Package 1",
                PackageStatus = StatusPackageEnum.ACTIVE,
            };

            var packageReturn = new Return<Package>
            {
                IsSuccess = true,
                Data = package,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var existPackageName = new Return<Package>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
            };

            var req = new UpdateCoinPackageReqDto
            {
                PackageId = packageId,
                Name = "UpdateName",
            };

            var updateReturn = new Return<Package>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByPackageIdAsync(packageId)).ReturnsAsync(packageReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByNameAsync(req.Name)).ReturnsAsync(existPackageName);
            _packageRepositoryMock.Setup(x => x.UpdateCoinPackage(It.IsAny<Package>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _packageService.UpdateCoinPackage(req);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // DeleteCoinPackage
        // Successful
        [Fact]
        public async Task DeleteCoinPackage_ShouldReturnSuccess()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "manager@localhost.com",
                FullName = "Manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var package = new Package
            {
                Id = packageId,
                Name = "Package 1",
                PackageStatus = StatusPackageEnum.ACTIVE,
            };

            var packageReturn = new Return<Package>
            {
                IsSuccess = true,
                Data = package,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var updateReturn = new Return<Package>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                IsSuccess = true,
                Data = new Package
                {
                    Name = package.Name,
                    PackageStatus = StatusPackageEnum.ACTIVE,
                    DeletedDate = DateTime.Now,
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByPackageIdAsync(packageId)).ReturnsAsync(packageReturn);
            _packageRepositoryMock.Setup(x => x.UpdateCoinPackage(It.IsAny<Package>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _packageService.DeleteCoinPackage(packageId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteCoinPackage_ShouldReturnsFailure_WhenUnauthorizedUser()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                    IsSuccess = false
                });

            // Act
            var result = await _packageService.DeleteCoinPackage(packageId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteCoinPackage_ShouldReturnsFailure_WhenPackageNotFound()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "manager@localhost.com",
                FullName = "Manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var packageReturn = new Return<Package>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var updateReturn = new Return<Package>
            {
                Message = ErrorEnumApplication.PACKAGE_NOT_EXIST,
                IsSuccess = false,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByPackageIdAsync(packageId)).ReturnsAsync(packageReturn);
            _packageRepositoryMock.Setup(x => x.UpdateCoinPackage(It.IsAny<Package>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _packageService.DeleteCoinPackage(packageId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PACKAGE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteCoinPackage_ShouldReturnsFailure_WhenUpdateFails()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "manager@localhost.com",
                FullName = "Manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var package = new Package
            {
                Id = packageId,
                Name = "Package 1",
                PackageStatus = StatusPackageEnum.ACTIVE,
            };

            var packageReturn = new Return<Package>
            {
                IsSuccess = true,
                Data = package,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            var updateReturn = new Return<Package>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _packageRepositoryMock.Setup(x => x.GetPackageByPackageIdAsync(packageId)).ReturnsAsync(packageReturn);
            _packageRepositoryMock.Setup(x => x.UpdateCoinPackage(It.IsAny<Package>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _packageService.DeleteCoinPackage(packageId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

    }
}
