using Castle.Core.Resource;
using FirebaseService;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Customer;
using FUParkingModel.RequestObject.Firebase;
using FUParkingModel.RequestObject.Vehicle;
using FUParkingModel.ResponseObject;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FUParkingTesting
{
    public class VehicleServiceTesting
    {
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock = new();
        private readonly Mock<IHelpperService> _helpperServiceMock = new();
        private readonly Mock<ISessionRepository> _sessionRepositoryMock = new();
        private readonly Mock<IMinioService> _minioServiceMock = new();
        private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
        private readonly Mock<IFirebaseService> _firebaseServiceMock = new();

        private readonly VehicleService _vehicleService;

        public VehicleServiceTesting()
        {
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();
            _helpperServiceMock = new Mock<IHelpperService>();
            _sessionRepositoryMock = new Mock<ISessionRepository>();
            _minioServiceMock = new Mock<IMinioService>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _firebaseServiceMock = new Mock<IFirebaseService>();

            _vehicleService = new VehicleService(_vehicleRepositoryMock.Object, _helpperServiceMock.Object, _sessionRepositoryMock.Object, _minioServiceMock.Object, _customerRepositoryMock.Object, _firebaseServiceMock.Object);
        }

        // GetVehicleTypesAsync
        // Successful
        [Fact]
        public async Task GetVehicleTypesAsync_ShouldReturnSuccess()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var vehicleTypes = new List<VehicleType>
            {
                new VehicleType {
                    Name = "Car",
                    StatusVehicleType = "Active",
                }
            };

            var vehicleResult = new Return<IEnumerable<VehicleType>>
            {
                IsSuccess = true,
                Data = vehicleTypes,
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

            _vehicleRepositoryMock.Setup(x => x.GetAllVehicleTypeAsync(req)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.GetVehicleTypesAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetVehicleTypesAsync_ShouldReturnFailure_WhenUserNotAuthorized()
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
            var result = await _vehicleService.GetVehicleTypesAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetVehicleTypesAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var vehicleTypes = new List<VehicleType>
            {
                new VehicleType {
                    Name = "Car",
                    StatusVehicleType = "Active",
                }
            };

            var vehicleResult = new Return<IEnumerable<VehicleType>>
            {
                IsSuccess = false,
                Data = null,
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

            _vehicleRepositoryMock.Setup(x => x.GetAllVehicleTypeAsync(req)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.GetVehicleTypesAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetListVehicleTypeByCustomer
        // Successful
        [Fact]
        public async Task GetListVehicleTypeByCustomer_ShouldReturnSuccess()
        {
            // Arrange
            var vehicleTypes = new List<VehicleType>
            {
                new VehicleType
                {
                    Name = "Car",
                    Description = "Four-wheeled vehicle"
                }
            };

            var vehicleResult = new Return<IEnumerable<VehicleType>>
            {
                IsSuccess = true,
                Data = vehicleTypes,
                TotalRecord = 1,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _vehicleRepositoryMock.Setup(x => x.GetAllVehicleTypeByCustomer()).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.GetListVehicleTypeByCustomer();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListVehicleTypeByCustomer_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var vehicleResult = new Return<IEnumerable<VehicleType>>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _vehicleRepositoryMock.Setup(x => x.GetAllVehicleTypeByCustomer()).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.GetListVehicleTypeByCustomer();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CreateVehicleTypeAsync
        // Successful
        [Fact]
        public async Task CreateVehicleTypeAsync_ShouldReturnSuccess()
        {
            // Arrange
            var reqDto = new CreateVehicleTypeReqDto
            {
                Name = "Car"
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var createResult = new Return<VehicleType>
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByName(reqDto.Name)).ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock.Setup(x => x.CreateVehicleTypeAsync(It.IsAny<VehicleType>())).ReturnsAsync(createResult);

            // Act
            var result = await _vehicleService.CreateVehicleTypeAsync(reqDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateVehicleTypeAsync_ShouldReturnFailure_WhenUserNotAuthorized()
        {
            // Arrange
            var reqDto = new CreateVehicleTypeReqDto
            {
                Name = "Car",
            };

            _helpperServiceMock
               .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
               .ReturnsAsync(new Return<User>
               {
                   IsSuccess = false,
                   Message = ErrorEnumApplication.NOT_AUTHENTICATION
               });

            // Act
            var result = await _vehicleService.CreateVehicleTypeAsync(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateVehicleTypeAsync_ShouldReturnFailure_WhenVehicleTypeExists()
        {
            // Arrange
            var reqDto = new CreateVehicleTypeReqDto
            {
                Name = "Car"
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    Name = "Car"
                }
            };

            var createResult = new Return<VehicleType>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.OBJECT_EXISTED
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByName(reqDto.Name)).ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock.Setup(x => x.CreateVehicleTypeAsync(It.IsAny<VehicleType>())).ReturnsAsync(createResult);

            // Act
            var result = await _vehicleService.CreateVehicleTypeAsync(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.OBJECT_EXISTED, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateVehicleTypeAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var reqDto = new CreateVehicleTypeReqDto
            {
                Name = "Car",
                Description = "Four-wheeled vehicle"
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var createResult = new Return<VehicleType>
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByName(reqDto.Name)).ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock.Setup(x => x.CreateVehicleTypeAsync(It.IsAny<VehicleType>())).ReturnsAsync(createResult);

            // Act
            var result = await _vehicleService.CreateVehicleTypeAsync(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdateVehicleTypeAsync
        // Successful
        [Fact]
        public async Task UpdateVehicleTypeAsync_ShouldReturnSuccess()
        {
            // Arrange
            var reqDto = new UpdateVehicleTypeReqDto
            {
                Name = "Update Car"
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    Name = "Car"
                },
                IsSuccess = true
            };

            var updateResult = new Return<VehicleType>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(reqDto.Id)).ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleTypeByName(reqDto.Name))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleTypeAsync(It.IsAny<VehicleType>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.UpdateVehicleTypeAsync(reqDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleTypeAsync_ShouldReturnFailure_WhenUserNotAuthorized()
        {
            // Arrange
            var reqDto = new UpdateVehicleTypeReqDto
            {
                Name = "Updated Car"
            };

            _helpperServiceMock
              .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
              .ReturnsAsync(new Return<User>
              {
                  IsSuccess = false,
                  Message = ErrorEnumApplication.NOT_AUTHENTICATION
              });

            // Act
            var result = await _vehicleService.UpdateVehicleTypeAsync(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleTypeAsync_ShouldReturnFailure_WhenVehicleTypeDoesNotExist()
        {
            // Arrange
            var reqDto = new UpdateVehicleTypeReqDto
            {
                Name = "Updated Car"
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST,
                Data = null
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(reqDto.Id)).ReturnsAsync(vehicleTypeResult);

            // Act
            var result = await _vehicleService.UpdateVehicleTypeAsync(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleTypeAsync_ShouldReturnFailure_WhenVehicleTypeNameExists()
        {
            // Arrange
            var reqDto = new UpdateVehicleTypeReqDto
            {
                Name = "Update Car"
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    Name = "Car"
                },
                IsSuccess = true
            };

            var updateResult = new Return<VehicleType>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.OBJECT_EXISTED
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
                .Setup(x => x.GetVehicleTypeByIdAsync(reqDto.Id))
                .ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleTypeByName(reqDto.Name))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new VehicleType
                    {
                        Name = reqDto.Name,
                    },
                    IsSuccess = true
                });

            _vehicleRepositoryMock
                .Setup(x => x.UpdateVehicleTypeAsync(It.IsAny<VehicleType>()))
                .ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.UpdateVehicleTypeAsync(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.OBJECT_EXISTED, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleTypeAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var reqDto = new UpdateVehicleTypeReqDto
            {
                Name = "Update Car"
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    Name = "Car"
                },
                IsSuccess = true
            };

            var updateResult = new Return<VehicleType>
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

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(reqDto.Id)).ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleTypeByName(reqDto.Name))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleTypeAsync(It.IsAny<VehicleType>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.UpdateVehicleTypeAsync(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetVehiclesAsync
        // Successful
        [Fact]
        public async Task GetVehiclesAsync_ShouldReturnSuccess()
        {
            // Arrange
            var req = new GetListObjectWithFillerAttributeAndDateReqDto();

            var vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    PlateNumber = "99L999999",
                    VehicleType = new VehicleType
                    {
                        Name = "Car"
                    },
                    StatusVehicle = StatusVehicleEnum.ACTIVE
                }
            };

            var vehicleResult = new Return<IEnumerable<Vehicle>>
            {
                IsSuccess = true,
                Data = vehicles,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
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

            _vehicleRepositoryMock.Setup(x => x.GetVehiclesAsync(req)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.GetVehiclesAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetVehiclesAsync_ShouldReturnFailure_WhenUserNotAuthorized()
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
            var result = await _vehicleService.GetVehiclesAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetVehiclesAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var req = new GetListObjectWithFillerAttributeAndDateReqDto();

            var vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    PlateNumber = "99L999999",
                    VehicleType = new VehicleType
                    {
                        Name = "Car"
                    },
                    StatusVehicle = StatusVehicleEnum.ACTIVE
                }
            };

            var vehicleResult = new Return<IEnumerable<Vehicle>>
            {
                IsSuccess = false,
                Data = null,
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

            _vehicleRepositoryMock.Setup(x => x.GetVehiclesAsync(req)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.GetVehiclesAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetListCustomerVehicleByCustomerIdAsync
        // Successful
        [Fact]
        public async Task GetListCustomerVehicleByCustomerIdAsync_ShouldReturnSuccess()
        {
            // Arrange
            var req = new GetListObjectWithFillerAttributeAndDateReqDto();

            var vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    PlateNumber = "99L999999",
                    VehicleType = new VehicleType
                    {
                        Name = "Car"
                    },
                    StatusVehicle = StatusVehicleEnum.ACTIVE
                }
            };
            var vehicleResult = new Return<IEnumerable<Vehicle>>
            {
                IsSuccess = true,
                Data = vehicles,
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                TotalRecord = 1
            };

            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                Id = customerId,
                Email = "customer@gmail.com",
                FullName = "customer",
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = customer,
                    IsSuccess = true
                });

            _vehicleRepositoryMock.Setup(x => x.GetAllCustomerVehicleByCustomerIdAsync(customerId)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.GetListCustomerVehicleByCustomerIdAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListCustomerVehicleByCustomerIdAsync_ShouldReturnFailure_WhenUserNotAuthorized()
        {
            // Arrange
            _helpperServiceMock
               .Setup(x => x.ValidateCustomerAsync())
               .ReturnsAsync(new Return<Customer>
               {
                   IsSuccess = false,
                   Message = ErrorEnumApplication.NOT_AUTHENTICATION
               });

            // Act
            var result = await _vehicleService.GetListCustomerVehicleByCustomerIdAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListCustomerVehicleByCustomerIdAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var req = new GetListObjectWithFillerAttributeAndDateReqDto();

            var vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    PlateNumber = "99L999999",
                    VehicleType = new VehicleType
                    {
                        Name = "Car"
                    },
                    StatusVehicle = StatusVehicleEnum.ACTIVE
                }
            };
            var vehicleResult = new Return<IEnumerable<Vehicle>>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                Id = customerId,
                Email = "customer@gmail.com",
                FullName = "customer",
                StatusCustomer = StatusCustomerEnum.ACTIVE
            };

            _helpperServiceMock
                .Setup(x => x.ValidateCustomerAsync())
                .ReturnsAsync(new Return<Customer>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = customer,
                    IsSuccess = true
                });

            _vehicleRepositoryMock.Setup(x => x.GetAllCustomerVehicleByCustomerIdAsync(customerId)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.GetListCustomerVehicleByCustomerIdAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // ChangeStatusVehicleTypeAsync
        // Successful
        [Fact]
        public async Task ChangeStatusVehicleTypeAsync_ShouldReturnSuccess_WhenStatusIsChangedToActive()
        {
            // Arrange
            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    StatusVehicleType = StatusVehicleType.INACTIVE,
                    Name = "Car"
                }
            };

            var updateResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            var id = Guid.NewGuid();
            _helpperServiceMock
               .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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
                       PasswordSalt = ""
                   }
               });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(id)).ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleTypeAsync(It.IsAny<VehicleType>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.ChangeStatusVehicleTypeAsync(id, true);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successful
        [Fact]
        public async Task ChangeStatusVehicleTypeAsync_ShouldReturnSuccess_WhenStatusIsChangedToInActive()
        {
            // Arrange
            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    StatusVehicleType = StatusVehicleType.ACTIVE,
                    Name = "Car"
                }
            };

            var updateResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            var id = Guid.NewGuid();
            _helpperServiceMock
               .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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
                       PasswordSalt = ""
                   }
               });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(id)).ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleTypeAsync(It.IsAny<VehicleType>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.ChangeStatusVehicleTypeAsync(id, false);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeStatusVehicleTypeAsync_ShouldReturnFailure_WhenUserNotAuthorized()
        {
            // Arrange
            var id = Guid.NewGuid();

            _helpperServiceMock
               .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
               .ReturnsAsync(new Return<User>
               {
                   IsSuccess = false,
                   Message = ErrorEnumApplication.SERVER_ERROR
               });

            // Act
            var result = await _vehicleService.ChangeStatusVehicleTypeAsync(id, true);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeStatusVehicleTypeAsync_ShouldReturnFailure_WhenVehicleTypeDoesNotExist()
        {
            // Arrange
            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                Data = null
            };

            var id = Guid.NewGuid();

            _helpperServiceMock
               .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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
                       PasswordSalt = ""
                   }
               });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(id)).ReturnsAsync(vehicleTypeResult);

            // Act
            var result = await _vehicleService.ChangeStatusVehicleTypeAsync(id, true);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeStatusVehicleTypeAsync_ShouldReturnFailure_WhenStatusIsAlreadyApplied()
        {
            // Arrange
            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    StatusVehicleType = StatusVehicleType.ACTIVE,
                    Name = "Car"
                }
            };

            var updateResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            var id = Guid.NewGuid();
            _helpperServiceMock
               .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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
                       PasswordSalt = ""
                   }
               });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(id)).ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleTypeAsync(It.IsAny<VehicleType>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.ChangeStatusVehicleTypeAsync(id, true);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.STATUS_IS_ALREADY_APPLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeStatusVehicleTypeAsync_ShouldReturnFailue_WhenServerError()
        {
            // Arrange
            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    StatusVehicleType = StatusVehicleType.INACTIVE,
                    Name = "Car"
                }
            };

            var updateResult = new Return<VehicleType>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false
            };

            var id = Guid.NewGuid();
            _helpperServiceMock
               .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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
                       PasswordSalt = ""
                   }
               });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(id)).ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleTypeAsync(It.IsAny<VehicleType>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.ChangeStatusVehicleTypeAsync(id, true);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // DeleteVehicleTypeAsync
        // Successful
        [Fact]
        public async Task DeleteVehicleTypeAsync_ShouldReturnSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();

            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new VehicleType
                {
                    StatusVehicleType = StatusVehicleType.ACTIVE,
                    Name = "Car"
                }
            };

            var vehiclesResult = new Return<Vehicle>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var updateResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                IsSuccess = true,
                Data = new VehicleType
                {
                    Name = "Car",
                    DeletedDate = DateTime.Now,
                    StatusVehicleType = StatusVehicleType.INACTIVE,
                }
            };

            _helpperServiceMock
               .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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
                       PasswordSalt = ""
                   }
               });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(id)).ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock.Setup(x => x.GetNewestVehicleByVehicleTypeId(id)).ReturnsAsync(vehiclesResult);

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleTypeAsync(It.IsAny<VehicleType>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.DeleteVehicleTypeAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteVehicleTypeAsync_ShouldReturnFailure_WhenUserNotAuthorized()
        {
            // Arrange
            var id = Guid.NewGuid();

            _helpperServiceMock
               .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
               .ReturnsAsync(new Return<User>
               {
                   IsSuccess = false,
                   Message = ErrorEnumApplication.SERVER_ERROR
               });

            // Act
            var result = await _vehicleService.DeleteVehicleTypeAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteVehicleTypeAsync_ShouldReturnFailure_WhenVehicleTypeDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                IsSuccess = true,
                Data = null
            };

            var updateResult = new Return<VehicleType>
            {
                Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST,
                IsSuccess = false,
                Data = null
            };

            _helpperServiceMock
               .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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
                       PasswordSalt = ""
                   }
               });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(id)).ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleTypeAsync(It.IsAny<VehicleType>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.DeleteVehicleTypeAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteVehicleTypeAsync_ShouldReturnFailure_WhenVehicleTypeIsInUse()
        {
            // Arrange
            var id = Guid.NewGuid();

            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new VehicleType
                {
                    StatusVehicleType = StatusVehicleType.ACTIVE,
                    Name = "Car"
                }
            };

            var vehiclesResult = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true
            };

            var updateResult = new Return<VehicleType>
            {
                Message = ErrorEnumApplication.VEHICLE_TYPE_IS_IN_USE,
                IsSuccess = false,
                Data = null
            };

            _helpperServiceMock
               .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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
                       PasswordSalt = ""
                   }
               });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(id)).ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock.Setup(x => x.GetNewestVehicleByVehicleTypeId(id)).ReturnsAsync(vehiclesResult);

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleTypeAsync(It.IsAny<VehicleType>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.DeleteVehicleTypeAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_IS_IN_USE, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteVehicleTypeAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var id = Guid.NewGuid();

            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new VehicleType
                {
                    StatusVehicleType = StatusVehicleType.ACTIVE,
                    Name = "Car"
                }
            };

            var vehiclesResult = new Return<Vehicle>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var updateResult = new Return<VehicleType>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false,
                Data = null
            };

            _helpperServiceMock
               .Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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
                       PasswordSalt = ""
                   }
               });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(id)).ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock.Setup(x => x.GetNewestVehicleByVehicleTypeId(id)).ReturnsAsync(vehiclesResult);

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleTypeAsync(It.IsAny<VehicleType>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.DeleteVehicleTypeAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CreateCustomerVehicleAsync
        // Successful
        [Fact]
        public async Task CreateCustomerVehicleAsync_ShouldReturnSuccess()
        {
            // Arrange
            var reqDto = new CreateCustomerVehicleReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                PlateImage = new Mock<IFormFile>().Object
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = true,
                Data = new VehicleType
                {
                    Id = reqDto.VehicleTypeId,
                    Name = "Car"
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var vehicleResult = new Return<Vehicle>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var uploadResult = new Return<ReturnObjectUrlResDto>
            {
                Data = new ReturnObjectUrlResDto
                {
                    ObjUrl = "image_url"
                },
                Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY
            };

            var createResult = new Return<Vehicle>
            {
                IsSuccess = true,
                Data = new Vehicle
                {
                    PlateNumber = reqDto.PlateNumber,
                    VehicleType = new VehicleType
                    {
                        Name = "Car"
                    },
                    PlateImage = "image_url",
                    StatusVehicle = StatusVehicleEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    FCMToken = "token",
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleTypeByIdAsync(reqDto.VehicleTypeId))
                .ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleByPlateNumberAsync(reqDto.PlateNumber))
                .ReturnsAsync(vehicleResult);

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>())).ReturnsAsync(uploadResult);

            _vehicleRepositoryMock
                .Setup(x => x.CreateVehicleAsync(It.IsAny<Vehicle>()))
                .ReturnsAsync(createResult);

            _firebaseServiceMock
                .Setup(x => x.SendNotificationAsync(It.IsAny<FirebaseReqDto>()))
                .ReturnsAsync(new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                });

            // Act
            var result = await _vehicleService.CreateCustomerVehicleAsync(reqDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateCustomerVehicleAsync_ShouldReturnFailure_WhenUserNotAuthorized()
        {
            // Arrange
            var reqDto = new CreateCustomerVehicleReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                PlateNumber = "30A12345",
                PlateImage = new Mock<IFormFile>().Object
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                IsSuccess = false,
                Data = null
            });

            // Act
            var result = await _vehicleService.CreateCustomerVehicleAsync(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateCustomerVehicleAsync_ShouldReturnFailure_WhenVehicleTypeDoesNotExist()
        {
            // Arrange
            var reqDto = new CreateCustomerVehicleReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                PlateNumber = "30A12345",
                PlateImage = new Mock<IFormFile>().Object
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    FCMToken = "token",
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(reqDto.VehicleTypeId)).ReturnsAsync(vehicleTypeResult);

            // Act
            var result = await _vehicleService.CreateCustomerVehicleAsync(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateCustomerVehicleAsync_ShouldReturnFailure_WhenPlateNumberIsInvalid()
        {
            // Arrange
            var reqDto = new CreateCustomerVehicleReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                PlateNumber = "INVALID",
                PlateImage = new Mock<IFormFile>().Object
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = true,
                Data = new VehicleType
                {
                    Id = reqDto.VehicleTypeId,
                    Name = "Car"
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    FCMToken = "token",
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleTypeByIdAsync(reqDto.VehicleTypeId))
                .ReturnsAsync(vehicleTypeResult);

            // Act
            var result = await _vehicleService.CreateCustomerVehicleAsync(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_A_PLATE_NUMBER, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateCustomerVehicleAsync_ShouldReturnFailure_WhenPlateNumberExists()
        {
            // Arrange
            var reqDto = new CreateCustomerVehicleReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                PlateNumber = "30A12345",
                PlateImage = new Mock<IFormFile>().Object
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = true,
                Data = new VehicleType
                {
                    Id = reqDto.VehicleTypeId,
                    Name = "Car"
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var vehicleResult = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    FCMToken = "token",
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleTypeByIdAsync(reqDto.VehicleTypeId))
                .ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleByPlateNumberAsync(reqDto.PlateNumber))
                .ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.CreateCustomerVehicleAsync(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PLATE_NUMBER_IS_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateCustomerVehicleAsync_ShouldReturnFailure_WhenImageUploadFails()
        {
            // Arrange
            var reqDto = new CreateCustomerVehicleReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                PlateNumber = "30A12345",
                PlateImage = new Mock<IFormFile>().Object
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = true,
                Data = new VehicleType
                {
                    Id = reqDto.VehicleTypeId,
                    Name = "Car"
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var vehicleResult = new Return<Vehicle>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var uploadResult = new Return<ReturnObjectUrlResDto>
            {
                Data = null,
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    FCMToken = "token",
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleTypeByIdAsync(reqDto.VehicleTypeId))
                .ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleByPlateNumberAsync(reqDto.PlateNumber))
                .ReturnsAsync(vehicleResult);

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>())).ReturnsAsync(uploadResult);


            // Act
            var result = await _vehicleService.CreateCustomerVehicleAsync(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateCustomerVehicleAsync_ShouldReturnFailure_WhenCreateFail()
        {
            // Arrange
            var reqDto = new CreateCustomerVehicleReqDto
            {
                VehicleTypeId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                PlateImage = new Mock<IFormFile>().Object
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = true,
                Data = new VehicleType
                {
                    Id = reqDto.VehicleTypeId,
                    Name = "Car"
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var vehicleResult = new Return<Vehicle>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var uploadResult = new Return<ReturnObjectUrlResDto>
            {
                Data = new ReturnObjectUrlResDto
                {
                    ObjUrl = "image_url"
                },
                Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY
            };

            var createResult = new Return<Vehicle>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    FCMToken = "token",
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleTypeByIdAsync(reqDto.VehicleTypeId))
                .ReturnsAsync(vehicleTypeResult);

            _vehicleRepositoryMock
                .Setup(x => x.GetVehicleByPlateNumberAsync(reqDto.PlateNumber))
                .ReturnsAsync(vehicleResult);

            _minioServiceMock.Setup(x => x.UploadObjectAsync(It.IsAny<UploadObjectReqDto>())).ReturnsAsync(uploadResult);

            _vehicleRepositoryMock
                .Setup(x => x.CreateVehicleAsync(It.IsAny<Vehicle>()))
                .ReturnsAsync(createResult);

            // Act
            var result = await _vehicleService.CreateCustomerVehicleAsync(reqDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdateVehicleInformationAsync
        // Successful
        [Fact]
        public async Task UpdateVehicleInformationAsync_ShouldReturnSuccess()
        {
            // Arrange
            var req = new UpdateCustomerVehicleReqDto
            {
                PlateNumber = "99L999999",
                VehicleTypeId = Guid.NewGuid()
            };

            var cusId = Guid.NewGuid();


            var vehicleResult = new Return<Vehicle>
            {
                IsSuccess = true,
                Data = new Vehicle
                {
                    CustomerId = cusId,
                    StatusVehicle = StatusVehicleEnum.PENDING,
                    PlateNumber = "30A12345"
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = true,
                Data = new VehicleType
                {
                    Id = req.VehicleTypeId.Value,
                    Name = "Car",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var plateNumberReturn = new Return<Vehicle>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };


            var updateResult = new Return<Vehicle>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    Id = cusId,
                    FCMToken = "token",
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(req.VehicleId)).ReturnsAsync(vehicleResult);
            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId.Value)).ReturnsAsync(vehicleTypeResult);
            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber)).ReturnsAsync(plateNumberReturn);
            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleAsync(It.IsAny<Vehicle>())).ReturnsAsync(updateResult);
            _firebaseServiceMock
                .Setup(x => x.SendNotificationAsync(It.IsAny<FirebaseReqDto>()))
                .ReturnsAsync(new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                });

            // Act
            var result = await _vehicleService.UpdateVehicleInformationAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleInformationAsync_ShouldReturnFailure_WhenUserNotAuthorized()
        {
            // Arrange
            var req = new UpdateCustomerVehicleReqDto
            {
                VehicleId = Guid.NewGuid(),
                PlateNumber = "30A12345",
                VehicleTypeId = Guid.NewGuid()
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                IsSuccess = false,
                Data = null
            });

            // Act
            var result = await _vehicleService.UpdateVehicleInformationAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleInformationAsync_ShouldReturnFailure_WhenVehicleDoesNotExist()
        {
            // Arrange
            var req = new UpdateCustomerVehicleReqDto
            {
                VehicleId = Guid.NewGuid(),
                PlateNumber = "99L999999",
                VehicleTypeId = Guid.NewGuid()
            };

            var vehicleResult = new Return<Vehicle>
            {
                IsSuccess = true,
                Message = ErrorEnumApplication.VEHICLE_NOT_EXIST
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    FCMToken = "token",
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(req.VehicleId)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.UpdateVehicleInformationAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleInformationAsync_ShouldReturnFailure_WhenVehicleTypeDoesNotExist()
        {
            // Arrange
            var cusId = Guid.NewGuid();

            var req = new UpdateCustomerVehicleReqDto
            {
                PlateNumber = "99L999999",
                VehicleTypeId = Guid.NewGuid()
            };

            var vehicleResult = new Return<Vehicle>
            {
                IsSuccess = true,
                Data = new Vehicle
                {
                    CustomerId = cusId,
                    StatusVehicle = StatusVehicleEnum.PENDING,
                    PlateNumber = "30A12345"
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    Id = cusId,
                    FCMToken = "token",
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(req.VehicleId)).ReturnsAsync(vehicleResult);
            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId.Value)).ReturnsAsync(vehicleTypeResult);
            _firebaseServiceMock
                .Setup(x => x.SendNotificationAsync(It.IsAny<FirebaseReqDto>()))
                .ReturnsAsync(new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                });

            // Act
            var result = await _vehicleService.UpdateVehicleInformationAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleInformationAsync_ShouldReturnFailure_WhenPlateNumberIsInvalid()
        {
            // Arrange
            var req = new UpdateCustomerVehicleReqDto
            {
                PlateNumber = "INVALID",
                VehicleTypeId = Guid.NewGuid()
            };
            var cusId = Guid.NewGuid();


            var vehicleResult = new Return<Vehicle>
            {
                IsSuccess = true,
                Data = new Vehicle
                {
                    CustomerId = cusId,
                    StatusVehicle = StatusVehicleEnum.PENDING,
                    PlateNumber = "30A12345"
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = true,
                Data = new VehicleType
                {
                    Id = req.VehicleTypeId.Value,
                    Name = "Car",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    Id = cusId,
                    FCMToken = "token",
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(req.VehicleId)).ReturnsAsync(vehicleResult);
            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId.Value)).ReturnsAsync(vehicleTypeResult);
            _firebaseServiceMock
                .Setup(x => x.SendNotificationAsync(It.IsAny<FirebaseReqDto>()))
                .ReturnsAsync(new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                });

            // Act
            var result = await _vehicleService.UpdateVehicleInformationAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_A_PLATE_NUMBER, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleInformationAsync_ShouldReturnFailure_WhenPlateNumberExists()
        {
            // Arrange
            var req = new UpdateCustomerVehicleReqDto
            {
                PlateNumber = "30A12345",
                VehicleTypeId = Guid.NewGuid()
            };

            var cusId = Guid.NewGuid();


            var vehicleResult = new Return<Vehicle>
            {
                IsSuccess = true,
                Data = new Vehicle
                {
                    CustomerId = cusId,
                    StatusVehicle = StatusVehicleEnum.PENDING,
                    PlateNumber = "30A12345"
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = true,
                Data = new VehicleType
                {
                    Id = req.VehicleTypeId.Value,
                    Name = "Car",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var plateNumberReturn = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    Id = cusId,
                    FCMToken = "token",
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(req.VehicleId)).ReturnsAsync(vehicleResult);
            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId.Value)).ReturnsAsync(vehicleTypeResult);
            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber)).ReturnsAsync(plateNumberReturn);
            _firebaseServiceMock
                .Setup(x => x.SendNotificationAsync(It.IsAny<FirebaseReqDto>()))
                .ReturnsAsync(new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                });

            // Act
            var result = await _vehicleService.UpdateVehicleInformationAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PLATE_NUMBER_IS_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleInformationAsync_ShouldReturnFailure_WhenUpdateFail()
        {
            // Arrange
            var req = new UpdateCustomerVehicleReqDto
            {
                PlateNumber = "99L999999",
                VehicleTypeId = Guid.NewGuid()
            };

            var cusId = Guid.NewGuid();


            var vehicleResult = new Return<Vehicle>
            {
                IsSuccess = true,
                Data = new Vehicle
                {
                    CustomerId = cusId,
                    StatusVehicle = StatusVehicleEnum.PENDING,
                    PlateNumber = "30A12345"
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                IsSuccess = true,
                Data = new VehicleType
                {
                    Id = req.VehicleTypeId.Value,
                    Name = "Car",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var plateNumberReturn = new Return<Vehicle>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };


            var updateResult = new Return<Vehicle>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    Id = cusId,
                    FCMToken = "token",
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(req.VehicleId)).ReturnsAsync(vehicleResult);
            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleTypeId.Value)).ReturnsAsync(vehicleTypeResult);
            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber)).ReturnsAsync(plateNumberReturn);
            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleAsync(It.IsAny<Vehicle>())).ReturnsAsync(updateResult);
            _firebaseServiceMock
                .Setup(x => x.SendNotificationAsync(It.IsAny<FirebaseReqDto>()))
                .ReturnsAsync(new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                });

            // Act
            var result = await _vehicleService.UpdateVehicleInformationAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // DeleteVehicleByCustomerAsync
        // Successful
        [Fact]
        public async Task DeleteVehicleByCustomerAsync_ShouldReturnSuccess()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var vehicleResult = new Return<Vehicle> 
            { 
                IsSuccess = true, 
                Data = new Vehicle 
                { 
                    Id = vehicleId, 
                    PlateNumber = "99L999999",
                    StatusVehicle = StatusVehicleEnum.ACTIVE,
                    CustomerId = customerId
                }, 
                Message = SuccessfullyEnumServer.FOUND_OBJECT 
            };

            var sessionResult = new Return<Session> 
            { 
                IsSuccess = true, 
                Data = new Session 
                { 
                    Status = SessionEnum.CLOSED,
                    PlateNumber = "99L999999",
                    Block = 30,
                    ImageInUrl = "image_url",
                    Mode = "MODE1",
                    TimeIn = DateTime.Now,
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var updateResult = new Return<Vehicle> 
            { 
                IsSuccess = true, 
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                Data = new Vehicle
                {
                    PlateNumber = "99L999999",
                    StatusVehicle = StatusVehicleEnum.ACTIVE,
                    DeletedDate = DateTime.Now,
                    CustomerId = customerId
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    Id = customerId,
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(vehicleId)).ReturnsAsync(vehicleResult);

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(vehicleResult.Data.PlateNumber)).ReturnsAsync(sessionResult);
            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleAsync(It.IsAny<Vehicle>())).ReturnsAsync(updateResult);
            _firebaseServiceMock
                .Setup(x => x.SendNotificationAsync(It.IsAny<FirebaseReqDto>()))
                .ReturnsAsync(new Return<dynamic> { 
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                });

            // Act
            var result = await _vehicleService.DeleteVehicleByCustomerAsync(vehicleId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteVehicleByCustomerAsync_ShouldReturnFailure_WhenUserNotAuthorized()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                IsSuccess = false,
                Data = null
            });

            // Act
            var result = await _vehicleService.DeleteVehicleByCustomerAsync(vehicleId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteVehicleByCustomerAsync_ShouldReturnFailure_WhenVehicleDoesNotExist()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var vehicleResult = new Return<Vehicle>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    Id = customerId,
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(vehicleId)).ReturnsAsync(vehicleResult);

            _firebaseServiceMock
                .Setup(x => x.SendNotificationAsync(It.IsAny<FirebaseReqDto>()))
                .ReturnsAsync(new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                });

            // Act
            var result = await _vehicleService.DeleteVehicleByCustomerAsync(vehicleId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteVehicleByCustomerAsync_ShouldReturnFailure_WhenVehicleIsInSession()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var vehicleResult = new Return<Vehicle>
            {
                IsSuccess = true,
                Data = new Vehicle
                {
                    Id = vehicleId,
                    PlateNumber = "99L999999",
                    StatusVehicle = StatusVehicleEnum.ACTIVE,
                    CustomerId = customerId
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var sessionResult = new Return<Session>
            {
                IsSuccess = true,
                Data = new Session
                {
                    Status = SessionEnum.PARKED,
                    PlateNumber = "99L999999",
                    Block = 30,
                    ImageInUrl = "image_url",
                    Mode = "MODE1",
                    TimeIn = DateTime.Now,
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    Id = customerId,
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(vehicleId)).ReturnsAsync(vehicleResult);

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(vehicleResult.Data.PlateNumber)).ReturnsAsync(sessionResult);

            _firebaseServiceMock
                .Setup(x => x.SendNotificationAsync(It.IsAny<FirebaseReqDto>()))
                .ReturnsAsync(new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                });

            // Act
            var result = await _vehicleService.DeleteVehicleByCustomerAsync(vehicleId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_IS_IN_SESSION, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteVehicleByCustomerAsync_ShouldReturnFailure_WhenDeleteFail()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var vehicleResult = new Return<Vehicle>
            {
                IsSuccess = true,
                Data = new Vehicle
                {
                    Id = vehicleId,
                    PlateNumber = "99L999999",
                    StatusVehicle = StatusVehicleEnum.ACTIVE,
                    CustomerId = customerId
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var sessionResult = new Return<Session>
            {
                IsSuccess = true,
                Data = new Session
                {
                    Status = SessionEnum.CLOSED,
                    PlateNumber = "99L999999",
                    Block = 30,
                    ImageInUrl = "image_url",
                    Mode = "MODE1",
                    TimeIn = DateTime.Now,
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var updateResult = new Return<Vehicle>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR,
                Data = null
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    Id = customerId,
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(vehicleId)).ReturnsAsync(vehicleResult);

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(vehicleResult.Data.PlateNumber)).ReturnsAsync(sessionResult);
            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleAsync(It.IsAny<Vehicle>())).ReturnsAsync(updateResult);
            _firebaseServiceMock
                .Setup(x => x.SendNotificationAsync(It.IsAny<FirebaseReqDto>()))
                .ReturnsAsync(new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.SUCCESSFULLY
                });

            // Act
            var result = await _vehicleService.DeleteVehicleByCustomerAsync(vehicleId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // ChangeStatusVehicleByUserAsync
        // Successful
        [Fact]
        public async Task ChangeStatusVehicleByUserAsync_ShouldReturnSuccess_WhenStatusIsChangedToActive()
        {
            // Arrange
            var req = new UpdateNewCustomerVehicleByUseReqDto
            {
                PlateNumber = "99L999999",
                IsAccept = true,
                VehicleType = Guid.NewGuid()
            };

            var vehicleResult = new Return<Vehicle> 
            { 
                Message = SuccessfullyEnumServer.FOUND_OBJECT, 
                Data = new Vehicle 
                { 
                    PlateNumber = "99L999999", 
                    StatusVehicle = StatusVehicleEnum.PENDING 
                } 
            };
            
            var vehicleTypeResult = new Return<VehicleType> 
            { 
                Message = SuccessfullyEnumServer.FOUND_OBJECT, 
                Data = new VehicleType 
                { 
                    Id = req.VehicleType.Value,
                    Name = "Car"
                } 
            };
            
            var updateResult = new Return<Vehicle> 
            { 
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY 
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber)).ReturnsAsync(vehicleResult);
            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleType.Value)).ReturnsAsync(vehicleTypeResult);
            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleAsync(It.IsAny<Vehicle>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.ChangeStatusVehicleByUserAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeStatusVehicleByUserAsync_ShouldReturnFailure_WhenUserNotAuthorized()
        {
            // Arrange
            var req = new UpdateNewCustomerVehicleByUseReqDto
            {
                PlateNumber = "30A12345",
                IsAccept = true,
                VehicleType = Guid.NewGuid()
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(new Return<User>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                Data = null,
                IsSuccess = false
            });

            // Act
            var result = await _vehicleService.ChangeStatusVehicleByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeStatusVehicleByUserAsync_ShouldReturnFailure_WhenVehicleDoesNotExist()
        {
            // Arrange
            var req = new UpdateNewCustomerVehicleByUseReqDto
            {
                PlateNumber = "99L999999",
                IsAccept = true,
                VehicleType = Guid.NewGuid()
            };

            var vehicleResult = new Return<Vehicle>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                Data = null
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.ChangeStatusVehicleByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeStatusVehicleByUserAsync_ShouldReturnFailure_WhenVehicleIsAlreadyActive()
        {
            // Arrange
            var req = new UpdateNewCustomerVehicleByUseReqDto
            {
                PlateNumber = "99L999999",
                IsAccept = true,
                VehicleType = Guid.NewGuid()
            };

            var vehicleResult = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    PlateNumber = "99L999999",
                    StatusVehicle = StatusVehicleEnum.ACTIVE
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.ChangeStatusVehicleByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_IS_ACTIVE, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeStatusVehicleByUserAsync_ShouldReturnFailure_WhenVehicleTypeDoesNotExist()
        {
            // Arrange
            var req = new UpdateNewCustomerVehicleByUseReqDto
            {
                PlateNumber = "99L999999",
                IsAccept = true,
                VehicleType = Guid.NewGuid()
            };

            var vehicleResult = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    PlateNumber = "99L999999",
                    StatusVehicle = StatusVehicleEnum.PENDING
                }
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                Data = null
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber)).ReturnsAsync(vehicleResult);
            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleType.Value)).ReturnsAsync(vehicleTypeResult);
            
            // Act
            var result = await _vehicleService.ChangeStatusVehicleByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeStatusVehicleByUserAsync_ShouldReturnFailure_WhenChangeFail()
        {
            // Arrange
            var req = new UpdateNewCustomerVehicleByUseReqDto
            {
                PlateNumber = "30A12345",
                IsAccept = true,
                VehicleType = Guid.NewGuid()
            };

            var vehicleResult = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    PlateNumber = "99L999999",
                    StatusVehicle = StatusVehicleEnum.PENDING
                }
            };

            var vehicleTypeResult = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    Id = req.VehicleType.Value,
                    Name = "Car"
                }
            };

            var updateResult = new Return<Vehicle>
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber)).ReturnsAsync(vehicleResult);
            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(req.VehicleType.Value)).ReturnsAsync(vehicleTypeResult);
            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleAsync(It.IsAny<Vehicle>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.ChangeStatusVehicleByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync
        // Successful
        [Fact]
        public async Task UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync_ShouldReturnSuccess_WhenStatusIsChangedToActive()
        {
            // Arrange
            var req = new UpdateStatusInactiveAndActiveCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                IsActive = true
            };

            var vehicleResult = new Return<Vehicle> 
            { 
                Message = SuccessfullyEnumServer.FOUND_OBJECT, 
                Data = new Vehicle 
                { 
                    Id = req.VehicleId, 
                    StatusVehicle = StatusVehicleEnum.INACTIVE,
                    PlateNumber = "99L999999"
                } 
            };
            
            var sessionResult = new Return<Session> 
            { 
                IsSuccess = true, 
                Data = new Session 
                { 
                    PlateNumber = vehicleResult.Data.PlateNumber,
                    Block = 30,
                    ImageInUrl = "image_url",
                    Mode = "MODE1",
                    Status = SessionEnum.CLOSED,
                    TimeIn = DateTime.Now,
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };
            
            var updateResult = new Return<Vehicle> 
            { 
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY 
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(req.VehicleId)).ReturnsAsync(vehicleResult);
            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(vehicleResult.Data.PlateNumber)).ReturnsAsync(sessionResult);
            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleAsync(It.IsAny<Vehicle>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successful
        [Fact]
        public async Task UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync_ShouldReturnSuccess_WhenStatusIsChangedToInActive()
        {
            // Arrange
            var req = new UpdateStatusInactiveAndActiveCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                IsActive = false
            };

            var vehicleResult = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    Id = req.VehicleId,
                    StatusVehicle = StatusVehicleEnum.ACTIVE,
                    PlateNumber = "99L999999"
                }
            };

            var sessionResult = new Return<Session>
            {
                IsSuccess = true,
                Data = new Session
                {
                    PlateNumber = vehicleResult.Data.PlateNumber,
                    Block = 30,
                    ImageInUrl = "image_url",
                    Mode = "MODE1",
                    Status = SessionEnum.CLOSED,
                    TimeIn = DateTime.Now,
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var updateResult = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(req.VehicleId)).ReturnsAsync(vehicleResult);
            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(vehicleResult.Data.PlateNumber)).ReturnsAsync(sessionResult);
            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleAsync(It.IsAny<Vehicle>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync_ShouldReturnFailure_WhenUserNotAuthorized()
        {
            // Arrange
            var req = new UpdateStatusInactiveAndActiveCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                IsActive = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                Data = null,
                IsSuccess = false
            });

            // Act
            var result = await _vehicleService.UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync_ShouldReturnFailure_WhenVehicleDoesNotExist()
        {
            // Arrange
            var req = new UpdateStatusInactiveAndActiveCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                IsActive = true
            };

            var vehicleResult = new Return<Vehicle>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                Data = null
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(req.VehicleId)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync_ShouldReturnFailure_WhenVehicleIsInSession()
        {
            // Arrange
            var req = new UpdateStatusInactiveAndActiveCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                IsActive = true
            };

            var vehicleResult = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    Id = req.VehicleId,
                    StatusVehicle = StatusVehicleEnum.INACTIVE,
                    PlateNumber = "99L999999"
                }
            };

            var sessionResult = new Return<Session>
            {
                IsSuccess = true,
                Data = new Session
                {
                    PlateNumber = vehicleResult.Data.PlateNumber,
                    Block = 30,
                    ImageInUrl = "image_url",
                    Mode = "MODE1",
                    Status = SessionEnum.CLOSED,
                    TimeIn = DateTime.Now,
                    GateOut = new Gate
                    {
                        Name = "Gate 1",
                        StatusGate = StatusGateEnum.ACTIVE
                    }
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(req.VehicleId)).ReturnsAsync(vehicleResult);
            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(vehicleResult.Data.PlateNumber)).ReturnsAsync(sessionResult);

            // Act
            var result = await _vehicleService.UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_IS_IN_SESSION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync_ShouldReturnFailure_WhenStatusIsAlreadyApplied()
        {
            // Arrange
            var req = new UpdateStatusInactiveAndActiveCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                IsActive = true
            };

            var vehicleResult = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    Id = req.VehicleId,
                    StatusVehicle = StatusVehicleEnum.ACTIVE,
                    PlateNumber = "99L999999"
                }
            };

            var sessionResult = new Return<Session>
            {
                IsSuccess = true,
                Data = new Session
                {
                    PlateNumber = vehicleResult.Data.PlateNumber,
                    Block = 30,
                    ImageInUrl = "image_url",
                    Mode = "MODE1",
                    Status = SessionEnum.CLOSED,
                    TimeIn = DateTime.Now,
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(req.VehicleId)).ReturnsAsync(vehicleResult);
            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(vehicleResult.Data.PlateNumber)).ReturnsAsync(sessionResult);

            // Act
            var result = await _vehicleService.UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.STATUS_IS_ALREADY_APPLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync_ShouldReturnShould_WhenUpdateFails()
        {
            // Arrange
            var req = new UpdateStatusInactiveAndActiveCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                IsActive = true
            };

            var vehicleResult = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    Id = req.VehicleId,
                    StatusVehicle = StatusVehicleEnum.INACTIVE,
                    PlateNumber = "99L999999"
                }
            };

            var sessionResult = new Return<Session>
            {
                IsSuccess = true,
                Data = new Session
                {
                    PlateNumber = vehicleResult.Data.PlateNumber,
                    Block = 30,
                    ImageInUrl = "image_url",
                    Mode = "MODE1",
                    Status = SessionEnum.CLOSED,
                    TimeIn = DateTime.Now,
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var updateResult = new Return<Vehicle>
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(req.VehicleId)).ReturnsAsync(vehicleResult);
            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(vehicleResult.Data.PlateNumber)).ReturnsAsync(sessionResult);
            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleAsync(It.IsAny<Vehicle>())).ReturnsAsync(updateResult);

            // Act
            var result = await _vehicleService.UpdateStatusInactiveAndActiveCustomerVehicleByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetCustomerVehicleByVehicleIdAsync
        // Successful
        [Fact]
        public async Task GetCustomerVehicleByVehicleIdAsync_ShouldReturnSuccess()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var cusId = Guid.NewGuid();

            var vehicleResult = new Return<Vehicle> 
            { 
                Message = SuccessfullyEnumServer.FOUND_OBJECT, 
                Data = new Vehicle 
                { 
                    Id = vehicleId, 
                    CustomerId = cusId, 
                    PlateNumber = "30A12345", 
                    StatusVehicle = StatusVehicleEnum.ACTIVE, 
                } 
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    Id = cusId,
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(vehicleId)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.GetCustomerVehicleByVehicleIdAsync(vehicleId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetCustomerVehicleByVehicleIdAsync_ShouldReturnFailure_WhenUserNotAuthorized()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                IsSuccess = false,
                Data = null
            });

            // Act
            var result = await _vehicleService.GetCustomerVehicleByVehicleIdAsync(vehicleId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetCustomerVehicleByVehicleIdAsync_ShouldReturnFailure_WhenVehicleDoesNotExist()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var cusId = Guid.NewGuid();

            var vehicleResult = new Return<Vehicle>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                Data = null
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    Id = cusId,
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(vehicleId)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.GetCustomerVehicleByVehicleIdAsync(vehicleId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetCustomerVehicleByVehicleIdAsync_ShouldReturnFailure_WhenUserNotOwner()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();

            var vehicleResult = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    Id = vehicleId,
                    CustomerId = Guid.NewGuid(),
                    PlateNumber = "30A12345",
                    StatusVehicle = StatusVehicleEnum.ACTIVE,
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                IsSuccess = true,
                Data = new Customer
                {
                    Id = Guid.NewGuid(),
                    Email = "customer@gmail.com",
                    FullName = "customer",
                    StatusCustomer = StatusCustomerEnum.ACTIVE
                }
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(vehicleId)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.GetCustomerVehicleByVehicleIdAsync(vehicleId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHORITY, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetCustomerVehicleByVehicleIdAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var cusId = Guid.NewGuid();

            var vehicleResult = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    Id = vehicleId,
                    CustomerId = cusId,
                    PlateNumber = "30A12345",
                    StatusVehicle = StatusVehicleEnum.ACTIVE,
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateCustomerAsync()).ReturnsAsync(new Return<Customer>
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
                Data = null,
                IsSuccess = false
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(vehicleId)).ReturnsAsync(vehicleResult);

            // Act
            var result = await _vehicleService.GetCustomerVehicleByVehicleIdAsync(vehicleId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdateVehicleInformationByUserAsync
        // Successful
        [Fact]
        public async Task UpdateVehicleInformationByUserAsync_SuccessfulUpdate_ReturnsSuccess()
        {
            // Arrange
            var req = new UpdateCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                PlateNumber = "51A12345",
                VehicleTypeId = Guid.NewGuid()
            };

            var vehicle = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    PlateNumber = "51A12345",
                    StatusVehicle = StatusVehicleEnum.ACTIVE
                },
                IsSuccess = true
            };

            var session = new Return<Session>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Session
                {
                    TimeIn = DateTime.Now,
                    PlateNumber = "51A12345",
                    Block = 30,
                    Mode = "MODE1",
                    Status = SessionEnum.CLOSED,
                    ImageInUrl = "image_url"
                },
                IsSuccess = true
            };

            var plateNumber = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    PlateNumber = req.PlateNumber,
                    StatusVehicle = StatusVehicleEnum.ACTIVE,    
                    
                },
                IsSuccess = true
            };

            var vehicleType = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    Name = "Car"
                },
                IsSuccess = true
            };

            var updateReturn = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                IsSuccess = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(vehicle);

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(session);

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(plateNumber);

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(vehicleType);

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleAsync(It.IsAny<Vehicle>()))
                .ReturnsAsync(updateReturn);

            // Act
            var result = await _vehicleService.UpdateVehicleInformationByUserAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleInformationByUserAsync_ShouldReturnsFailure_WhenInvalidUser()
        {
            // Arrange
            var req = new UpdateCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                PlateNumber = "51A12345",
                VehicleTypeId = Guid.NewGuid()
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(new Return<User>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                Data = null,
                IsSuccess = false
            });

            // Act
            var result = await _vehicleService.UpdateVehicleInformationByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleInformationByUserAsync_ReturnsFailure_ShouldVehicleNotFound()
        {
            // Arrange
            var req = new UpdateCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                PlateNumber = "51A12345",
                VehicleTypeId = Guid.NewGuid()
            };

            var vehicle = new Return<Vehicle>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                Data = null,
                IsSuccess = false
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(vehicle);

            // Act
            var result = await _vehicleService.UpdateVehicleInformationByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleInformationByUserAsync_ShouldReturnsFailure_WhenVehicleInSession()
        {
            // Arrange
            var req = new UpdateCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                PlateNumber = "51A12345",
                VehicleTypeId = Guid.NewGuid()
            };

            var vehicle = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    PlateNumber = "51A12345",
                    StatusVehicle = StatusVehicleEnum.ACTIVE
                },
                IsSuccess = true
            };

            var session = new Return<Session>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Session
                {
                    TimeIn = DateTime.Now,
                    PlateNumber = "51A12345",
                    Block = 30,
                    Mode = "MODE1",
                    Status = SessionEnum.CLOSED,
                    ImageInUrl = "image_url",
                    GateOut = new Gate
                    {
                        Name = "Gate 1",
                        StatusGate = StatusGateEnum.ACTIVE
                    }
                },
                IsSuccess = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(vehicle);

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(session);

            // Act
            var result = await _vehicleService.UpdateVehicleInformationByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_IS_IN_SESSION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleInformationByUserAsync_ShouldReturnsFailure_WhenInvalidPlateNumber()
        {
            // Arrange
            var req = new UpdateCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                PlateNumber = "INVALID",
                VehicleTypeId = Guid.NewGuid()
            };

            var vehicle = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    PlateNumber = "51A12345",
                    StatusVehicle = StatusVehicleEnum.ACTIVE
                },
                IsSuccess = true
            };

            var session = new Return<Session>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Session
                {
                    TimeIn = DateTime.Now,
                    PlateNumber = "51A12345",
                    Block = 30,
                    Mode = "MODE1",
                    Status = SessionEnum.CLOSED,
                    ImageInUrl = "image_url"
                },
                IsSuccess = true
            };

            var vehicleType = new Return<VehicleType>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new VehicleType
                {
                    Id = req.VehicleTypeId.Value,
                    Name = "Car"
                },
                IsSuccess = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(vehicle);

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(session);

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(vehicleType);

            // Act
            var result = await _vehicleService.UpdateVehicleInformationByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_A_PLATE_NUMBER, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleInformationByUserAsync_ShouldReturnsFailure_WhenPlateNumberAlreadyExists()
        {
            // Arrange
            var req = new UpdateCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                PlateNumber = "51A12345",
                //VehicleTypeId = Guid.NewGuid()
            };

            var vehicle = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    Id = req.VehicleId, 
                    PlateNumber = "99L999999",
                    StatusVehicle = StatusVehicleEnum.ACTIVE
                },
                IsSuccess = true
            };

            var session = new Return<Session>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Session
                {
                    TimeIn = DateTime.Now,
                    PlateNumber = "51A12345",
                    Block = 30,
                    Mode = "MODE1",
                    Status = SessionEnum.CLOSED,
                    ImageInUrl = "image_url"
                },
                IsSuccess = true
            };

            var plateNumber = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    PlateNumber = "51A12345",
                    StatusVehicle = StatusVehicleEnum.ACTIVE,
                },
                IsSuccess = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    PasswordHash = "",
                    PasswordSalt = "",
                    StatusUser = StatusUserEnum.ACTIVE
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(vehicle);

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(session);

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(plateNumber);

            // Act
            var result = await _vehicleService.UpdateVehicleInformationByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PLATE_NUMBER_IS_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateVehicleInformationByUserAsync_InvalidVehicleType_ReturnsFailure()
        {
            // Arrange
            var req = new UpdateCustomerVehicleByUserReqDto
            {
                VehicleId = Guid.NewGuid(),
                PlateNumber = "51A12345",
                VehicleTypeId = Guid.NewGuid()
            };

            var vehicle = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    PlateNumber = "51A12345",
                    StatusVehicle = StatusVehicleEnum.ACTIVE
                },
                IsSuccess = true
            };

            var session = new Return<Session>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Session
                {
                    TimeIn = DateTime.Now,
                    PlateNumber = "51A12345",
                    Block = 30,
                    Mode = "MODE1",
                    Status = SessionEnum.CLOSED,
                    ImageInUrl = "image_url"
                },
                IsSuccess = true
            };

            var plateNumber = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new Vehicle
                {
                    PlateNumber = req.PlateNumber,
                    StatusVehicle = StatusVehicleEnum.ACTIVE,

                },
                IsSuccess = true
            };

            var vehicleType = new Return<VehicleType>
            {
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                Data = null,
                IsSuccess = false
            };

            var updateReturn = new Return<Vehicle>
            {
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY,
                IsSuccess = true
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(vehicle);

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(session);

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(req.PlateNumber))
                .ReturnsAsync(plateNumber);

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(vehicleType);

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleAsync(It.IsAny<Vehicle>()))
                .ReturnsAsync(updateReturn);

            // Act
            var result = await _vehicleService.UpdateVehicleInformationByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST, result.Message);
        }

        // GetStatisticVehicleAsync
        // Successful
        [Fact]
        public async Task GetStatisticVehicleAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var expectedStatistics = new StatisticVehicleResDto
            {
                TotalVehicle = 100,
                TotalNewResgisterVehicleInMonth = 2000
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetStatisticVehicleAsync())
                .ReturnsAsync(new Return<StatisticVehicleResDto>
                {
                    IsSuccess = true,
                    Data = expectedStatistics,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _vehicleService.GetStatisticVehicleAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetStatisticVehicleAsync_ShouldReturnsFailure_WhenInvalidAuth()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                Data = null,
                IsSuccess = false
            });

            // Act
            var result = await _vehicleService.GetStatisticVehicleAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetStatisticVehicleAsync_ShouldReturnFailure_WhenServerError()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetStatisticVehicleAsync())
                .ReturnsAsync(new Return<StatisticVehicleResDto>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _vehicleService.GetStatisticVehicleAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetListVehicleByCustomerIdForUserAsync
        // Successful
        [Fact]
        public async Task GetListVehicleByCustomerIdForUserAsync_ShouldReturnsSuccess_WhenRecordFound()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var vehicleData = new List<Vehicle>
            {
                new Vehicle
                {
                    PlateNumber = "99L999999",
                    VehicleType = new VehicleType 
                    { 
                        Name = "Car" 
                    },
                    PlateImage = "image.jpg",
                    StatusVehicle = StatusVehicleEnum.ACTIVE,
                    CreatedDate = DateTime.Now,
                    Customer = new Customer 
                    { 
                        Id = customerId,
                        Email = "customer@example.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetAllCustomerVehicleByCustomerIdAsync(customerId))
                .ReturnsAsync(new Return<IEnumerable<Vehicle>>
                {
                    IsSuccess = true,
                    Data = vehicleData,
                    TotalRecord = 1,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _vehicleService.GetListVehicleByCustomerIdForUserAsync(customerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Successful
        [Fact]
        public async Task GetListVehicleByCustomerIdForUserAsync_ShouldReturnsSuccess_WhenRecordNotFound()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetAllCustomerVehicleByCustomerIdAsync(customerId))
                .ReturnsAsync(new Return<IEnumerable<Vehicle>>
                {
                    IsSuccess = true,
                    Data = null,
                    TotalRecord = 0,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _vehicleService.GetListVehicleByCustomerIdForUserAsync(customerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListVehicleByCustomerIdForUserAsync_ShouldReturnsFailure_WhenInvalidAuth()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                Data = null,
                IsSuccess = false
            });

            // Act
            var result = await _vehicleService.GetListVehicleByCustomerIdForUserAsync(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        [Fact]
        public async Task GetListVehicleByCustomerIdForUserAsync_ShouldReturnsFailure_WhenServerError()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetAllCustomerVehicleByCustomerIdAsync(customerId))
                .ReturnsAsync(new Return<IEnumerable<Vehicle>>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _vehicleService.GetListVehicleByCustomerIdForUserAsync(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CreateListVehicleForCustomerByUserAsync
        // Successful
        [Fact]
        public async Task CreateListVehicleForCustomerByUserAsync_ShouldReturnsSuccess()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var req = new CreateListVehicleForCustomerByUserReqDto
            {
                CustomerId = customerId,
                Vehicles = new CreateVehiclesNonPriceResDto[]
                {
                    new CreateVehiclesNonPriceResDto
                    {
                        PlateNumber = "51A12345",
                        VehicleTypeId = Guid.NewGuid()
                    }
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(new Return<Customer> 
                { 
                    IsSuccess = true, 
                    Data = new Customer
                    {
                        Email = "customer@gmail.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType> 
                { 
                    Message = SuccessfullyEnumServer.FOUND_OBJECT, 
                    Data = new VehicleType
                    {
                        Name = "Car"
                    },
                    IsSuccess = true
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Vehicle> 
                { 
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = false,
                    Data = null
                });

            _vehicleRepositoryMock.Setup(x => x.CreateVehicleAsync(It.IsAny<Vehicle>()))
                .ReturnsAsync(new Return<Vehicle> 
                { 
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, 
                    Data = new Vehicle
                    {
                        PlateNumber = "99L999999",
                        StatusVehicle = StatusVehicleEnum.ACTIVE    
                    } ,
                    IsSuccess = true
                });

            // Act
            var result = await _vehicleService.CreateListVehicleForCustomerByUserAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateListVehicleForCustomerByUserAsync_ShouldReturnsFailure_WhenInvalidAuth()
        {
            // Arrange
            var req = new CreateListVehicleForCustomerByUserReqDto();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User> 
                { 
                    IsSuccess = false, 
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _vehicleService.CreateListVehicleForCustomerByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);  
        }

        // Failure
        [Fact]
        public async Task CreateListVehicleForCustomerByUserAsync_ShouldReturnsFailure_WhenCustomerNotExist()
        {
            // Arrange
            var req = new CreateListVehicleForCustomerByUserReqDto
            {
                CustomerId = Guid.NewGuid(),
                Vehicles = new CreateVehiclesNonPriceResDto[]
                {
                    new CreateVehiclesNonPriceResDto
                    {
                        PlateNumber = "51A12345",
                        VehicleTypeId = Guid.NewGuid()
                    }
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(req.CustomerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _vehicleService.CreateListVehicleForCustomerByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CUSTOMER_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateListVehicleForCustomerByUserAsync_ShouldReturnsFailure_WhenVehicleTypeNotExist()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var req = new CreateListVehicleForCustomerByUserReqDto
            {
                CustomerId = customerId,
                Vehicles = new CreateVehiclesNonPriceResDto[]
                {
                    new CreateVehiclesNonPriceResDto
                    {
                        PlateNumber = "51A12345",
                        VehicleTypeId = Guid.NewGuid()
                    }
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Email = "customer@gmail.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null,
                    IsSuccess = false
                });

            // Act
            var result = await _vehicleService.CreateListVehicleForCustomerByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateListVehicleForCustomerByUserAsync_ShouldReturnsFailure_WhenInvalidPlateNumber()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var req = new CreateListVehicleForCustomerByUserReqDto
            {
                CustomerId = customerId,
                Vehicles = new CreateVehiclesNonPriceResDto[]
                {
                    new CreateVehiclesNonPriceResDto
                    {
                        PlateNumber = "INVALID",
                        VehicleTypeId = Guid.NewGuid()
                    }
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Email = "customer@gmail.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
               .ReturnsAsync(new Return<VehicleType>
               {
                   Message = SuccessfullyEnumServer.FOUND_OBJECT,
                   Data = new VehicleType
                   {
                       Name = "Car"
                   },
                   IsSuccess = true
               });

            // Act
            var result = await _vehicleService.CreateListVehicleForCustomerByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_A_PLATE_NUMBER, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateListVehicleForCustomerByUserAsync_ShouldReturnsFailure_WhenPlateNumberExists()
        {
            // Arrange
            var customerId = Guid.NewGuid();    

            var req = new CreateListVehicleForCustomerByUserReqDto
            {
                CustomerId = customerId,
                Vehicles = new CreateVehiclesNonPriceResDto[]
                {
                    new CreateVehiclesNonPriceResDto
                    {
                        PlateNumber = "51A12345",
                        VehicleTypeId = Guid.NewGuid()
                    }
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Email = "customer@gmail.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new VehicleType
                    {
                        Name = "Car"
                    },
                    IsSuccess = true
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Vehicle>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Vehicle
                    {
                        PlateNumber = req.Vehicles[0].PlateNumber,
                        StatusVehicle = StatusVehicleEnum.ACTIVE
                    },
                    IsSuccess = true
                });

            // Act
            var result = await _vehicleService.CreateListVehicleForCustomerByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.PLATE_NUMBER_IS_EXIST, result.Message);
        }

        [Fact]
        public async Task CreateListVehicleForCustomerByUserAsync_ShouldReturnsFailure_WhenCreateVehicleFails()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var req = new CreateListVehicleForCustomerByUserReqDto
            {
                CustomerId = customerId,
                Vehicles = new CreateVehiclesNonPriceResDto[]
                {
                    new CreateVehiclesNonPriceResDto
                    {
                        PlateNumber = "51A12345",
                        VehicleTypeId = Guid.NewGuid()
                    }
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _customerRepositoryMock.Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(new Return<Customer>
                {
                    IsSuccess = true,
                    Data = new Customer
                    {
                        Email = "customer@gmail.com",
                        FullName = "customer",
                        StatusCustomer = StatusCustomerEnum.ACTIVE
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleTypeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<VehicleType>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new VehicleType
                    {
                        Name = "Car"
                    },
                    IsSuccess = true
                });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<Vehicle>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = false,
                    Data = null
                });

            _vehicleRepositoryMock.Setup(x => x.CreateVehicleAsync(It.IsAny<Vehicle>()))
                .ReturnsAsync(new Return<Vehicle>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    Data = null,
                    IsSuccess = false
                });
            // Act
            var result = await _vehicleService.CreateListVehicleForCustomerByUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // DeleteVehicleByUserAsync
        // Successful
        [Fact]
        public async Task DeleteVehicleByUserAsync_ShouldReturnSuccess()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var currentTime = DateTime.UtcNow;

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(vehicleId))
                .ReturnsAsync(new Return<Vehicle>
                {
                    IsSuccess = true,
                    Data = new Vehicle
                    {
                        Id = vehicleId,
                        PlateNumber = "99L999999",
                        StatusVehicle = StatusVehicleEnum.ACTIVE,
                        CreatedDate = currentTime,
                        Customer = new Customer
                        {
                            Id = Guid.NewGuid(),
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE
                        },
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleAsync(It.IsAny<Vehicle>()))
                .ReturnsAsync(new Return<Vehicle>
                {
                    IsSuccess = true,
                    Data = new Vehicle
                    {
                        Id = vehicleId,
                        PlateNumber = "99L999999",
                        StatusVehicle = StatusVehicleEnum.ACTIVE,
                        DeletedDate = currentTime,
                        Customer = new Customer
                        {
                            Id = Guid.NewGuid(),
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE
                        },
                    },
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            // Act
            var result = await _vehicleService.DeleteVehicleByUserAsync(vehicleId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteVehicleByUserAsync_ShouldReturnFailure_WhenAuthenticationFailed()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                Data = null,
                IsSuccess = false
            });

            // Act
            var result = await _vehicleService.DeleteVehicleByUserAsync(vehicleId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteVehicleByUserAsync_ShouldReturnFailure_WhenVehicleNotFound()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var currentTime = DateTime.UtcNow;

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(vehicleId))
                .ReturnsAsync(new Return<Vehicle>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _vehicleService.DeleteVehicleByUserAsync(vehicleId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.VEHICLE_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteVehicleByUserAsync_ShouldReturnFailure_WhenUpdateFailed()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var currentTime = DateTime.UtcNow;

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(new Return<User>
            {
                Message = SuccessfullyEnumServer.FOUND_OBJECT,
                Data = new User
                {
                    Email = "user@gmail.com",
                    FullName = "user",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                },
                IsSuccess = true
            });

            _vehicleRepositoryMock.Setup(x => x.GetVehicleByIdAsync(vehicleId))
                .ReturnsAsync(new Return<Vehicle>
                {
                    IsSuccess = true,
                    Data = new Vehicle
                    {
                        Id = vehicleId,
                        PlateNumber = "99L999999",
                        StatusVehicle = StatusVehicleEnum.ACTIVE,
                        CreatedDate = currentTime,
                        Customer = new Customer
                        {
                            Id = Guid.NewGuid(),
                            Email = "customer@gmail.com",
                            FullName = "customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE
                        },
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _vehicleRepositoryMock.Setup(x => x.UpdateVehicleAsync(It.IsAny<Vehicle>()))
                .ReturnsAsync(new Return<Vehicle>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    Data = null,
                    IsSuccess = false
                });

            // Act
            var result = await _vehicleService.DeleteVehicleByUserAsync(vehicleId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }
    }
}
