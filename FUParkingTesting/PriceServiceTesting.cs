using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Price;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService;
using FUParkingService.Interface;
using Moq;
using Org.BouncyCastle.Ocsp;
using Xunit;

namespace FUParkingTesting
{
    public class PriceServiceTesting
    {
        private readonly Mock<IPriceRepository> _priceRepositoryMock = new();
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock = new();
        private readonly Mock<IHelpperService> _helpperServiceMock = new();

        private readonly PriceSevice _priceService;

        public PriceServiceTesting()
        {
            _priceRepositoryMock = new Mock<IPriceRepository>();
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();

            _priceService = new PriceSevice(_priceRepositoryMock.Object, _vehicleRepositoryMock.Object, _helpperServiceMock.Object);
        }

        // CreateDefaultPriceItemForDefaultPriceTableAsync
        // Successful
        [Fact]
        public async Task CreateDefaultPriceItemForDefaultPriceTableAsync_ShouldReturnSuccess()
        {
            // Arrange
            var req = new CreateDefaultItemPriceReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                MaxPrice = 100,
                MinPrice = 50,
                BlockPricing = 10
            };

            var vehicleType = new VehicleType
            {
                Name = "Motocycle",
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId))
                .ReturnsAsync(new Return<VehicleType> 
                { 
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = vehicleType
                });

            _priceRepositoryMock.Setup(x => x.GetDefaultPriceTableByVehicleTypeAsync(req.VehicleTypeId))
                .ReturnsAsync(new Return<PriceTable> 
                { 
                    IsSuccess = true, 
                    Data = new PriceTable 
                    { 
                        Id = Guid.NewGuid(),
                        Priority = 1,
                        Name = "Default",
                        StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.CreatePriceItemAsync(It.IsAny<PriceItem>()))
                .ReturnsAsync(new Return<PriceItem> 
                { 
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    Data = new PriceItem
                    {
                        PriceTableId = Guid.NewGuid(),
                        MaxPrice = req.MaxPrice,
                        MinPrice = req.MinPrice,
                        BlockPricing = req.BlockPricing,
                    },
                    IsSuccess = true
                });

            // Act
            var result = await _priceService.CreateDefaultPriceItemForDefaultPriceTableAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateDefaultPriceItemForDefaultPriceTableAsync_ShouldReturnFailure_WhenAuthenticationFailed()
        {
            // Arrange
            var req = new CreateDefaultItemPriceReqDto();
            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _priceService.CreateDefaultPriceItemForDefaultPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateDefaultPriceItemForDefaultPriceTableAsync_ShouldReturnFailure_WhenVehicleTypeNotExist()
        {
            // Arrange
            var req = new CreateDefaultItemPriceReqDto ();

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = false,
                    Data = null
                });

            // Act
            var result = await _priceService.CreateDefaultPriceItemForDefaultPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateDefaultPriceItemForDefaultPriceTableAsync_ShouldReturnFailure_WhenDefaultPriceTableNotExist()
        {
            // Arrange
            var req = new CreateDefaultItemPriceReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                MaxPrice = 100,
                MinPrice = 50,
                BlockPricing = 10
            };

            var vehicleType = new VehicleType
            {
                Name = "Motocycle",
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = vehicleType
                });

            _priceRepositoryMock.Setup(x => x.GetDefaultPriceTableByVehicleTypeAsync(req.VehicleTypeId))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _priceService.CreateDefaultPriceItemForDefaultPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.DEFAULT_PRICE_TABLE_IS_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateDefaultPriceItemForDefaultPriceTableAsync_ShouldReturnFailure_WhenCreatePriceItemFailed()
        {
            // Arrange
            var req = new CreateDefaultItemPriceReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                MaxPrice = 100,
                MinPrice = 50,
                BlockPricing = 10
            };

            var vehicleType = new VehicleType
            {
                Name = "Motocycle",
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true,
                    Data = vehicleType
                });

            _priceRepositoryMock.Setup(x => x.GetDefaultPriceTableByVehicleTypeAsync(req.VehicleTypeId))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = true,
                    Data = new PriceTable
                    {
                        Id = Guid.NewGuid(),
                        Priority = 1,
                        Name = "Default",
                        StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.CreatePriceItemAsync(It.IsAny<PriceItem>()))
                .ReturnsAsync(new Return<PriceItem>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    Data = null,
                    IsSuccess = false
                });

            // Act
            var result = await _priceService.CreateDefaultPriceItemForDefaultPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CreatePriceItemAsync
        // Successful
        [Fact]
        public async Task CreatePriceItemAsync_ShouldReturnSuccess()
        {
            // Arrange
            var req = new CreateListPriceItemReqDto
            {
                PriceTableId = Guid.NewGuid(),
                PriceItems =
                [
                    new CreatePriceItemReqDto
                    { 
                        From = 0, 
                        To = 12, 
                        MaxPrice = 100, 
                        MinPrice = 50, 
                        BlockPricing = 10 
                    },
                ]
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = true,
                    Data = new PriceTable
                    {
                        Id = Guid.NewGuid(),
                        Priority = 1,
                        Name = "Price Table 1",
                        StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock
                .Setup(x => x.GetAllPriceItemByPriceTableAsync(req.PriceTableId))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data =
                    [
                        new PriceItem
                        {
                            Id = Guid.NewGuid(),
                            PriceTableId = req.PriceTableId,
                            MaxPrice = 100,
                            MinPrice = 50,
                            BlockPricing = 10
                        }
                    ],
                    IsSuccess = true
                });

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceTableByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<PriceTable> 
                { 
                    Message = SuccessfullyEnumServer.FOUND_OBJECT, 
                    Data = new PriceTable 
                    {
                        Id = Guid.NewGuid(),
                        Priority = 1,
                        Name = "Default",
                        StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    },
                });

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceItemByPriceTableIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<PriceItem> 
                { 
                    Message = SuccessfullyEnumServer.FOUND_OBJECT, 
                    Data = new PriceItem 
                    { 
                        MaxPrice = 100, 
                        MinPrice = 50, 
                        BlockPricing = 10 
                    } 
                });

            _priceRepositoryMock
                .Setup(x => x.CreatePriceItemAsync(It.IsAny<PriceItem>()))
                .ReturnsAsync(new Return<PriceItem> 
                { 
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    Data = new PriceItem
                    {
                        PriceTableId = Guid.NewGuid(),
                        MaxPrice = 100,
                        MinPrice = 50,
                        BlockPricing = 10,
                    },
                    IsSuccess = true
                });

            // Act
            var result = await _priceService.CreatePriceItemAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreatePriceItemAsync_ShouldReturnFailure_WhenAuthenticationFailed()
        {
            // Arrange
            var req = new CreateListPriceItemReqDto();
            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User> 
                { 
                    IsSuccess = false, 
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _priceService.CreatePriceItemAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreatePriceItemAsync_ShouldReturnFailure_WhenPriceTableNotExist()
        {
            // Arrange
            var req = new CreateListPriceItemReqDto
            {
                PriceTableId = Guid.NewGuid(),
                PriceItems =
                [
                    new CreatePriceItemReqDto
                    {
                        From = 0,
                        To = 12,
                        MaxPrice = 100,
                        MinPrice = 50,
                        BlockPricing = 10
                    },
                ]
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
               .Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
               .ReturnsAsync(new Return<PriceTable>
               {
                   IsSuccess = false,
                   Data = null,
                   Message = ErrorEnumApplication.NOT_FOUND_OBJECT
               });

            // Act
            var result = await _priceService.CreatePriceItemAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PRICE_TABLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreatePriceItemAsync_ShouldReturnFailure_WhenPriceItemAlreadyExists()
        {
            // Arrange
            var req = new CreateListPriceItemReqDto
            {
                PriceTableId = Guid.NewGuid(),
                PriceItems =
                [
                    new CreatePriceItemReqDto
                    {
                        From = 0,
                        To = 12,
                        MaxPrice = 100,
                        MinPrice = 50,
                        BlockPricing = 10
                    },
                ]
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = true,
                    Data = new PriceTable
                    {
                        Id = Guid.NewGuid(),
                        Priority = 1,
                        Name = "Price Table 1",
                        StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock
                .Setup(x => x.GetAllPriceItemByPriceTableAsync(req.PriceTableId))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = true,
                    Data =
                    [
                        new PriceItem 
                        { 
                            ApplyFromHour = 0, 
                            ApplyToHour = 24 
                        } 
                    ],
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });



            // Act
            var result = await _priceService.CreatePriceItemAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PRICE_ITEM_IS_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreatePriceItemAsync_ShouldReturnFailure_WhenDefaultPriceTableNotExist()
        {
            // Arrange
            var req = new CreateListPriceItemReqDto
            {
                PriceTableId = Guid.NewGuid(),
                PriceItems =
                [
                    new CreatePriceItemReqDto
                    {
                        From = 0,
                        To = 12,
                        MaxPrice = 100,
                        MinPrice = 50,
                        BlockPricing = 10
                    },
                ]
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = true,
                    Data = new PriceTable
                    {
                        Id = Guid.NewGuid(),
                        Priority = 1,
                        Name = "Price Table 1",
                        StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock
                .Setup(x => x.GetAllPriceItemByPriceTableAsync(req.PriceTableId))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data =
                    [
                        new PriceItem
                        {
                            Id = Guid.NewGuid(),
                            PriceTableId = req.PriceTableId,
                            MaxPrice = 100,
                            MinPrice = 50,
                            BlockPricing = 10
                        }
                    ],
                    IsSuccess = true
                });

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceTableByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<PriceTable>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = false
                });

            // Act
            var result = await _priceService.CreatePriceItemAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.DEFAULT_PRICE_TABLE_IS_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreatePriceItemAsync_ShouldReturnFailure_WhenDefaultPriceItemNotExist()
        {
            // Arrange
            var req = new CreateListPriceItemReqDto
            {
                PriceTableId = Guid.NewGuid(),
                PriceItems =
                [
                    new CreatePriceItemReqDto
                    {
                        From = 0,
                        To = 12,
                        MaxPrice = 100,
                        MinPrice = 50,
                        BlockPricing = 10
                    },
                ]
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = true,
                    Data = new PriceTable
                    {
                        Id = Guid.NewGuid(),
                        Priority = 1,
                        Name = "Price Table 1",
                        StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock
                .Setup(x => x.GetAllPriceItemByPriceTableAsync(req.PriceTableId))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data =
                    [
                        new PriceItem
                        {
                            Id = Guid.NewGuid(),
                            PriceTableId = req.PriceTableId,
                            MaxPrice = 100,
                            MinPrice = 50,
                            BlockPricing = 10
                        }
                    ],
                    IsSuccess = true
                });

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceTableByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<PriceTable>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new PriceTable
                    {
                        Id = Guid.NewGuid(),
                        Priority = 1,
                        Name = "Default",
                        StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    },
                });

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceItemByPriceTableIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<PriceItem>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = false
                });

            // Act
            var result = await _priceService.CreatePriceItemAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.DEFAULT_PRICE_ITEM_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreatePriceItemAsync_ShouldReturnFailure_WhenCreatePriceItemFailed()
        {
            // Arrange
            var req = new CreateListPriceItemReqDto
            {
                PriceTableId = Guid.NewGuid(),
                PriceItems =
                [
                    new CreatePriceItemReqDto
                    {
                        From = 0,
                        To = 12,
                        MaxPrice = 100,
                        MinPrice = 50,
                        BlockPricing = 10
                    },
                ]
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = true,
                    Data = new PriceTable
                    {
                        Id = Guid.NewGuid(),
                        Priority = 1,
                        Name = "Price Table 1",
                        StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock
                .Setup(x => x.GetAllPriceItemByPriceTableAsync(req.PriceTableId))
                .ReturnsAsync(new Return<IEnumerable<PriceItem>>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data =
                    [
                        new PriceItem
                        {
                            Id = Guid.NewGuid(),
                            PriceTableId = req.PriceTableId,
                            MaxPrice = 100,
                            MinPrice = 50,
                            BlockPricing = 10
                        }
                    ],
                    IsSuccess = true
                });

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceTableByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<PriceTable>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new PriceTable
                    {
                        Id = Guid.NewGuid(),
                        Priority = 1,
                        Name = "Default",
                        StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    },
                });

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceItemByPriceTableIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<PriceItem>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new PriceItem
                    {
                        MaxPrice = 100,
                        MinPrice = 50,
                        BlockPricing = 10
                    }
                });

            _priceRepositoryMock
                .Setup(x => x.CreatePriceItemAsync(It.IsAny<PriceItem>()))
                .ReturnsAsync(new Return<PriceItem>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false
                });

            // Act
            var result = await _priceService.CreatePriceItemAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetAllPriceItemByPriceTableAsync
        // Successful
        [Fact]
        public async Task GetAllPriceItemByPriceTableAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var priceTableId = Guid.NewGuid();
            var req = new GetListObjectWithPageReqDto();

            var priceTable = new PriceTable
            {
                Id = priceTableId,
                Priority = 1,
                Name = "Price Table 1",
                StatusPriceTable = StatusPriceTableEnum.ACTIVE,
            };

            var priceTableReturn = new Return<PriceTable> 
            { 
                IsSuccess = true, 
                Data = priceTable,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var priceItems = new List<PriceItem>
            {
                new() { 
                    Id = Guid.NewGuid(), 
                    ApplyToHour = 1, 
                    ApplyFromHour = 0, 
                    MaxPrice = 100, 
                    MinPrice = 50 
                }
            };
            var priceItemsReturn = new Return<IEnumerable<PriceItem>> 
            { 
                IsSuccess = true, 
                Data = priceItems, 
                TotalRecord = 1,
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

            _priceRepositoryMock.Setup(x => x.GetPriceTableByIdAsync(priceTableId)).ReturnsAsync(priceTableReturn);
            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableWithPageAsync(priceTableId, req)).ReturnsAsync(priceItemsReturn);

            // Act
            var result = await _priceService.GetAllPriceItemByPriceTableAsync(priceTableId, req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.TotalRecord);
        }

        // Failure
        [Fact]
        public async Task GetAllPriceItemByPriceTableAsync_ShouldReturnsFailure_AuthenticationFailed()
        {
            // Arrange
            var priceTableId = Guid.NewGuid();
            var req = new GetListObjectWithPageReqDto();

            _helpperServiceMock
              .Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
              .ReturnsAsync(new Return<User>
              {
                  IsSuccess = false,
                  Message = ErrorEnumApplication.NOT_AUTHENTICATION
              });

            // Act
            var result = await _priceService.GetAllPriceItemByPriceTableAsync(priceTableId, req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllPriceItemByPriceTableAsync_ShouldReturnsFailure_WhenPriceTableNotExist()
        {
            // Arrange
            var priceTableId = Guid.NewGuid();
            var req = new GetListObjectWithPageReqDto();

            var priceTableReturn = new Return<PriceTable>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
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

            _priceRepositoryMock.Setup(x => x.GetPriceTableByIdAsync(priceTableId)).ReturnsAsync(priceTableReturn);

            // Act
            var result = await _priceService.GetAllPriceItemByPriceTableAsync(priceTableId, req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PRICE_TABLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllPriceItemByPriceTableAsync_RepositoryFailure_ReturnsFailure()
        {
            // Arrange
            var priceTableId = Guid.NewGuid();
            var req = new GetListObjectWithPageReqDto();

            var priceTable = new PriceTable
            {
                Id = priceTableId,
                Priority = 1,
                Name = "Price Table 1",
                StatusPriceTable = StatusPriceTableEnum.ACTIVE,
            };

            var priceTableReturn = new Return<PriceTable>
            {
                IsSuccess = true,
                Data = priceTable,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var priceItems = new List<PriceItem>
            {
                new() {
                    Id = Guid.NewGuid(),
                    ApplyToHour = 1,
                    ApplyFromHour = 0,
                    MaxPrice = 100,
                    MinPrice = 50
                }
            };
            var priceItemsReturn = new Return<IEnumerable<PriceItem>>
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

            _priceRepositoryMock.Setup(x => x.GetPriceTableByIdAsync(priceTableId)).ReturnsAsync(priceTableReturn);
            _priceRepositoryMock.Setup(x => x.GetAllPriceItemByPriceTableWithPageAsync(priceTableId, req)).ReturnsAsync(priceItemsReturn);

            // Act
            var result = await _priceService.GetAllPriceItemByPriceTableAsync(priceTableId, req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdatePriceItemAsync
        // Successful
        [Fact]
        public async Task UpdatePriceItemAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var req = new CreateListPriceItemReqDto
            {
                PriceTableId = Guid.NewGuid(),
                PriceItems =
                [
                    new CreatePriceItemReqDto
                    {
                        From = 0,
                        To = 12,
                        MinPrice = 10,
                        MaxPrice = 20,
                        BlockPricing = 2
                    },
                ]
            };

            var priceTableReturn = new Return<PriceTable>
            {
                Data = new PriceTable
                {
                    VehicleTypeId = Guid.NewGuid(),
                    Priority = 1,
                    Name = "Price Table 1",
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var defaultPriceItemReturn = new Return<PriceItem>
            {
                Data = new PriceItem
                {
                    MaxPrice = 30,
                    MinPrice = 5,
                    BlockPricing = 2
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var deleteReturn = new Return<dynamic>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
            };

            var createReturn = new Return<PriceItem>
            {
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                Data = new PriceItem
                {
                    PriceTableId = Guid.NewGuid(),
                    MaxPrice = 20,
                    MinPrice = 10,
                    BlockPricing = 2
                },
                IsSuccess = true
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(priceTableReturn);

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceItemByPriceTableIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(defaultPriceItemReturn);

            _priceRepositoryMock
                .Setup(x => x.DeleteAllPriceItemByPriceTableIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(deleteReturn);

            _priceRepositoryMock
                .Setup(x => x.CreatePriceItemAsync(It.IsAny<PriceItem>()))
                .ReturnsAsync(createReturn);

            // Act
            var result = await _priceService.UpdatePriceItemAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePriceItemAsync_ShouldReturnsFailure_WhenAuthenticationFailed()
        {
            // Arrange
            var req = new CreateListPriceItemReqDto();

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _priceService.UpdatePriceItemAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePriceItemAsync_ShouldReturnsFailure_WhenPriceTableNotExist()
        {
            // Arrange
            var req = new CreateListPriceItemReqDto
            {
                PriceTableId = Guid.NewGuid(),
                PriceItems =
                [
                    new CreatePriceItemReqDto
                    {
                        From = 0,
                        To = 12,
                        MinPrice = 10,
                        MaxPrice = 20,
                        BlockPricing = 2
                    },
                ]
            };

            var priceTableResult = new Return<PriceTable> 
            { 
                Data = null, 
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = false
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock.Setup(x => x.GetPriceTableByIdAsync(It.IsAny<Guid>())).ReturnsAsync(priceTableResult);

            // Act
            var result = await _priceService.UpdatePriceItemAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PRICE_TABLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePriceItemAsync_ShouldReturnsFailure_WhenDefaultPriceItemNotExist()
        {
            // Arrange
            var req = new CreateListPriceItemReqDto
            {
                PriceTableId = Guid.NewGuid(),
                PriceItems =
                [
                    new CreatePriceItemReqDto
                    {
                        From = 0,
                        To = 12,
                        MinPrice = 10,
                        MaxPrice = 20,
                        BlockPricing = 2
                    },
                ]
            };

            var priceTableReturn = new Return<PriceTable>
            {
                Data = new PriceTable
                {
                    VehicleTypeId = Guid.NewGuid(),
                    Priority = 1,
                    Name = "Price Table 1",
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var defaultPriceItemReturn = new Return<PriceItem>
            {
                Data = null,
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };
            
            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(priceTableReturn);

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceItemByPriceTableIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(defaultPriceItemReturn);

            // Act
            var result = await _priceService.UpdatePriceItemAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.DEFAULT_PRICE_ITEM_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePriceItemAsync_ShouldReturnsFailure_WhenDeleteAllPriceItemsFailed()
        {
            // Arrange
            var req = new CreateListPriceItemReqDto
            {
                PriceTableId = Guid.NewGuid(),
                PriceItems =
                [
                    new CreatePriceItemReqDto
                    {
                        From = 0,
                        To = 12,
                        MinPrice = 10,
                        MaxPrice = 20,
                        BlockPricing = 2
                    },
                ]
            };

            var priceTableReturn = new Return<PriceTable>
            {
                Data = new PriceTable
                {
                    VehicleTypeId = Guid.NewGuid(),
                    Priority = 1,
                    Name = "Price Table 1",
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var defaultPriceItemReturn = new Return<PriceItem>
            {
                Data = new PriceItem
                {
                    MaxPrice = 30,
                    MinPrice = 5,
                    BlockPricing = 2
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var deleteReturn = new Return<dynamic>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            var createReturn = new Return<PriceItem>
            {
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                Data = new PriceItem
                {
                    PriceTableId = Guid.NewGuid(),
                    MaxPrice = 20,
                    MinPrice = 10,
                    BlockPricing = 2
                },
                IsSuccess = true
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(priceTableReturn);

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceItemByPriceTableIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(defaultPriceItemReturn);

            _priceRepositoryMock
                .Setup(x => x.DeleteAllPriceItemByPriceTableIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(deleteReturn);

            // Act
            var result = await _priceService.UpdatePriceItemAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePriceItemAsync_ShouldReturnsFailure_WhenCreatePriceItemFailed()
        {
            // Arrange
            var req = new CreateListPriceItemReqDto
            {
                PriceTableId = Guid.NewGuid(),
                PriceItems =
                [
                    new CreatePriceItemReqDto
                    {
                        From = 0,
                        To = 12,
                        MinPrice = 10,
                        MaxPrice = 20,
                        BlockPricing = 2
                    },
                ]
            };

            var priceTableReturn = new Return<PriceTable>
            {
                Data = new PriceTable
                {
                    VehicleTypeId = Guid.NewGuid(),
                    Priority = 1,
                    Name = "Price Table 1",
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var defaultPriceItemReturn = new Return<PriceItem>
            {
                Data = new PriceItem
                {
                    MaxPrice = 30,
                    MinPrice = 5,
                    BlockPricing = 2
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var deleteReturn = new Return<dynamic>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
            };

            var createReturn = new Return<PriceItem>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(priceTableReturn);

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceItemByPriceTableIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(defaultPriceItemReturn);

            _priceRepositoryMock
                .Setup(x => x.DeleteAllPriceItemByPriceTableIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(deleteReturn);

            _priceRepositoryMock
                .Setup(x => x.CreatePriceItemAsync(It.IsAny<PriceItem>()))
                .ReturnsAsync(createReturn);

            // Act
            var result = await _priceService.UpdatePriceItemAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CreateDefaultPriceTableAsync
        // Successful
        [Fact]
        public async Task CreateDefaultPriceTableAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var req = new CreateDefaultPriceTableReqDto { 
                VehicleTypeId = Guid.NewGuid() 
            };

            var vehicleTypeResult = new Return<VehicleType> 
            { 
                IsSuccess = true, 
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    Id = Guid.NewGuid(),
                    Name = "ELECTRIC_BICYCLE",
                    StatusVehicleType = StatusVehicleType.ACTIVE,
                }
            };

            var defaultPriceTableResult = new Return<PriceTable> 
            { 
                IsSuccess = true, 
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
            };

            var createPriceTableResult = new Return<PriceTable> 
            { 
                IsSuccess = true, 
                Data = new PriceTable
                {
                    Id = Guid.NewGuid(),
                    VehicleTypeId = req.VehicleTypeId,
                    Priority = 1,
                    Name = "Price Table 1",
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                },
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY 
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(vehicleTypeResult);

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceTableByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(defaultPriceTableResult);

            _priceRepositoryMock
                .Setup(x => x.CreatePriceTableAsync(It.IsAny<PriceTable>()))
                .ReturnsAsync(createPriceTableResult);

            // Act
            var result = await _priceService.CreateDefaultPriceTableAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateDefaultPriceTableAsync_ShouldReturnsFailure_WhenAuthenticationFailed()
        {
            // Arrange
            var req = new CreateDefaultPriceTableReqDto();

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _priceService.CreateDefaultPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateDefaultPriceTableAsync_ShouldReturnsFailure_WhenVehicleTypeNotExist()
        {
            // Arrange
            var req = new CreateDefaultPriceTableReqDto
            {
                VehicleTypeId = Guid.NewGuid()
            };

            var vehicleTypeResult = new Return<VehicleType> 
            { 
                IsSuccess = true, 
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT 
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(vehicleTypeResult);

            // Act
            var result = await _priceService.CreateDefaultPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateDefaultPriceTableAsync_ShouldReturnsFailure_WhenDefaultPriceTableAlreadyExists()
        {
            // Arrange
            var req = new CreateDefaultPriceTableReqDto
            {
                VehicleTypeId = Guid.NewGuid()
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    Id = Guid.NewGuid(),
                    Name = "ELECTRIC_BICYCLE",
                    StatusVehicleType = StatusVehicleType.ACTIVE,
                }
            };

            var defaultPriceTableResult = new Return<PriceTable>
            {
                IsSuccess = true,
                Data = new PriceTable
                {
                    Id = Guid.NewGuid(),
                    VehicleTypeId = req.VehicleTypeId,
                    Priority = 1,
                    Name = "Price Table 1",
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(vehicleTypeResult);

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceTableByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(defaultPriceTableResult);

            // Act
            var result = await _priceService.CreateDefaultPriceTableAsync(req);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.DEFAULT_PRICE_TABLE_IS_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateDefaultPriceTableAsync_ShouldReturnsFailure_CreatePriceTableFailed()
        {
            // Arrange
            var req = new CreateDefaultPriceTableReqDto
            {
                VehicleTypeId = Guid.NewGuid()
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    Id = Guid.NewGuid(),
                    Name = "ELECTRIC_BICYCLE",
                    StatusVehicleType = StatusVehicleType.ACTIVE,
                }
            };

            var defaultPriceTableResult = new Return<PriceTable>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
            };

            var createPriceTableResult = new Return<PriceTable>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(vehicleTypeResult);

            _priceRepositoryMock
                .Setup(x => x.GetDefaultPriceTableByVehicleTypeAsync(It.IsAny<Guid>()))
                .ReturnsAsync(defaultPriceTableResult);

            _priceRepositoryMock
                .Setup(x => x.CreatePriceTableAsync(It.IsAny<PriceTable>()))
                .ReturnsAsync(createPriceTableResult);

            // Act
            var result = await _priceService.CreateDefaultPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CreatePriceTableAsync
        // Successful
        [Fact]
        public async Task CreatePriceTableAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var req = new CreatePriceTableReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                Priority = 1,
                Name = "Test Price Table",
                ApplyFromDate = DateTime.Now,
                ApplyToDate = DateTime.Now.AddDays(30),
                MaxPrice = 100,
                MinPrice = 50,
                PricePerBlock = 2,
            };

            var vehicleTypeResult = new Return<VehicleType> 
            { 
                Data = new VehicleType 
                { 
                    Id = req.VehicleTypeId,
                    Name = "ELECTRIC_BICYCLE",
                }, 
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true
            };

            var priorityResult = new Return<PriceTable> 
            { 
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT 
            };

            var createPriceTableResult = new Return<PriceTable> 
            { 
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, 
                Data = new PriceTable 
                { 
                    Priority = 1,
                    Name = "Price Table",
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                } 
            };

            var createPriceItemResult = new Return<PriceItem> 
            { 
                IsSuccess = true,
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY 
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>())).ReturnsAsync(vehicleTypeResult);

            _priceRepositoryMock.Setup(x => x.GetPriceTableByPriorityAndVehicleTypeAsync(It.IsAny<int>(), It.IsAny<Guid>())).ReturnsAsync(priorityResult);

            _priceRepositoryMock.Setup(x => x.CreatePriceTableAsync(It.IsAny<PriceTable>())).ReturnsAsync(createPriceTableResult);

            _priceRepositoryMock.Setup(x => x.CreatePriceItemAsync(It.IsAny<PriceItem>())).ReturnsAsync(createPriceItemResult);

            // Act
            var result = await _priceService.CreatePriceTableAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreatePriceTableAsync_ShouldReturnsFailure_WhenAuthenticationFailed()
        {
            // Arrange
            var req = new CreatePriceTableReqDto();

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _priceService.CreatePriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreatePriceTableAsync_ShouldReturnsFailure_WhenVehicleTypeNotExist()
        {
            // Arrange
            var req = new CreatePriceTableReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                Priority = 1,
                Name = "Test Price Table",
                ApplyFromDate = DateTime.Now,
                ApplyToDate = DateTime.Now.AddDays(30),
                MaxPrice = 100,
                MinPrice = 50,
                PricePerBlock = 2,
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = false
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>())).ReturnsAsync(vehicleTypeResult);

            // Act
            var result = await _priceService.CreatePriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreatePriceTableAsync_ShouldReturnsFailure_WhenPriorityExists()
        {
            // Arrange
            var req = new CreatePriceTableReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                Priority = 1,
                Name = "Test Price Table",
                ApplyFromDate = DateTime.Now,
                ApplyToDate = DateTime.Now.AddDays(30),
                MaxPrice = 100,
                MinPrice = 50,
                PricePerBlock = 2,
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                Data = new VehicleType
                {
                    Id = req.VehicleTypeId,
                    Name = "ELECTRIC_BICYCLE",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true
            };

            var priorityResult = new Return<PriceTable>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>())).ReturnsAsync(vehicleTypeResult);

            _priceRepositoryMock.Setup(x => x.GetPriceTableByPriorityAndVehicleTypeAsync(It.IsAny<int>(), It.IsAny<Guid>())).ReturnsAsync(priorityResult);

            // Act
            var result = await _priceService.CreatePriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PRIORITY_IS_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreatePriceTableAsync_ShouldReturnsFailure_WhenCreatePriceTableFailed()
        {
            // Arrange
            var req = new CreatePriceTableReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                Priority = 1,
                Name = "Test Price Table",
                ApplyFromDate = DateTime.Now,
                ApplyToDate = DateTime.Now.AddDays(30),
                MaxPrice = 100,
                MinPrice = 50,
                PricePerBlock = 2,
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                Data = new VehicleType
                {
                    Id = req.VehicleTypeId,
                    Name = "ELECTRIC_BICYCLE",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true
            };

            var priorityResult = new Return<PriceTable>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var createPriceTableResult = new Return<PriceTable>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>())).ReturnsAsync(vehicleTypeResult);

            _priceRepositoryMock.Setup(x => x.GetPriceTableByPriorityAndVehicleTypeAsync(It.IsAny<int>(), It.IsAny<Guid>())).ReturnsAsync(priorityResult);

            _priceRepositoryMock.Setup(x => x.CreatePriceTableAsync(It.IsAny<PriceTable>())).ReturnsAsync(createPriceTableResult);

            // Act
            var result = await _priceService.CreatePriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreatePriceTableAsync_ShouldReturnsFailure_WhenCreatePriceItemFailed()
        {
            // Arrange
            var req = new CreatePriceTableReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                Priority = 1,
                Name = "Test Price Table",
                ApplyFromDate = DateTime.Now,
                ApplyToDate = DateTime.Now.AddDays(30),
                MaxPrice = 100,
                MinPrice = 50,
                PricePerBlock = 2,
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                Data = new VehicleType
                {
                    Id = req.VehicleTypeId,
                    Name = "ELECTRIC_BICYCLE",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true
            };

            var priorityResult = new Return<PriceTable>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var createPriceTableResult = new Return<PriceTable>
            {
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                Data = new PriceTable
                {
                    Priority = 1,
                    Name = "Price Table",
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                }
            };

            var createPriceItemResult = new Return<PriceItem>
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>())).ReturnsAsync(vehicleTypeResult);

            _priceRepositoryMock.Setup(x => x.GetPriceTableByPriorityAndVehicleTypeAsync(It.IsAny<int>(), It.IsAny<Guid>())).ReturnsAsync(priorityResult);

            _priceRepositoryMock.Setup(x => x.CreatePriceTableAsync(It.IsAny<PriceTable>())).ReturnsAsync(createPriceTableResult);

            _priceRepositoryMock.Setup(x => x.CreatePriceItemAsync(It.IsAny<PriceItem>())).ReturnsAsync(createPriceItemResult);

            // Act
            var result = await _priceService.CreatePriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetAllPriceTableAsync
        // Successful
        [Fact]
        public async Task GetAllPriceTableAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var priceTableList = new List<PriceTable>
            {
                new() {
                    Name = "Price Table 1",
                    Priority = 1,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                }
            };

            var listReturn = new Return<IEnumerable<PriceTable>>
            {
                IsSuccess = true,
                Data = priceTableList,
                Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
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

            _priceRepositoryMock.Setup(x => x.GetAllPriceTableAsync(req))
                .ReturnsAsync(listReturn);

            // Act
            var result = await _priceService.GetAllPriceTableAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllPriceTableAsync_ShouldReturnsFailure_WhenAuthenticationFails()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _priceService.GetAllPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllPriceTableAsync_ShouldReturnsFailure_WhenServerError()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var listReturn = new Return<IEnumerable<PriceTable>>
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

            _priceRepositoryMock.Setup(x => x.GetAllPriceTableAsync(req))
                .ReturnsAsync(listReturn);

            // Act
            var result = await _priceService.GetAllPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdateStatusPriceTableAsync
        // Successful
        [Fact]
        public async Task UpdateStatusPriceTableAsync_ShouldReturnsSuccess_WhenInActiveToActive()
        {
            // Arrange
            var isActive = true;

            var req = new ChangeStatusPriceTableReqDto
            {
                PriceTableId = Guid.NewGuid(),
                IsActive = isActive
            };

            var priceTable = new PriceTable
            {
                Priority = 2,
                StatusPriceTable = StatusPriceTableEnum.INACTIVE,
                Name = "Price Table",
            };

            var priceTableReturn = new Return<PriceTable>
            {
                IsSuccess = true,
                Data = priceTable,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(priceTableReturn);

            _priceRepositoryMock
                .Setup(x => x.UpdatePriceTableAsync(It.IsAny<PriceTable>()))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                    Data = new PriceTable
                    {
                        Id = req.PriceTableId,
                        Name = "Update Name",
                        Priority = 1,
                        StatusPriceTable = isActive ? StatusPriceTableEnum.ACTIVE : StatusPriceTableEnum.INACTIVE
                    }
                });
            
            // Act
            var result = await _priceService.UpdateStatusPriceTableAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successful
        [Fact]
        public async Task UpdateStatusPriceTableAsync_ShouldReturnsSuccess_WhenActiveToInActive()
        {
            // Arrange
            var isActive = false;

            var req = new ChangeStatusPriceTableReqDto
            {
                PriceTableId = Guid.NewGuid(),
                IsActive = isActive
            };

            var priceTable = new PriceTable
            {
                Priority = 2,
                StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                Name = "Price Table",
            };

            var priceTableReturn = new Return<PriceTable>
            {
                IsSuccess = true,
                Data = priceTable,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(priceTableReturn);

            _priceRepositoryMock
                .Setup(x => x.UpdatePriceTableAsync(It.IsAny<PriceTable>()))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                    Data = new PriceTable
                    {
                        Id = req.PriceTableId,
                        Name = "Update Name",
                        Priority = 1,
                        StatusPriceTable = isActive ? StatusPriceTableEnum.ACTIVE : StatusPriceTableEnum.INACTIVE
                    }
                });

            // Act
            var result = await _priceService.UpdateStatusPriceTableAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusPriceTableAsync_ShouldReturnsFailure_WhenAuthenticationFails()
        {
            // Arrange
            var req = new ChangeStatusPriceTableReqDto();

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _priceService.UpdateStatusPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusPriceTableAsync_ShouldReturnsFailure_WhenPriceTableNotExist()
        {
            // Arrange
            var isActive = true;

            var req = new ChangeStatusPriceTableReqDto
            {
                PriceTableId = Guid.NewGuid(),
                IsActive = isActive
            };

            var priceTableReturn = new Return<PriceTable>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(priceTableReturn);

            // Act
            var result = await _priceService.UpdateStatusPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PRICE_TABLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusPriceTableAsync_ShouldReturnsFailure_WhenIsDefaultPriceTable()
        {
            // Arrange
            var isActive = true;

            var req = new ChangeStatusPriceTableReqDto
            {
                PriceTableId = Guid.NewGuid(),
                IsActive = isActive
            };

            var priceTable = new PriceTable
            {
                Priority = 1,
                StatusPriceTable = StatusPriceTableEnum.INACTIVE,
                Name = "Price Table",
            };

            var priceTableReturn = new Return<PriceTable>
            {
                IsSuccess = true,
                Data = priceTable,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(priceTableReturn);

            // Act
            var result = await _priceService.UpdateStatusPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CAN_NOT_UPDATE_STATUS_DEFAULT_PRICE_TABLE, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusPriceTableAsync_ShouldReturnsFailure_WhenActivateAlreadyActiveTable()
        {
            // Arrange
            var isActive = true;

            var req = new ChangeStatusPriceTableReqDto
            {
                PriceTableId = Guid.NewGuid(),
                IsActive = isActive
            };

            var priceTable = new PriceTable
            {
                Priority = 2,
                StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                Name = "Price Table",
            };

            var priceTableReturn = new Return<PriceTable>
            {
                IsSuccess = true,
                Data = priceTable,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(priceTableReturn);

            _priceRepositoryMock
                .Setup(x => x.UpdatePriceTableAsync(It.IsAny<PriceTable>()))
                .ReturnsAsync(new Return<PriceTable>
                {
                    Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY,
                });

            // Act
            var result = await _priceService.UpdateStatusPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.STATUS_IS_ALREADY_APPLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusPriceTableAsync_ShouldReturnsFailure_WhenDeactivateAlreadyInactiveTable()
        {
            // Arrange
            var isActive = false;

            var req = new ChangeStatusPriceTableReqDto
            {
                PriceTableId = Guid.NewGuid(),
                IsActive = isActive
            };

            var priceTable = new PriceTable
            {
                Priority = 2,
                StatusPriceTable = StatusPriceTableEnum.INACTIVE,
                Name = "Price Table",
            };

            var priceTableReturn = new Return<PriceTable>
            {
                IsSuccess = true,
                Data = priceTable,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(priceTableReturn);

            _priceRepositoryMock
                .Setup(x => x.UpdatePriceTableAsync(It.IsAny<PriceTable>()))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY,
                });

            // Act
            var result = await _priceService.UpdateStatusPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.STATUS_IS_ALREADY_APPLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusPriceTableAsync_ShouldReturnsFailure_WhenUpdateFails()
        {
            // Arrange
            var isActive = true;

            var req = new ChangeStatusPriceTableReqDto
            {
                PriceTableId = Guid.NewGuid(),
                IsActive = isActive
            };

            var priceTable = new PriceTable
            {
                Priority = 2,
                StatusPriceTable = StatusPriceTableEnum.INACTIVE,
                Name = "Price Table",
            };

            var priceTableReturn = new Return<PriceTable>
            {
                IsSuccess = true,
                Data = priceTable,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock
                .Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(priceTableReturn);

            _priceRepositoryMock
                .Setup(x => x.UpdatePriceTableAsync(It.IsAny<PriceTable>()))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR,
                });

            // Act
            var result = await _priceService.UpdateStatusPriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.UPDATE_OBJECT_ERROR, result.Message);
        }

        // DeletePriceTableAsync
        // Successful
        [Fact]
        public async Task DeletePriceTableAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var priceTableId = Guid.NewGuid();

            var priceTableReturn = new Return<PriceTable>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new PriceTable
                {
                    Id = priceTableId,
                    Name = "Price Table 1",
                    Priority = 2,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                }
            };

            var updateReturn = new Return<PriceTable>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                IsSuccess = true,
                Data = new PriceTable
                {
                    Id = priceTableId,
                    Name = "Price Table 1",
                    Priority = 2,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    DeletedDate = DateTime.Now
                }
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock.Setup(x => x.GetPriceTableByIdAsync(priceTableId))
                .ReturnsAsync(priceTableReturn);

            _priceRepositoryMock.Setup(x => x.UpdatePriceTableAsync(It.IsAny<PriceTable>()))
                .ReturnsAsync(updateReturn);

            // Act
            var result = await _priceService.DeletePriceTableAsync(priceTableId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeletePriceTableAsync_ShouldReturnsFailure_WhenAuthenticationFails()
        {
            // Arrange
            var priceTableId = Guid.NewGuid();

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _priceService.DeletePriceTableAsync(priceTableId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeletePriceTableAsync_ShouldReturnsFailure_WhenUpdateFails()
        {
            // Arrange
            var priceTableId = Guid.NewGuid();

            var priceTableReturn = new Return<PriceTable>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new PriceTable
                {
                    Id = priceTableId,
                    Name = "Price Table 1",
                    Priority = 2,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                }
            };

            var updateReturn = new Return<PriceTable>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false,
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock.Setup(x => x.GetPriceTableByIdAsync(priceTableId))
                .ReturnsAsync(priceTableReturn);

            _priceRepositoryMock.Setup(x => x.UpdatePriceTableAsync(It.IsAny<PriceTable>()))
                .ReturnsAsync(updateReturn);

            // Act
            var result = await _priceService.DeletePriceTableAsync(priceTableId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeletePriceTableAsync_ShouldReturnsFailure_WhenPriceTableNotExis()
        {
            // Arrange
            var priceTableId = Guid.NewGuid();

            var priceTableReturn = new Return<PriceTable>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = false,
                Data = null
            };

            var updateReturn = new Return<PriceTable>
            {
                Message = ErrorEnumApplication.PRICE_TABLE_NOT_EXIST,
                IsSuccess = false,
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock.Setup(x => x.GetPriceTableByIdAsync(priceTableId))
                .ReturnsAsync(priceTableReturn);

            _priceRepositoryMock.Setup(x => x.UpdatePriceTableAsync(It.IsAny<PriceTable>()))
                .ReturnsAsync(updateReturn);

            // Act
            var result = await _priceService.DeletePriceTableAsync(priceTableId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PRICE_TABLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeletePriceTableAsync_ShouldReturnsFailure_WhenDefaultPriceTable()
        {
            // Arrange
            var priceTableId = Guid.NewGuid();

            var priceTableReturn = new Return<PriceTable>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new PriceTable
                {
                    Id = priceTableId,
                    Name = "Price Table 1",
                    Priority = 1,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                }
            };

            var updateReturn = new Return<PriceTable>
            {
                Message = ErrorEnumApplication.CAN_NOT_DELETE_DEFAULT_PRICE_TABLE,
                IsSuccess = false,
            };

            _helpperServiceMock
                .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock.Setup(x => x.GetPriceTableByIdAsync(priceTableId))
                .ReturnsAsync(priceTableReturn);

            _priceRepositoryMock.Setup(x => x.UpdatePriceTableAsync(It.IsAny<PriceTable>()))
                .ReturnsAsync(updateReturn);

            // Act
            var result = await _priceService.DeletePriceTableAsync(priceTableId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CAN_NOT_DELETE_DEFAULT_PRICE_TABLE, result.Message);
        }

        // UpdatePriceTableAsync
        // Successful
        [Fact]
        public async Task UpdatePriceTableAsync_ShouldReturnSuccess_WhenPriceTableUpdatedSuccessfully()
        {
            // Arrange
            var req = new UpdatePriceTableReqDto
            {
                PriceTableId = Guid.NewGuid(),
                Name = "Updated Price Table",
                ApplyFromDate = DateTime.Now,
                ApplyToDate = DateTime.Now.AddYears(1)
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock.Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new PriceTable 
                    { 
                        Id = req.PriceTableId,
                        Priority = 2,
                        Name = "Price Table",
                        StatusPriceTable = StatusPriceTableEnum.ACTIVE
                    }
                });

            _priceRepositoryMock.Setup(x => x.UpdatePriceTableAsync(It.IsAny<PriceTable>()))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            // Act
            var result = await _priceService.UpdatePriceTableAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePriceTableAsync_ShouldReturnFailure_WhenPriceTableUpdateFails()
        {
            // Arrange
            var req = new UpdatePriceTableReqDto
            {
                PriceTableId = Guid.NewGuid(),
                Name = "Updated Price Table",
                ApplyFromDate = DateTime.Now,
                ApplyToDate = DateTime.Now.AddYears(1)
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock.Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new PriceTable
                    {
                        Id = req.PriceTableId,
                        Priority = 2,
                        Name = "Price Table",
                        StatusPriceTable = StatusPriceTableEnum.ACTIVE
                    }
                });

            _priceRepositoryMock.Setup(x => x.UpdatePriceTableAsync(It.IsAny<PriceTable>()))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _priceService.UpdatePriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePriceTableAsync_ShouldReturnFailure_WhenTryingToUpdateDefaultPriceTable()
        {
            // Arrange
            var req = new UpdatePriceTableReqDto { PriceTableId = Guid.NewGuid(), Name = "New Price Table" };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock.Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = true,
                    Data = new PriceTable 
                    { 
                        Id = req.PriceTableId, Priority = 1,
                        Name = "Price Table",
                        StatusPriceTable = StatusPriceTableEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _priceService.UpdatePriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CAN_NOT_UPDATE_DEFAULT_PRICE_TABLE, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePriceTableAsync_ShouldReturnFailure_WhenPriceTableDoesNotExist()
        {
            // Arrange
            var req = new UpdatePriceTableReqDto { PriceTableId = Guid.NewGuid(), Name = "New Price Table" };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _priceRepositoryMock.Setup(x => x.GetPriceTableByIdAsync(req.PriceTableId))
                .ReturnsAsync(new Return<PriceTable>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _priceService.UpdatePriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PRICE_TABLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePriceTableAsync_ShouldReturnFailure_WhenAuthorizationFails()
        {
            // Arrange
            var req = new UpdatePriceTableReqDto { PriceTableId = Guid.NewGuid(), Name = "New Price Table" };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                });

            // Act
            var result = await _priceService.UpdatePriceTableAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // GetAllPriceTableByVehicleTypeAsync
        // Successful
        [Fact]
        public async Task GetAllPriceTableByVehicleTypeAsync_ShouldReturnSuccess_WhenPriceTablesFound()
        {
            // Arrange
            var vehicleTypeId = Guid.NewGuid();
            var priceTables = new List<PriceTable>
            {
                new PriceTable
                {
                    Name = "Price Table 1",
                    ApplyFromDate = DateTime.Now,
                    ApplyToDate = DateTime.Now.AddYears(1),
                    Priority = 2,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE,
                    VehicleType = new VehicleType { Id = vehicleTypeId, Name = "Car" }
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(vehicleTypeId))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType 
                    { 
                        Id = vehicleTypeId, 
                        Name = "Car" 
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceTableByVehicleTypeAsync(vehicleTypeId))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = priceTables,
                    TotalRecord = priceTables.Count,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _priceService.GetAllPriceTableByVehicleTypeAsync(vehicleTypeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetAllPriceTableByVehicleTypeAsync_ShouldReturnSuccess_WhenNoPriceTablesFound()
        {
            // Arrange
            var vehicleTypeId = Guid.NewGuid();

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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(vehicleTypeId))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType
                    {
                        Id = vehicleTypeId,
                        Name = "Car"
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceTableByVehicleTypeAsync(vehicleTypeId))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = true,
                    Data = null,
                    TotalRecord = 0,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _priceService.GetAllPriceTableByVehicleTypeAsync(vehicleTypeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllPriceTableByVehicleTypeAsync_ShouldReturnFailure_WhenPriceTableGetFails()
        {
            // Arrange
            var vehicleTypeId = Guid.NewGuid();

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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(vehicleTypeId))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Data = new VehicleType { Id = vehicleTypeId, Name = "Car" },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _priceRepositoryMock.Setup(x => x.GetAllPriceTableByVehicleTypeAsync(vehicleTypeId))
                .ReturnsAsync(new Return<IEnumerable<PriceTable>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _priceService.GetAllPriceTableByVehicleTypeAsync(vehicleTypeId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllPriceTableByVehicleTypeAsync_ShouldReturnFailure_WhenVehicleTypeDoesNotExist()
        {
            // Arrange
            var vehicleTypeId = Guid.NewGuid();

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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(vehicleTypeId))
                .ReturnsAsync(new Return<VehicleType>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                });

            // Act
            var result = await _priceService.GetAllPriceTableByVehicleTypeAsync(vehicleTypeId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllPriceTableByVehicleTypeAsync_ShouldReturnFailure_WhenAuthorizationFails()
        {
            // Arrange
            var vehicleTypeId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                });

            // Act
            var result = await _priceService.GetAllPriceTableByVehicleTypeAsync(vehicleTypeId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

    }
}
