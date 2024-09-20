using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
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
    public class ParkingAreaServiceTesting
    {
        private readonly Mock<IParkingAreaRepository> _parkingAreaRepositoryMock = new();
        private readonly Mock<IHelpperService> _helpperServiceMock = new();
        private readonly Mock<ISessionRepository> _sessionRepositoryMock = new();
        private readonly Mock<IGateRepository> _gateRepositoryMock = new();
        private readonly ParkingAreaService _parkingService;

        public ParkingAreaServiceTesting()
        {
            _parkingAreaRepositoryMock = new Mock<IParkingAreaRepository>();
            _sessionRepositoryMock = new Mock<ISessionRepository>();
            _helpperServiceMock = new Mock<IHelpperService>();
            _gateRepositoryMock = new Mock<IGateRepository>();
            _parkingService = new ParkingAreaService(_parkingAreaRepositoryMock.Object, _helpperServiceMock.Object, _sessionRepositoryMock.Object, _gateRepositoryMock.Object);
        }

        // DeleteParkingArea
        // Successful
        [Fact]
        public async Task DeleteParkingArea_ShouldReturnsSuccess()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();

            var parking = new ParkingArea
            {
                Id = parkingAreaId,
                Name = "FPTU1",
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
            };

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingReturn = new Return<ParkingArea>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = parking,
            };

            var sessionReturn = new Return<IEnumerable<Session>>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = false,
                Data = null,
            };

            var updateReturn = new Return<ParkingArea>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                IsSuccess = true,
                Data = new ParkingArea
                {
                    Id = parking.Id,
                    Name = parking.Name,
                    Block = parking.Block,
                    Mode = parking.Mode,
                    StatusParkingArea = parking.StatusParkingArea,
                    DeletedDate = DateTime.Now,
                },
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(parkingReturn);

            _sessionRepositoryMock.Setup(x => x.GetListSessionActiveByParkingIdAsync(parkingAreaId))
                .ReturnsAsync(sessionReturn);

            _parkingAreaRepositoryMock.Setup(x => x.UpdateParkingAreaAsync(It.IsAny<ParkingArea>()))
                .ReturnsAsync(updateReturn);

            // Act
            var result = await _parkingService.DeleteParkingArea(parkingAreaId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteParkingArea_ShouldReturnsUnauthorized_WhenUnauthorizedUser()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                IsSuccess = false
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            // Act
            var result = await _parkingService.DeleteParkingArea(parkingAreaId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteParkingArea_ShouldReturnsfailure_WhenNonExistentParkingArea()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingReturn = new Return<ParkingArea>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = false,
                Data = null,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(parkingReturn);

            // Act
            var result = await _parkingService.DeleteParkingArea(parkingAreaId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PARKING_AREA_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteParkingArea_ShouldReturnsFailure_WhenParkingAreaInUse()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();

            var parking = new ParkingArea
            {
                Id = parkingAreaId,
                Name = "FPTU1",
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
            };

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingReturn = new Return<ParkingArea>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = parking,
            };

            var sessionExistList = new List<Session>
            {
                new() {
                    Status = SessionEnum.PARKED,
                    Block = 30,
                    Mode = "MODE1",
                    TimeIn = DateTime.Now,
                    PlateNumber = "99L999999",
                    ImageInUrl = "localhost/"
                },
            };

            var sessionReturn = new Return<IEnumerable<Session>>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = sessionExistList
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(parkingReturn);

            _sessionRepositoryMock.Setup(x => x.GetListSessionActiveByParkingIdAsync(parkingAreaId))
                .ReturnsAsync(sessionReturn);

            // Act
            var result = await _parkingService.DeleteParkingArea(parkingAreaId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PARKING_AREA_IS_USING, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteParkingArea_ShouldReturnsFailure_WhenVirtualParkingArea()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();

            var parking = new ParkingArea
            {
                Id = parkingAreaId,
                Name = GateTypeEnum.VIRUTAL,
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
            };

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingReturn = new Return<ParkingArea>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = parking,
            };

            var sessionReturn = new Return<IEnumerable<Session>>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = false,
                Data = null
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(parkingReturn);

            _sessionRepositoryMock.Setup(x => x.GetListSessionActiveByParkingIdAsync(parkingAreaId))
                .ReturnsAsync(sessionReturn);

            // Act
            var result = await _parkingService.DeleteParkingArea(parkingAreaId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CANNOT_DELETE_VIRTUAL_PARKING_AREA, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteParkingArea_ShouldReturnsFailure_WhenUpdateFails()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();

            var parking = new ParkingArea
            {
                Id = parkingAreaId,
                Name = "FPTU1",
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
            };

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingReturn = new Return<ParkingArea>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = parking,
            };

            var sessionReturn = new Return<IEnumerable<Session>>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = false,
                Data = null,
            };

            var updateReturn = new Return<ParkingArea>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(parkingReturn);

            _sessionRepositoryMock.Setup(x => x.GetListSessionActiveByParkingIdAsync(parkingAreaId))
                .ReturnsAsync(sessionReturn);

            _parkingAreaRepositoryMock.Setup(x => x.UpdateParkingAreaAsync(It.IsAny<ParkingArea>()))
                .ReturnsAsync(updateReturn);

            // Act
            var result = await _parkingService.DeleteParkingArea(parkingAreaId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CreateParkingAreaAsync
        // Successful
        [Fact]
        public async Task CreateParkingAreaAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var req = new CreateParkingAreaReqDto
            {
                Name = "FPT1",
                Description = "FPT 1",
                Mode = 1,
                Block = 30,
            };

            var parkingReturn = new Return<ParkingArea>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                Data = null,
                IsSuccess = false
            };

            var createReturn = new Return<ParkingArea>
            {
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                Data = new ParkingArea
                {
                    Name = req.Name,
                    Block = req.Block,
                    Mode = req.Mode.ToString(),
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                },
                IsSuccess = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByNameAsync(req.Name))
                .ReturnsAsync(parkingReturn);

            _parkingAreaRepositoryMock.Setup(x => x.CreateParkingAreaAsync(It.IsAny<ParkingArea>()))
                .ReturnsAsync(createReturn);

            // Act
            var result = await _parkingService.CreateParkingAreaAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateParkingAreaAsync_ShouldReturnsUnauthorized_WhenUnauthorizedUser()
        {
            // Arrange
            var req = new CreateParkingAreaReqDto
            {
                Name = "Test Parking",
                Description = "Test Description",
                Mode = 1,
                Block = 30,
            };

            var userReturn = new Return<User>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                IsSuccess = false
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            // Act
            var result = await _parkingService.CreateParkingAreaAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateParkingAreaAsync_ShouldReturnsFailure_WhenParkingAreaNameExisted()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var req = new CreateParkingAreaReqDto
            {
                Name = "FPT1",
                Description = "FPT 1",
                Mode = 1,
                Block = 30,
            };

            var existingParking = new ParkingArea
            {
                Name = req.Name,
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
            };

            var parkingReturn = new Return<ParkingArea>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = existingParking,
                IsSuccess = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByNameAsync(req.Name))
                .ReturnsAsync(parkingReturn);

            // Act
            var result = await _parkingService.CreateParkingAreaAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.OBJECT_EXISTED, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateParkingAreaAsync_ShouldReturnsFailure_WhenInvalidModeInput()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var req = new CreateParkingAreaReqDto
            {
                Name = "FPTU1",
                Description = "FPT 1",
                Mode = 6,
                Block = 30,
            };

            var parkingReturn = new Return<ParkingArea>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                Data = null,
                IsSuccess = false
            };

            var createReturn = new Return<ParkingArea>
            {
                Message = ErrorEnumApplication.INVALID_INPUT,
                Data = new ParkingArea
                {
                    Name = req.Name,
                    Block = req.Block,
                    Mode = req.Mode.ToString(),
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                },
                IsSuccess = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByNameAsync(req.Name))
                .ReturnsAsync(parkingReturn);

            // Act
            var result = await _parkingService.CreateParkingAreaAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.INVALID_INPUT, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateParkingAreaAsync_ShouleReturnsFailure_WhenServerError()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var req = new CreateParkingAreaReqDto
            {
                Name = "FPT1",
                Description = "FPT 1",
                Mode = 1,
                Block = 30,
            };

            var parkingReturn = new Return<ParkingArea>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                Data = null,
                IsSuccess = false
            };

            var createReturn = new Return<ParkingArea>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByNameAsync(req.Name))
                .ReturnsAsync(parkingReturn);

            _parkingAreaRepositoryMock.Setup(x => x.CreateParkingAreaAsync(It.IsAny<ParkingArea>()))
                .ReturnsAsync(createReturn);

            // Act
            var result = await _parkingService.CreateParkingAreaAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetParkingAreasAsync
        // Successful
        [Fact]
        public async Task GetParkingAreasAsync_ShouldReturnsSuccess_WhenNotFound()
        {
            // Arrange
            var user = new User
            {
                Email = "supervisor@localhost.com",
                FullName = "supervisor",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var req = new GetListObjectWithFiller();

            var parkingAreas = new List<ParkingArea>
            {
                new() {
                    Name = "Parking Area 1",
                    Block = 30,
                    Mode = ModeEnum.MODE1,
                    StatusParkingArea = StatusParkingEnum.ACTIVE
                },
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetAllParkingAreasAsync(req))
                .ReturnsAsync(new Return<IEnumerable<ParkingArea>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = parkingAreas,
                    TotalRecord = parkingAreas.Count,
                    IsSuccess = true
                });

            // Act
            var result = await _parkingService.GetParkingAreasAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetParkingAreasAsync_ShouldReturnsSuccess_WhenFound()
        {
            // Arrange
            var user = new User
            {
                Email = "supervisor@localhost.com",
                FullName = "supervisor",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var req = new GetListObjectWithFiller();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetAllParkingAreasAsync(req))
                .ReturnsAsync(new Return<IEnumerable<ParkingArea>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = null,
                    TotalRecord = 0,
                    IsSuccess = true
                });

            // Act
            var result = await _parkingService.GetParkingAreasAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetParkingAreasAsync_ShouldReturnsUnauthorized_WhenUnauthorizedUser()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var userReturn = new Return<User>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                IsSuccess = false
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            // Act
            var result = await _parkingService.GetParkingAreasAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetParkingAreasAsync_ShouldReturnsFailure_WhenServerError()
        {
            // Arrange
            var user = new User
            {
                Email = "supervisor@localhost.com",
                FullName = "supervisor",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var req = new GetListObjectWithFiller();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetAllParkingAreasAsync(req))
                .ReturnsAsync(new Return<IEnumerable<ParkingArea>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false
                });

            // Act
            var result = await _parkingService.GetParkingAreasAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
            Assert.Null(result.Data);
        }

        // UpdateParkingAreaAsync
        // Successful
        [Fact]
        public async Task UpdateParkingAreaAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingAreaId = Guid.NewGuid();
            var req = new UpdateParkingAreaReqDto
            {
                ParkingAreaId = parkingAreaId,
                Name = "Update Name",
                Block = 30,
                Mode = 2
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new ParkingArea 
                    { 
                        Id = parkingAreaId, 
                        Name = "Old Name",
                        Block = 30,
                        Mode = "MODE1",
                        MaxCapacity = 20,
                        StatusParkingArea = StatusParkingEnum.ACTIVE
                    }
                });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByNameAsync(req.Name))
                .ReturnsAsync(new Return<ParkingArea> 
                { 
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null,
                    IsSuccess = false
                });

            _parkingAreaRepositoryMock.Setup(x => x.UpdateParkingAreaAsync(It.IsAny<ParkingArea>()))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                    IsSuccess = true,
                    Data = new ParkingArea
                    {
                        Id = parkingAreaId,
                        Name = req.Name,
                        Block = (int)req.Block,
                        Mode = ModeEnum.MODE2,
                        MaxCapacity = 20,
                        StatusParkingArea = StatusParkingEnum.ACTIVE
                    }
                });

            // Act
            var result = await _parkingService.UpdateParkingAreaAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateParkingAreaAsync_ShouldReturnsUnauthorized_WhenUnauthorizedUser()
        {
            // Arrange
            var req = new UpdateParkingAreaReqDto();

            var userReturn = new Return<User>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                IsSuccess = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            // Act
            var result = await _parkingService.UpdateParkingAreaAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateParkingAreaAsync_ShouldReturnsFailure_WhenParkingAreaNotFound()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingAreaId = Guid.NewGuid();
            var req = new UpdateParkingAreaReqDto { ParkingAreaId = parkingAreaId };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
               .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea> 
                { 
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null,
                    IsSuccess = false
                });

            // Act
            var result = await _parkingService.UpdateParkingAreaAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PARKING_AREA_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateParkingAreaAsync_ShouldReturnsFailure_WhenNameExisted()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingAreaId = Guid.NewGuid();
            var req = new UpdateParkingAreaReqDto
            {
                ParkingAreaId = parkingAreaId,
                Name = "Update Name",
                Block = 30,
                Mode = 2
            };

            var existingParking = new ParkingArea
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Block = 30,
                Mode = "MODE1",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new ParkingArea
                    {
                        Id = parkingAreaId,
                        Name = "FPTU",
                        Block = 30,
                        Mode = "MODE1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE,
                    }
                });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByNameAsync(req.Name))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = existingParking,
                    IsSuccess = true
                });

            _parkingAreaRepositoryMock.Setup(x => x.UpdateParkingAreaAsync(It.IsAny<ParkingArea>()))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = ErrorEnumApplication.OBJECT_EXISTED,
                    IsSuccess = false,
                });


            // Act
            var result = await _parkingService.UpdateParkingAreaAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.OBJECT_EXISTED, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateParkingAreaAsync_ShouldReturnFailure_WhenInvalidMode()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingAreaId = Guid.NewGuid();
            var req = new UpdateParkingAreaReqDto
            {
                ParkingAreaId = parkingAreaId,
                Name = "Update Name",
                Block = 30,
                Mode = 5
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new ParkingArea
                    {
                        Id = parkingAreaId,
                        Name = "Old Name",
                        Block = 30,
                        Mode = "MODE1",
                        MaxCapacity = 20,
                        StatusParkingArea = StatusParkingEnum.ACTIVE
                    }
                });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByNameAsync(req.Name))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null,
                    IsSuccess = false
                });

            _parkingAreaRepositoryMock.Setup(x => x.UpdateParkingAreaAsync(It.IsAny<ParkingArea>()))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = ErrorEnumApplication.INVALID_INPUT,
                    IsSuccess = false,
                });

            // Act
            var result = await _parkingService.UpdateParkingAreaAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.INVALID_INPUT, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateParkingAreaAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingAreaId = Guid.NewGuid();
            var req = new UpdateParkingAreaReqDto
            {
                ParkingAreaId = parkingAreaId,
                Name = "Update Name",
                Block = 30,
                Mode = 2
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new ParkingArea
                    {
                        Id = parkingAreaId,
                        Name = "Old Name",
                        Block = 30,
                        Mode = "MODE1",
                        MaxCapacity = 20,
                        StatusParkingArea = StatusParkingEnum.ACTIVE
                    }
                });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByNameAsync(req.Name))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null,
                    IsSuccess = false
                });

            _parkingAreaRepositoryMock.Setup(x => x.UpdateParkingAreaAsync(It.IsAny<ParkingArea>()))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                });

            // Act
            var result = await _parkingService.UpdateParkingAreaAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdateStatusParkingAreaAsync
        // Successful
        [Fact]
        public async Task UpdateStatusParkingAreaAsync_ShouleReturn_WhenInActiveToActive()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingAreaId = Guid.NewGuid();
            var isActive = true;

            var sessionReturn = new Return<IEnumerable<Session>>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = false,
                Data = null
            };

            var updateReturn = new Return<ParkingArea>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                IsSuccess = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new ParkingArea 
                    { 
                        Id = parkingAreaId, 
                        StatusParkingArea = StatusParkingEnum.INACTIVE,
                        Mode = "MODE1",
                        Block = 30,
                        Name = "FPTU1"
                    }
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionActiveByParkingIdAsync(parkingAreaId))
                .ReturnsAsync(sessionReturn);

            _parkingAreaRepositoryMock.Setup(x => x.UpdateParkingAreaAsync(It.IsAny<ParkingArea>()))
                .ReturnsAsync(updateReturn);

            // Act
            var result = await _parkingService.UpdateStatusParkingAreaAsync(parkingAreaId, isActive);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successful
        [Fact]
        public async Task UpdateStatusParkingAreaAsync_ShouleReturn_WhenActiveToInActive()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingAreaId = Guid.NewGuid();
            var isActive = false;

            var sessionReturn = new Return<IEnumerable<Session>>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = false,
                Data = null
            };

            var updateReturn = new Return<ParkingArea>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                IsSuccess = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new ParkingArea
                    {
                        Id = parkingAreaId,
                        StatusParkingArea = StatusParkingEnum.ACTIVE,
                        Mode = "MODE1",
                        Block = 30,
                        Name = "FPTU1"
                    }
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionActiveByParkingIdAsync(parkingAreaId))
                .ReturnsAsync(sessionReturn);

            _parkingAreaRepositoryMock.Setup(x => x.UpdateParkingAreaAsync(It.IsAny<ParkingArea>()))
                .ReturnsAsync(updateReturn);

            // Act
            var result = await _parkingService.UpdateStatusParkingAreaAsync(parkingAreaId, isActive);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusParkingAreaAsync_ShouldReturnsUnauthorized_WhenUnauthorizedUser()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                IsSuccess = false
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            // Act
            var result = await _parkingService.UpdateStatusParkingAreaAsync(parkingAreaId, true);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusParkingAreaAsync_ReturnsShouldFailure_WhenParkingAreaNotFound()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingAreaId = Guid.NewGuid();
            var isActive = true;

            var sessionReturn = new Return<IEnumerable<Session>>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = false,
                Data = null
            };

            var updateReturn = new Return<ParkingArea>
            {
                Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST,
                IsSuccess = false
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null,
                    IsSuccess = false
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionActiveByParkingIdAsync(parkingAreaId))
                .ReturnsAsync(sessionReturn);

            _parkingAreaRepositoryMock.Setup(x => x.UpdateParkingAreaAsync(It.IsAny<ParkingArea>()))
                .ReturnsAsync(updateReturn);

            // Act
            var result = await _parkingService.UpdateStatusParkingAreaAsync(parkingAreaId, isActive);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PARKING_AREA_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusParkingAreaAsync_ShouldReturnsFailure_WhenParkingAreaInUse()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingAreaId = Guid.NewGuid();
            //var isActive = true;

            var existingSession = new List<Session>
            {
                new() {
                    Status = SessionEnum.PARKED,
                    Block = 30,
                    Mode = "MODE1",
                    PlateNumber = "99L999999",
                    TimeIn = DateTime.Now,
                    ImageInUrl = "localhost/",
                }
            };

            var sessionReturn = new Return<IEnumerable<Session>>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = existingSession,
            };

            var updateReturn = new Return<ParkingArea>
            {
                Message = ErrorEnumApplication.PARKING_AREA_IS_USING,
                IsSuccess = false
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = new ParkingArea
                    {
                        Id = parkingAreaId,
                        StatusParkingArea = StatusParkingEnum.INACTIVE,
                        Mode = "MODE1",
                        Block = 30,
                        Name = "FPTU1"
                    }
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionActiveByParkingIdAsync(parkingAreaId))
                .ReturnsAsync(sessionReturn);

            _parkingAreaRepositoryMock.Setup(x => x.UpdateParkingAreaAsync(It.IsAny<ParkingArea>()))
                .ReturnsAsync(updateReturn);

            // Act
            var result = await _parkingService.UpdateStatusParkingAreaAsync(parkingAreaId, false);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PARKING_AREA_IS_USING, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusParkingAreaAsync_ShouldReturnsFailure_WhenStatusAlreadyApplied()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();
            var isActive = true;

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User> 
                { 
                    IsSuccess = true, 
                    Data = new User 
                    { 
                        Email = "manager@localhost.com",
                        FullName = "manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new ParkingArea
                    {
                        Id = parkingAreaId,
                        StatusParkingArea = StatusParkingEnum.ACTIVE,
                        Mode = "MODE1",
                        Block = 30,
                        Name = "FPTU1"
                    }
                });

            _sessionRepositoryMock
                .Setup(x => x.GetListSessionActiveByParkingIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<IEnumerable<Session>>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = false,
                    Data = null
                });

            // Act
            var result = await _parkingService.UpdateStatusParkingAreaAsync(parkingAreaId, isActive);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.STATUS_IS_ALREADY_APPLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusParkingAreaAsync_ShouldReturnsFailure_WhenServerError()
        {
            // Arrange
            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = user,
                IsSuccess = true
            };

            var parkingAreaId = Guid.NewGuid();
            //var isActive = true;

            var sessionReturn = new Return<IEnumerable<Session>>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = false,
                Data = null
            };

            var updateReturn = new Return<ParkingArea>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(userReturn);

            _parkingAreaRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new ParkingArea
                    {
                        Id = parkingAreaId,
                        StatusParkingArea = StatusParkingEnum.INACTIVE,
                        Mode = "MODE1",
                        Block = 30,
                        Name = "FPTU1"
                    }
                });

            _sessionRepositoryMock.Setup(x => x.GetListSessionActiveByParkingIdAsync(parkingAreaId))
                .ReturnsAsync(sessionReturn);

            _parkingAreaRepositoryMock.Setup(x => x.UpdateParkingAreaAsync(It.IsAny<ParkingArea>()))
                .ReturnsAsync(updateReturn);

            // Act
            var result = await _parkingService.UpdateStatusParkingAreaAsync(parkingAreaId, true);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetParkingAreaOptionAsync
        // Successful
        [Fact]
        public async Task GetParkingAreaOptionAsync_ShouldReturnSuccess_WhenFoundObject()
        {
            // Arrange
            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User> 
                { 
                    IsSuccess = true, 
                    Data = new User
                    {
                        Email = "staff@localhost.com",
                        FullName = "staff",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            var parkingList = new List<ParkingArea>
            {
                new() { 
                    Name = "FPTU",
                    Block = 30,
                    Mode = "MODE1",
                    StatusParkingArea = StatusParkingEnum.ACTIVE
                },
            };

            _parkingAreaRepositoryMock
                .Setup(x => x.GetParkingAreaOptionAsync())
                .ReturnsAsync(new Return<IEnumerable<ParkingArea>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = parkingList,
                    TotalRecord = parkingList.Count,
                    IsSuccess = true,
                });

            // Act
            var result = await _parkingService.GetParkingAreaOptionAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
            Assert.Equal(1, result.TotalRecord);
        }

        // Failure
        [Fact]
        public async Task GetParkingAreaOptionAsync_ShouldReturnsFailure_WhenAuthenticationFailed()
        {
            // Arrange
            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _parkingService.GetParkingAreaOptionAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetParkingAreaOptionAsync_ShouldReturnsSuccess_When_WhenNotFoundObject()
        {
            // Arrange
            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "staff@localhost.com",
                        FullName = "staff",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            var parkingList = new List<ParkingArea>();

            _parkingAreaRepositoryMock
                .Setup(x => x.GetParkingAreaOptionAsync())
                .ReturnsAsync(new Return<IEnumerable<ParkingArea>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = parkingList,
                    TotalRecord = parkingList.Count,
                    IsSuccess = true,
                });

            // Act
            var result = await _parkingService.GetParkingAreaOptionAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetParkingAreaOptionAsync_ShouldReturnsFailure_WhenServerError()
        {
            // Arrange
            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        Email = "staff@localhost.com",
                        FullName = "staff",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _parkingAreaRepositoryMock
                .Setup(x => x.GetParkingAreaOptionAsync())
                .ReturnsAsync(new Return<IEnumerable<ParkingArea>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                });

            // Act
            var result = await _parkingService.GetParkingAreaOptionAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }
    }
}
