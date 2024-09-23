using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Gate;
using FUParkingModel.ResponseObject.Gate;
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
    public class GateServiceTesting
    {
        private readonly Mock<IHelpperService> _helpperServiceMock = new();
        private readonly Mock<IGateRepository> _gateRepositoryMock = new();
        private readonly Mock<IParkingAreaRepository> _parkingRepositoryMock = new();
        private readonly GateService _gateService;

        public GateServiceTesting()
        {
            _gateService = new GateService(_helpperServiceMock.Object, _gateRepositoryMock.Object, _parkingRepositoryMock.Object);
        }

        // GetAllGateAsync
        // Successful
        [Fact]
        public async Task GetAllGateAsync_ShouldReturnsSuccess_WhenGateFound()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var user = new User 
            { 
                Email = "staff@localhost.com",
                FullName = "Staff",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User> 
            { 
                IsSuccess = true, 
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var gates = new List<Gate>
            {
                new() {
                    Name = "Gate1",
                    Description = "Gate 1",
                    ParkingArea = new ParkingArea 
                    { 
                        Name = "FPTU1", 
                        Description = "Main Parking",
                        StatusParkingArea = StatusParkingEnum.ACTIVE,
                        Mode = "MODE1",
                        Block = 30,
                    },
                    StatusGate = StatusGateEnum.ACTIVE,
                }
            };

            var gateReturn = new Return<IEnumerable<Gate>>
            {
                IsSuccess = true,
                Data = gates,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                TotalRecord = gates.Count
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);
            _gateRepositoryMock.Setup(x => x.GetAllGateAsync(req)).ReturnsAsync(gateReturn);

            // Act
            var result = await _gateService.GetAllGateAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
            Assert.NotNull(result.Data);
        }

        // Successful
        [Fact]
        public async Task GetAllGateAsync_ShouldReturnsSuccess_WhenGateNotFound()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var user = new User
            {
                Email = "staff@localhost.com",
                FullName = "Staff",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var gates = new List<Gate>();

            var gateReturn = new Return<IEnumerable<Gate>>
            {
                IsSuccess = true,
                Data = gates,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                TotalRecord = gates.Count
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);
            _gateRepositoryMock.Setup(x => x.GetAllGateAsync(req)).ReturnsAsync(gateReturn);

            // Act
            var result = await _gateService.GetAllGateAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllGateAsync_ShoulReturnsFailure_WhenInvalidUser()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);

            // Act
            var result = await _gateService.GetAllGateAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
            Assert.Null(result.Data);
        }

        // Failure
        [Fact]
        public async Task GetAllGateAsync_ShouldReturnsSuccess_WhenGetFail()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var user = new User
            {
                Email = "staff@localhost.com",
                FullName = "Staff",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var gateReturn = new Return<IEnumerable<Gate>>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);
            _gateRepositoryMock.Setup(x => x.GetAllGateAsync(req)).ReturnsAsync(gateReturn);

            // Act
            var result = await _gateService.GetAllGateAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CreateGateAsync
        // Successful
        [Fact]
        public async Task CreateGateAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var req = new CreateGateReqDto
            {
                Name = "New Gate",
                ParkingAreaId = Guid.NewGuid()
            };

            var user = new User
            {
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

            var gateReturn = new Return<Gate> 
            { 
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT 
            };

            var parkingReturn = new Return<ParkingArea> 
            { 
                Message = SuccessfullyEnumServer.FOUND_OBJECT, 
                Data = new ParkingArea 
                { 
                    Id = req.ParkingAreaId,
                    Name = "FPTU1",
                    Description = "Main Parking",
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Mode = "MODE1",
                    Block = 30,
                } 
            };

            var createReturn = new Return<Gate> 
            {
                IsSuccess = true,
                Data = new Gate
                {
                    Name = req.Name,
                    StatusGate = StatusGateEnum.ACTIVE,
                    ParkingArea = parkingReturn.Data,
                },
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY 
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _gateRepositoryMock.Setup(x => x.GetGateByNameAsync(req.Name)).ReturnsAsync(gateReturn);
            _parkingRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(req.ParkingAreaId)).ReturnsAsync(parkingReturn);
            _gateRepositoryMock.Setup(x => x.CreateGateAsync(It.IsAny<Gate>())).ReturnsAsync(createReturn);

            // Act
            var result = await _gateService.CreateGateAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateGateAsync_ShouldReturnsFailure_WhenInvalidUser()
        {
            // Arrange
            var req = new CreateGateReqDto
            {
                Name = "New Gate",
                ParkingAreaId = Guid.NewGuid()
            };

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);

            // Act
            var result = await _gateService.CreateGateAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateGateAsync_ShouldReturnsFailure_WhenGateNameExists()
        {
            // Arrange
            var req = new CreateGateReqDto
            {
                Name = "New Gate",
                ParkingAreaId = Guid.NewGuid()
            };

            var user = new User
            {
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

            var existingGate = new Gate
            {
                Name = req.Name,
                Description = "Existing Gate",
                StatusGate = StatusGateEnum.ACTIVE,
            };

            var gateReturn = new Return<Gate>
            {
                IsSuccess = true,
                Data = existingGate,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _gateRepositoryMock.Setup(x => x.GetGateByNameAsync(req.Name)).ReturnsAsync(gateReturn);

            // Act
            var result = await _gateService.CreateGateAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.OBJECT_EXISTED, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateGateAsync_ShouldReturnsFailure_ParkingAreaNotExists()
        {
            // Arrange
            var req = new CreateGateReqDto
            {
                Name = "New Gate",
                ParkingAreaId = Guid.NewGuid()
            };

            var user = new User
            {
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

            var gateReturn = new Return<Gate>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var parkingReturn = new Return<ParkingArea>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = false,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _gateRepositoryMock.Setup(x => x.GetGateByNameAsync(req.Name)).ReturnsAsync(gateReturn);
            _parkingRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(req.ParkingAreaId)).ReturnsAsync(parkingReturn);

            // Act
            var result = await _gateService.CreateGateAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PARKING_AREA_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateGateAsync_ShouldReturnsFailure_WhenCreateFail()
        {
            // Arrange
            var req = new CreateGateReqDto
            {
                Name = "New Gate",
                ParkingAreaId = Guid.NewGuid()
            };

            var user = new User
            {
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

            var gateReturn = new Return<Gate>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var parkingReturn = new Return<ParkingArea>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new ParkingArea
                {
                    Id = req.ParkingAreaId,
                    Name = "FPTU1",
                    Description = "Main Parking",
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Mode = "MODE1",
                    Block = 30,
                }
            };

            var createReturn = new Return<Gate>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(userReturn);
            _gateRepositoryMock.Setup(x => x.GetGateByNameAsync(req.Name)).ReturnsAsync(gateReturn);
            _parkingRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(req.ParkingAreaId)).ReturnsAsync(parkingReturn);
            _gateRepositoryMock.Setup(x => x.CreateGateAsync(It.IsAny<Gate>())).ReturnsAsync(createReturn);

            // Act
            var result = await _gateService.CreateGateAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdateGateAsync
        // Successful
        [Fact]
        public async Task UpdateGateAsync_ShouldReturnSuccess_WhenNotUpdateParkingArea()
        {
            // Arrange
            var gateId = Guid.NewGuid();
            var parkingId = Guid.NewGuid();

            var req = new UpdateGateReqDto
            {
                Name = "Updated Gate",
                Description = "Updated Description",
            };

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var existingGate = new Gate
            {
                Id = gateId,
                Name = "Old Gate",
                Description = "Old Description",
                ParkingArea = new ParkingArea
                {
                    Id = parkingId,
                    Name = "FPTU1",
                    Description = "Main Parking",
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Mode = "MODE1",
                    Block = 30,
                },
                StatusGate = StatusGateEnum.ACTIVE
            };

            var updatedGate = new Gate
            {
                Id = gateId,
                Name = req.Name,
                Description = req.Description,
                ParkingArea = existingGate.ParkingArea, 
                StatusGate = StatusGateEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = existingGate,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByNameAsync(req.Name))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = updatedGate,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            // Act
            var result = await _gateService.UpdateGateAsync(req, gateId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successful
        [Fact]
        public async Task UpdateGateAsync_ShouldReturnSuccess_WhenUpdateParkingArea()
        {
            // Arrange
            var gateId = Guid.NewGuid();
            var parkingId = Guid.NewGuid();

            var req = new UpdateGateReqDto
            {
                Name = "Updated Gate",
                Description = "Updated Description",
                ParkingAreaId = Guid.NewGuid()
            };

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var existingGate = new Gate
            {
                Id = gateId,
                Name = "Old Gate",
                Description = "Old Description",
                ParkingArea = new ParkingArea
                {
                    Id = parkingId,
                    Name = "FPTU1",
                    Description = "Main Parking",
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Mode = "MODE1",
                    Block = 30,
                },
                StatusGate = StatusGateEnum.ACTIVE
            };

            var updatedGate = new Gate
            {
                Id = gateId,
                Name = req.Name,
                Description = req.Description,
                ParkingArea = existingGate.ParkingArea,
                StatusGate = StatusGateEnum.ACTIVE
            };

            var parkingReturn = new Return<ParkingArea>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = existingGate.ParkingArea,
                IsSuccess = true,
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = existingGate,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByNameAsync(req.Name))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            if (req.ParkingAreaId != null)
            {
                _parkingRepositoryMock
                    .Setup(x => x.GetParkingAreaByIdAsync(req.ParkingAreaId.Value))
                    .ReturnsAsync(parkingReturn);
            };

            _gateRepositoryMock
                .Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = updatedGate,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            // Act
            var result = await _gateService.UpdateGateAsync(req, gateId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateGateAsync_ShouldReturnsFailure_WhenInvalidUser()
        {
            // Arrange
            var gateId = Guid.NewGuid();
            var req = new UpdateGateReqDto();

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _gateService.UpdateGateAsync(req, gateId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateGateAsync_ShouldReturnsFailure_WhenGateNotExist()
        {
            // Arrange
            var gateId = Guid.NewGuid();
            var req = new UpdateGateReqDto();

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var existingGateResult = new Return<Gate> { Message = ErrorEnumApplication.NOT_FOUND_OBJECT };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _gateRepositoryMock
               .Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
               .ReturnsAsync(new Return<Gate>
               {
                   IsSuccess = false,
                   Message = ErrorEnumApplication.GATE_NOT_EXIST
               });

            // Act
            var result = await _gateService.UpdateGateAsync(req, gateId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.GATE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateGateAsync_ShouldReturnsFailure_WhenParkingAreaNotExist()
        {
            // Arrange
            var gateId = Guid.NewGuid();
            var parkingId = Guid.NewGuid();

            var req = new UpdateGateReqDto 
            { 
                ParkingAreaId = Guid.NewGuid() 
            };

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var existingGate = new Gate
            {
                Id = gateId,
                Name = "Old Gate",
                Description = "Old Description",
                ParkingArea = new ParkingArea
                {
                    Id = parkingId,
                    Name = "FPTU1",
                    Description = "Main Parking",
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Mode = "MODE1",
                    Block = 30,
                },
                StatusGate = StatusGateEnum.ACTIVE
            };

            var parkingReturn = new Return<ParkingArea>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                Data = null,
                IsSuccess = false,
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = existingGate,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            if (req.ParkingAreaId != null)
            {
                _parkingRepositoryMock
                    .Setup(x => x.GetParkingAreaByIdAsync(req.ParkingAreaId.Value))
                    .ReturnsAsync(parkingReturn);
            };

            _gateRepositoryMock
                 .Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
                 .ReturnsAsync(new Return<Gate>
                 {
                     IsSuccess = false,
                     Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST
                 });

            // Act
            var result = await _gateService.UpdateGateAsync(req, gateId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PARKING_AREA_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateGateAsync_ShouldReturnsFailure_WhenGateNameExisted()
        {
            // Arrange
            var gateId = Guid.NewGuid();
            var parkingId = Guid.NewGuid();

            var req = new UpdateGateReqDto
            {
                Name = "Updated Gate",
                Description = "Updated Description",
            };

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var existingGate = new Gate
            {
                Id = gateId,
                Name = "Old Gate",
                Description = "Old Description",
                ParkingArea = new ParkingArea
                {
                    Id = parkingId,
                    Name = "FPTU1",
                    Description = "Main Parking",
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Mode = "MODE1",
                    Block = 30,
                },
                StatusGate = StatusGateEnum.ACTIVE
            };

            var updatedGate = new Gate
            {
                Id = gateId,
                Name = req.Name,
                Description = req.Description,
                ParkingArea = existingGate.ParkingArea,
                StatusGate = StatusGateEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = existingGate,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByNameAsync(req.Name))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = existingGate,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.OBJECT_EXISTED
                });

            // Act
            var result = await _gateService.UpdateGateAsync(req, gateId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.OBJECT_EXISTED, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateGateAsync_ShouldReturnsServerError_WhenUpdateFailure()
        {
            // Arrange
            var gateId = Guid.NewGuid();
            var parkingId = Guid.NewGuid();

            var req = new UpdateGateReqDto
            {
                Name = "Updated Gate",
                Description = "Updated Description",
            };

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var existingGate = new Gate
            {
                Id = gateId,
                Name = "Old Gate",
                Description = "Old Description",
                ParkingArea = new ParkingArea
                {
                    Id = parkingId,
                    Name = "FPTU1",
                    Description = "Main Parking",
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Mode = "MODE1",
                    Block = 30,
                },
                StatusGate = StatusGateEnum.ACTIVE
            };

            var updatedGate = new Gate
            {
                Id = gateId,
                Name = req.Name,
                Description = req.Description,
                ParkingArea = existingGate.ParkingArea,
                StatusGate = StatusGateEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = existingGate,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByNameAsync(req.Name))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _gateService.UpdateGateAsync(req, gateId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // DeleteGate
        // Successful
        [Fact]
        public async Task DeleteGate_ShouldReturnsSuccess()
        {
            // Arrange
            var gateId = Guid.NewGuid();

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var existingGate = new Gate
            {
                Id = gateId,
                Name = "Old Gate",
                Description = "Old Description",
                ParkingArea = new ParkingArea
                {
                    Name = "FPTU1",
                    Description = "Main Parking",
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Mode = "MODE1",
                    Block = 30,
                },
                StatusGate = StatusGateEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = existingGate,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = new Gate
                    {
                        Name = existingGate.Name,
                        Description = existingGate.Description,
                        StatusGate = StatusGateEnum.ACTIVE,
                        DeletedDate = DateTime.Now,
                    },
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            // Act
            var result = await _gateService.DeleteGate(gateId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteGate_ShouldReturnFailure_UnauthorizedUser()
        {
            // Arrange
            var gateId = Guid.NewGuid();

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _gateService.DeleteGate(gateId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteGate_ShouldReturnsFailure_WhenGateNotFound()
        {
            // Arrange
            var gateId = Guid.NewGuid();

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GATE_NOT_EXIST
                });

            // Act
            var result = await _gateService.DeleteGate(gateId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.GATE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteGate_ShouldReturnsServerError_WhenUpdateFails_()
        {
            // Arrange
            var gateId = Guid.NewGuid();

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var existingGate = new Gate
            {
                Id = gateId,
                Name = "Old Gate",
                Description = "Old Description",
                ParkingArea = new ParkingArea
                {
                    Name = "FPTU1",
                    Description = "Main Parking",
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Mode = "MODE1",
                    Block = 30,
                },
                StatusGate = StatusGateEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = existingGate,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _gateService.DeleteGate(gateId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdateStatusGateAsync
        // Successful
        [Fact]
        public async Task UpdateStatusGateAsync_ShouldReturnsSuccess_WhenInActiveToActive()
        {
            // Arrange
            var gateId = Guid.NewGuid();

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var existingGate = new Gate
            {
                Id = gateId,
                Name = "Old Gate",
                Description = "Old Description",
                ParkingArea = new ParkingArea
                {
                    Name = "FPTU1",
                    Description = "Main Parking",
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Mode = "MODE1",
                    Block = 30,
                },
                StatusGate = StatusGateEnum.INACTIVE
            };

            var isActive = true;

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = existingGate,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
               .ReturnsAsync(new Return<Gate>
               {
                   IsSuccess = true,
                   Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                   Data = new Gate
                   {
                       Name = existingGate.Name,
                       StatusGate = isActive ? StatusGateEnum.ACTIVE : StatusGateEnum.INACTIVE
                   }
               });

            // Act
            var result = await _gateService.UpdateStatusGateAsync(gateId, isActive);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successful
        [Fact]
        public async Task UpdateStatusGateAsync_ShouldReturnsSuccess_WhenActiveToInActive()
        {
            // Arrange
            var gateId = Guid.NewGuid();

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var existingGate = new Gate
            {
                Id = gateId,
                Name = "Old Gate",
                Description = "Old Description",
                ParkingArea = new ParkingArea
                {
                    Name = "FPTU1",
                    Description = "Main Parking",
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Mode = "MODE1",
                    Block = 30,
                },
                StatusGate = StatusGateEnum.ACTIVE
            };

            var isActive = false;

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = existingGate,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
               .ReturnsAsync(new Return<Gate>
               {
                   IsSuccess = true,
                   Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                   Data = new Gate
                   {
                       Name = existingGate.Name,
                       StatusGate = isActive ? StatusGateEnum.ACTIVE : StatusGateEnum.INACTIVE
                   }
               });

            // Act
            var result = await _gateService.UpdateStatusGateAsync(gateId, isActive);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusGateAsync_ShouldReturnsUnauthorized_WhenUnauthorizedUser()
        {
            // Arrange
            var gateId = Guid.NewGuid();
            var isActive = true;

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _gateService.UpdateStatusGateAsync(gateId, isActive);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusGateAsync_ShouldReturnsFailure_WhenGateNotFound()
        {
            // Arrange
            var gateId = Guid.NewGuid();
            var isActive = true;

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
               .ReturnsAsync(new Return<Gate>
               {
                   IsSuccess = false,
                   Message = ErrorEnumApplication.GATE_NOT_EXIST,
               });

            // Act
            var result = await _gateService.UpdateStatusGateAsync(gateId, isActive);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.GATE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusGateAsync_ShouldReturnsFailure_WhenStatusAlreadyApplied()
        {
            // Arrange
            var gateId = Guid.NewGuid();
            var isActive = true;

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var existingGate = new Gate
            {
                Id = gateId,
                Name = "Old Gate",
                Description = "Old Description",
                ParkingArea = new ParkingArea
                {
                    Name = "FPTU1",
                    Description = "Main Parking",
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Mode = "MODE1",
                    Block = 30,
                },
                StatusGate = StatusGateEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = existingGate,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
               .ReturnsAsync(new Return<Gate>
               {
                   IsSuccess = true,
                   Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY,
               });

            // Act
            var result = await _gateService.UpdateStatusGateAsync(gateId, isActive);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.STATUS_IS_ALREADY_APPLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusGateAsync_ShouldReturnsFailure_WhenUpdateFails()
        {
            // Arrange
            var gateId = Guid.NewGuid();

            var user = new User
            {
                Email = "manager@localhost.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            var existingGate = new Gate
            {
                Id = gateId,
                Name = "Old Gate",
                Description = "Old Description",
                ParkingArea = new ParkingArea
                {
                    Name = "FPTU1",
                    Description = "Main Parking",
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Mode = "MODE1",
                    Block = 30,
                },
                StatusGate = StatusGateEnum.INACTIVE
            };

            var isActive = true;

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock
                .Setup(x => x.GetGateByIdAsync(gateId))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Data = existingGate,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.UpdateGateAsync(It.IsAny<Gate>()))
               .ReturnsAsync(new Return<Gate>
               {
                   IsSuccess = false,
                   Message = ErrorEnumApplication.SERVER_ERROR,
               });

            // Act
            var result = await _gateService.UpdateStatusGateAsync(gateId, isActive);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetListGateByParkingAreaAsync
        // Successful
        [Fact]
        public async Task GetListGateByParkingAreaAsync_ShouldReturnSuccess_WhenGateFound()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "staff@localhost.com",
                FullName = "staff",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            var parking = new ParkingArea
            {
                Id = parkingAreaId,
                Name = "FPTU1",
                Description = "Main Parking",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
                Mode = "MODE1",
                Block = 30,
            };

            _parkingRepositoryMock
                .Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Data = parking,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            var gateList = new List<Gate>
            {
                new() {
                    Name = "Old Gate",
                    Description = "Old Description",
                    ParkingArea = parking,
                    StatusGate = StatusGateEnum.ACTIVE,
                }
            };

            var gateReturn = new Return<IEnumerable<Gate>>
            {
                IsSuccess = true,
                Data = gateList,
                TotalRecord = gateList.Count,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _gateRepositoryMock.Setup(x => x.GetListGateByParkingAreaAsync(parkingAreaId)).ReturnsAsync(gateReturn);

            // Act
            var result = await _gateService.GetListGateByParkingAreaAsync(parkingAreaId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetListGateByParkingAreaAsync_ShouldReturnSuccess_WhenGateNotFound()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "staff@localhost.com",
                FullName = "staff",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            var parking = new ParkingArea
            {
                Id = parkingAreaId,
                Name = "FPTU1",
                Description = "Main Parking",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
                Mode = "MODE1",
                Block = 30,
            };

            _parkingRepositoryMock
                .Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Data = parking,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            var gateList = new List<Gate>();

            var gateReturn = new Return<IEnumerable<Gate>>
            {
                IsSuccess = true,
                Data = gateList,
                TotalRecord = gateList.Count,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _gateRepositoryMock.Setup(x => x.GetListGateByParkingAreaAsync(parkingAreaId)).ReturnsAsync(gateReturn);

            // Act
            var result = await _gateService.GetListGateByParkingAreaAsync(parkingAreaId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListGateByParkingAreaAsync_ShouleReturnsFailure_WhenUnauthorizedUser()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _gateService.GetListGateByParkingAreaAsync(parkingAreaId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        [Fact]
        public async Task GetListGateByParkingAreaAsync_ShouleReturnsFailure_WhenParkingAreaNotFound()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();

            var user = new User
            {
                Email = "staff@localhost.com",
                FullName = "staff",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _parkingRepositoryMock
               .Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
               .ReturnsAsync(new Return<ParkingArea>
               {
                   IsSuccess = false,
                   Data = null,
                   Message = ErrorEnumApplication.NOT_FOUND_OBJECT
               });

            // Act
            var result = await _gateService.GetListGateByParkingAreaAsync(parkingAreaId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PARKING_AREA_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListGateByParkingAreaAsync_ShouleReturnsFailure_WhenServerError()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "staff@localhost.com",
                FullName = "staff",
                PasswordHash = "",
                PasswordSalt = "",
                StatusUser = StatusUserEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            var parking = new ParkingArea
            {
                Id = parkingAreaId,
                Name = "FPTU1",
                Description = "Main Parking",
                StatusParkingArea = StatusParkingEnum.ACTIVE,
                Mode = "MODE1",
                Block = 30,
            };

            _parkingRepositoryMock
                .Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Data = parking,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            var gateList = new List<Gate>
            {
                new() {
                    Name = "Old Gate",
                    Description = "Old Description",
                    ParkingArea = parking,
                    StatusGate = StatusGateEnum.ACTIVE,
                }
            };

            var gateReturn = new Return<IEnumerable<Gate>>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _gateRepositoryMock.Setup(x => x.GetListGateByParkingAreaAsync(parkingAreaId)).ReturnsAsync(gateReturn);

            // Act
            var result = await _gateService.GetListGateByParkingAreaAsync(parkingAreaId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CreateGatesForParkingAreaByStaffAsync
        // Failure
        [Fact]
        public async Task CreateGatesForParkingAreaByStaffAsync_ShouldReturnFailure_WhenAuthorizationFails()
        {
            // Arrange
            var req = new CreateGatesForParkingAreaByStaffReqDto ();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                });

            // Act
            var result = await _gateService.CreateGatesForParkingAreaByStaffAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateGatesForParkingAreaByStaffAsync_ShouldReturnFailure_WhenParkingAreaDoesNotExist()
        {
            // Arrange
            var req = new CreateGatesForParkingAreaByStaffReqDto();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    }
                });

            _parkingRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST,
                });

            // Act
            var result = await _gateService.CreateGatesForParkingAreaByStaffAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PARKING_AREA_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateGatesForParkingAreaByStaffAsync_ShouldReturnFailure_WhenGateNameAlreadyExists()
        {
            // Arrange
            var req = new CreateGatesForParkingAreaByStaffReqDto
            {
                ParkingAreaId = Guid.NewGuid(),
                Gates = new ListGateRegister[] 
                { 
                    new ListGateRegister
                    { 
                        Name = "Gate A" 
                    } 
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    }
                });

            _parkingRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new ParkingArea
                    {
                        Block = 30,
                        Mode = ModeEnum.MODE1,
                        Name = "FPTU1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE,
                    }
                });

            _gateRepositoryMock.Setup(x => x.GetGateByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _gateService.CreateGatesForParkingAreaByStaffAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.OBJECT_EXISTED, result.Message);
        }

        // Successful
        [Fact]
        public async Task CreateGatesForParkingAreaByStaffAsync_ShouldReturnSuccess()
        {
            // Arrange
            var req = new CreateGatesForParkingAreaByStaffReqDto
            {
                ParkingAreaId = Guid.NewGuid(),
                Gates = new ListGateRegister[]
               {
                    new ListGateRegister
                    {
                        Name = "Gate A"
                    }
               }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    }
                });

            _parkingRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new ParkingArea
                    {
                        Block = 30,
                        Mode = ModeEnum.MODE1,
                        Name = "FPTU1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE,
                    }
                });

            _gateRepositoryMock.Setup(x => x.GetGateByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.CreateGateAsync(It.IsAny<Gate>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                });

            // Act
            var result = await _gateService.CreateGatesForParkingAreaByStaffAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        //Failure
        [Fact]
        public async Task CreateGatesForParkingAreaByStaffAsync_ShouldReturnFailure_WhenCreateFail()
        {
            // Arrange
            var req = new CreateGatesForParkingAreaByStaffReqDto
            {
                ParkingAreaId = Guid.NewGuid(),
                Gates = new ListGateRegister[]
               {
                    new ListGateRegister
                    {
                        Name = "Gate A"
                    }
               }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    }
                });

            _parkingRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new ParkingArea
                    {
                        Block = 30,
                        Mode = ModeEnum.MODE1,
                        Name = "FPTU1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE,
                    }
                });

            _gateRepositoryMock.Setup(x => x.GetGateByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _gateRepositoryMock.Setup(x => x.CreateGateAsync(It.IsAny<Gate>()))
                .ReturnsAsync(new Return<Gate>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _gateService.CreateGatesForParkingAreaByStaffAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetAllGateByParkingAreaAsync
        // Successful
        [Fact]
        public async Task GetAllGateByParkingAreaAsync_ShouldReturnSuccess_WhenGateFound()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();
            var gates = new List<Gate>
            {
                new Gate
                {
                    Id = Guid.NewGuid(),
                    Name = "Gate A",
                    Description = "Main Gate",
                    StatusGate = StatusParkingEnum.ACTIVE
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    }
                });

            _parkingRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new ParkingArea
                    {
                        Block = 30,
                        Mode = ModeEnum.MODE1,
                        Name = "FPTU1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE,
                    }
                });

            _gateRepositoryMock.Setup(x => x.GetAllGateByParkingAreaAsync(parkingAreaId))
                .ReturnsAsync(new Return<IEnumerable<Gate>>
                {
                    IsSuccess = true,
                    Data = gates,
                    TotalRecord = gates.Count,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _gateService.GetAllGateByParkingAreaAsync(parkingAreaId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetAllGateByParkingAreaAsync_ShouldReturnSuccess_WhenGateNotFound()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();
            var gates = new List<Gate>();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    }
                });

            _parkingRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new ParkingArea
                    {
                        Block = 30,
                        Mode = ModeEnum.MODE1,
                        Name = "FPTU1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE,
                    }
                });

            _gateRepositoryMock.Setup(x => x.GetAllGateByParkingAreaAsync(parkingAreaId))
                .ReturnsAsync(new Return<IEnumerable<Gate>>
                {
                    IsSuccess = true,
                    Data = gates,
                    TotalRecord = gates.Count,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _gateService.GetAllGateByParkingAreaAsync(parkingAreaId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllGateByParkingAreaAsync_ShouldReturnFailure_WhenAuthorizationFails()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                });

            // Act
            var result = await _gateService.GetAllGateByParkingAreaAsync(parkingAreaId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllGateByParkingAreaAsync_ShouldReturnError_WhenParkingAreaDoesNotExist()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    }
                });

            _parkingRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST,
                });

            // Act
            var result = await _gateService.GetAllGateByParkingAreaAsync(parkingAreaId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PARKING_AREA_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllGateByParkingAreaAsync_ShouldReturnFailure_WhenGetFail()
        {
            // Arrange
            var parkingAreaId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "user@gmail.com",
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                    }
                });

            _parkingRepositoryMock.Setup(x => x.GetParkingAreaByIdAsync(parkingAreaId))
                .ReturnsAsync(new Return<ParkingArea>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new ParkingArea
                    {
                        Block = 30,
                        Mode = ModeEnum.MODE1,
                        Name = "FPTU1",
                        StatusParkingArea = StatusParkingEnum.ACTIVE,
                    }
                });

            _gateRepositoryMock.Setup(x => x.GetAllGateByParkingAreaAsync(parkingAreaId))
                .ReturnsAsync(new Return<IEnumerable<Gate>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _gateService.GetAllGateByParkingAreaAsync(parkingAreaId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

    }
}
