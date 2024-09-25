using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.User;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService;
using FUParkingService.Interface;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FUParkingTesting
{
    public class UserServiceTesting
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IRoleRepository> _roleRepositoryMock = new();
        private readonly Mock<IHelpperService> _helpperServiceMock = new();

        private readonly UserService _userService;

        public UserServiceTesting()
        {
            _helpperServiceMock = new Mock<IHelpperService>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _userService = new UserService(_userRepositoryMock.Object, _roleRepositoryMock.Object, _helpperServiceMock.Object);
        }

        // CreateStaffAsync
        // Successful
        [Fact]
        public async Task CreateStaffAsync_ShouldReturnSuccess()
        {
            // Arrange
            var managerId = Guid.NewGuid();

            var req = new CreateUserReqDto
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
                Password = "password123"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Id = managerId,
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = false,
                    Data = null
                });

            _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<Role>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Staff",
                    }
                });

            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    Data = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = req.Email,
                        FullName = req.FullName,
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                        RoleId = Guid.NewGuid()
                    }
                });

            // Act
            var result = await _userService.CreateStaffAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateStaffAsync_ShouldReturnFailure_WhenAuthenticationFails()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                    Data = null
                });

            var req = new CreateUserReqDto
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
            };

            // Act
            var result = await _userService.CreateStaffAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateStaffAsync_ShouldReturnFailure_WhenEmailIsAlreadyRegistered()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            var existEmail = new User
            {
                Email = "staff@gmail.com",
                FullName = "Manager",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = ""
            };

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            var req = new CreateUserReqDto
            {
                Email = existEmail.Email,
                FullName = "Staff",
            };

            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.EMAIL_IS_EXIST
                });

            // Act
            var result = await _userService.CreateStaffAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.EMAIL_IS_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateStaffAsync_ShouldReturnFailure_WhenRoleStaffNotFound()
        {
            // Arrange
            var managerId = Guid.NewGuid();

            var req = new CreateUserReqDto
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
                Password = "password123"
            };
            
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Id = managerId,
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User> 
                { 
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = false,
                    Data = null
                });

            _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<Role>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    Data = null
                });

            // Act
            var result = await _userService.CreateStaffAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.GET_OBJECT_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateStaffAsync_ShouldReturnFailure_WhenCreateUserFails()
        {
            // Arrange
            var managerId = Guid.NewGuid();

            var req = new CreateUserReqDto
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
                Password = "password123"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Id = managerId,
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = false,
                    Data = null
                });

            _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<Role>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Staff",
                    }
                });

            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    Data = null,
                });

            // Act
            var result = await _userService.CreateStaffAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CreateSupervisorAsync
        //Successful
        [Fact]
        public async Task CreateSupervisorAsync_ShouldReturnSuccess()
        {
            // Arrange
            var req = new CreateUserReqDto
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
                Password = "password123"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User> { Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<Role>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Supervisor",
                    }
                });

            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    Data = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = req.Email,
                        FullName = req.FullName,
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                        RoleId = Guid.NewGuid()
                    }
                });

            // Act
            var result = await _userService.CreateSupervisorAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateSupervisorAsync_ShouldReturnFailure_WhenAuthenticationFails()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                    Data = null
                });

            var req = new CreateUserReqDto
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
                Password = "password123"
            };

            // Act
            var result = await _userService.CreateSupervisorAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateSupervisorAsync_ShouldReturnFailure_WhenEmailIsAlreadyRegistered()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            var existEmail = new User
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = ""
            };

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            var req = new CreateUserReqDto
            {
                Email = existEmail.Email,
                FullName = "Staff",
                Password = "password123"
            };

            // Act
            var result = await _userService.CreateSupervisorAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.EMAIL_IS_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateSupervisorAsync_ShouldReturnFailure_WhenRoleSupervisorNotFound()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User> 
                { 
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = false,
                    Data = null
                });

            _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<Role>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR
                });

            var req = new CreateUserReqDto
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
                Password = "password123"
            };

            // Act
            var result = await _userService.CreateSupervisorAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.GET_OBJECT_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateSupervisorAsync_ShouldReturnFailure_WhenCreateUserFails()
        {
            // Arrange
            var req = new CreateUserReqDto
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
                Password = "password123"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User> { Message = ErrorEnumApplication.NOT_FOUND_OBJECT });

            _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<Role>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Supervisor",
                    }
                });

            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    Data = null,
                });

            // Act
            var result = await _userService.CreateSupervisorAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CreateManagerAsync 
        // Successful
        [Fact]
        public async Task CreateManagerAsync_ShouldReturnSuccess()
        {
            // Arrange
            var req = new CreateUserReqDto
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
                Password = "password123"
            };

            var managerId = Guid.NewGuid();
            var roleManagerId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User> 
                { 
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = false,
                    Data = null
                });

            _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<Role>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Supervisor",
                    }
                });

            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    Data = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = req.Email,
                        FullName = req.FullName,
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                        RoleId = Guid.NewGuid()
                    }
                });

            // Act
            var result = await _userService.CreateManagerAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateManagerAsync_ShouldReturnFailure_WhenAuthenticationFails()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                    Data = null
                });

            var req = new CreateUserReqDto
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
                Password = "password123"
            };

            // Act
            var result = await _userService.CreateManagerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateManagerAsync_ShouldReturnFailure_WhenEmailIsAlreadyRegistered()
        {
            // Arrange
            var req = new CreateUserReqDto
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
                Password = "password123"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = req.Email,
                        FullName = req.FullName,
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                        RoleId = Guid.NewGuid()
                    }
                });

            // Act
            var result = await _userService.CreateManagerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.EMAIL_IS_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateManagerAsync_ShouldReturnFailure_WhenRoleManagerNotFound()
        {
            // Arrange
            var req = new CreateUserReqDto
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
                Password = "password123"
            };

            var managerId = Guid.NewGuid();
            var roleManagerId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = false,
                    Data = null
                });

            _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<Role>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            // Act
            var result = await _userService.CreateManagerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.GET_OBJECT_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateManagerAsync_ShouldReturnFailure_WhenCreateUserFails()
        {
            // Arrange
            var req = new CreateUserReqDto
            {
                Email = "staff@gmail.com",
                FullName = "Staff",
                Password = "password123"
            };

            var managerId = Guid.NewGuid();
            var roleManagerId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    IsSuccess = false,
                    Data = null
                });

            _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<Role>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Supervisor",
                    }
                });

            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    Data = null,
                });

            // Act
            var result = await _userService.CreateManagerAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetListUserAsync
        // Successful
        [Fact]
        public async Task GetListUserAsync_ShouldReturnSuccess_WhenUsersRetrievedSuccessfully()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            var userList = new List<User>
            {
                new() {
                    FullName = "John Doe",
                    Email = "john@example.com",
                    CreatedDate = DateTime.UtcNow,
                    Role = new Role 
                    { 
                        Name = RoleEnum.STAFF 
                    },
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = ""
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetListUserAsync(It.IsAny<GetListObjectWithFiller>()))
                .ReturnsAsync(new Return<IEnumerable<User>>
                {
                    IsSuccess = true,
                    Data = userList,
                    TotalRecord = 1,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _userService.GetListUserAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListUserAsync_ShouldReturnFailure_WhenAuthenticationFails()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                    Data = null
                });

            // Act
            var result = await _userService.GetListUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetListUserAsync_ShouldReturnFailure_WhenUserListlFails()
        {
            // Arrange
            var req = new GetListObjectWithFiller();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetListUserAsync(It.IsAny<GetListObjectWithFiller>()))
                .ReturnsAsync(new Return<IEnumerable<User>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    Data = null
                });

            // Act
            var result = await _userService.GetListUserAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // ChangeUserStatusAsync
        // Successful
        [Fact]
        public async Task ChangeUserStatusAsync_ShouldReturnSuccess_WhenStatusChanged()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                StatusUser = StatusUserEnum.ACTIVE,
                Role = new Role { Name = RoleEnum.STAFF },
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = ""
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            // Act
            var result = await _userService.ChangeUserStatusAsync(userId, false);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeUserStatusAsync_ShouldReturnFailure_WhenAuthenticationFails()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                    Data = null,
                });

            // Act
            var result = await _userService.ChangeUserStatusAsync(userId, false);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeUserStatusAsync_ShouldReturnFailure_WhenTryingToChangeOwnStatus()
        {
            // Arrange
            var managerId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Id = managerId,
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            // Act
            var result = await _userService.ChangeUserStatusAsync(managerId, false);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CAN_NOT_CHANGE_STATUS_YOURSELF, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeUserStatusAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Id = managerId,
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            // Act
            var result = await _userService.ChangeUserStatusAsync(userId, false);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeUserStatusAsync_ShouldReturnFailure_WhenTryingToChangeManagerStatus()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                StatusUser = StatusUserEnum.ACTIVE,
                Role = new Role { Name = RoleEnum.MANAGER },
                PasswordHash = "",
                PasswordSalt = "",
                Email = "manager@gmail.com",
                FullName = "Manager",
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _userService.ChangeUserStatusAsync(userId, false);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHORITY, result.Message);
        }

        // Failure
        [Fact]
        public async Task ChangeUserStatusAsync_ShouldReturnFailure_WhenUpdateFail()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                StatusUser = StatusUserEnum.ACTIVE,
                Role = new Role { Name = RoleEnum.STAFF },
                Email = "manager@gmail.com",
                FullName = "Manager",
                PasswordHash = "",
                PasswordSalt = ""
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    Data = null
                });

            // Act
            var result = await _userService.ChangeUserStatusAsync(userId, false);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // ResetWrongPasswordCountAsync
        // Successful
        [Fact]
        public async Task ResetWrongPasswordCountAsync_ShouldReturnSuccess_WhenCountReset()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                WrongPassword = 5,
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = ""
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            // Act
            var result = await _userService.ResetWrongPasswordCountAsync(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task ResetWrongPasswordCountAsync_ShouldReturnError_WhenAuthFails()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                    Data = null
                });

            // Act
            var result = await _userService.ResetWrongPasswordCountAsync(userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task ResetWrongPasswordCountAsync_ShouldReturnError_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            // Act
            var result = await _userService.ResetWrongPasswordCountAsync(userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task ResetWrongPasswordCountAsync_ShouldReturnError_WhenUpdateFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                WrongPassword = 5,
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = ""
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "manager@gmail.com",
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _userService.ResetWrongPasswordCountAsync(userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdateUserAsync
        // Successful
        [Fact]
        public async Task UpdateUserAsync_ShouldReturnSuccess_WhenUserUpdated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateReq = new UpdateUserReqDto
            {
                Id = userId,
                FullName = "New",
                Email = "new@gmail.com"
            };

            var user = new User
            {
                Id = userId,
                FullName = "Old",
                Email = "old@gmail.com",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = ""
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "supervisor@gmail.com",
                        FullName = "Supervisor",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            // Act
            var result = await _userService.UpdateUserAsync(updateReq);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateUserAsync_ShouldReturnError_WhenAuthFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateReq = new UpdateUserReqDto
            {
                Id = userId,
                FullName = "Updated Name",
                Email = "updatedemail@gmail.com"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                    Data = null
                });

            // Act
            var result = await _userService.UpdateUserAsync(updateReq);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateUserAsync_ShouldReturnError_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateReq = new UpdateUserReqDto
            {
                Id = userId,
                FullName = "New",
                Email = "new@gmail.com"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "supervisor@gmail.com",
                        FullName = "Supervisor",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null
                });

            // Act
            var result = await _userService.UpdateUserAsync(updateReq);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdateUserAsync_ShouldReturnError_WhenUpdateFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateReq = new UpdateUserReqDto
            {
                Id = userId,
                FullName = "New",
                Email = "new@gmail.com"
            };

            var user = new User
            {
                Id = userId,
                FullName = "Old",
                Email = "old@gmail.com",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = ""
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = "supervisor@gmail.com",
                        FullName = "Supervisor",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    Data = null
                });

            // Act
            var result = await _userService.UpdateUserAsync(updateReq);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdatePasswordAsync
        // Successful
        [Fact]
        public async Task UpdatePasswordAsync_ShouldReturnSuccess_WhenPasswordUpdated()
        {
            // Arrange
            var req = new UpdateUserPasswordReqDto
            {
                OldPassword = "oldPassword",
                Password = "newPassword"
            };
            var userId = Guid.NewGuid();
            var oldPasswordSalt = Guid.NewGuid().ToByteArray();
            var oldPasswordHash = CreatePassHashAndPassSalt("oldPassword", out oldPasswordSalt);
            var newPasswordSalt = Guid.NewGuid().ToByteArray();
            var newPasswordHash = CreatePassHashAndPassSalt("newPassword", out newPasswordSalt);

            var user = new User
            {
                Id = userId,
                PasswordHash = oldPasswordHash,
                PasswordSalt = Convert.ToBase64String(oldPasswordSalt),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _helpperServiceMock.Setup(x => x.GetAccIdFromLogged())
                .Returns(userId);

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                });

            // Act
            var result = await _userService.UpdatePasswordAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        private static string CreatePassHashAndPassSalt(string password, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(passwordHash);
        }

        // Failure
        [Fact]
        public async Task UpdatePasswordAsync_ShouldReturnFailure_WhenInvalidUser()
        {
            // Arrange
            var req = new UpdateUserPasswordReqDto
            {
                OldPassword = "oldPassword",
                Password = "newPassword"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _userService.UpdatePasswordAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePasswordAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var req = new UpdateUserPasswordReqDto
            {
                OldPassword = "oldPassword",
                Password = "newPassword"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User
                    {
                        StatusUser = StatusUserEnum.ACTIVE,
                        Email = "user@gmail.com",
                        FullName = "User",
                        PasswordHash = "",
                        PasswordSalt = ""
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _helpperServiceMock.Setup(x => x.GetAccIdFromLogged())
                .Returns(Guid.NewGuid());

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _userService.UpdatePasswordAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePasswordAsync_ShouldReturnFailure_WhenOldPasswordIsIncorrect()
        {
            // Arrange
            var req = new UpdateUserPasswordReqDto
            {
                OldPassword = "wrongPassword", // Incorrect old password
                Password = "newPassword"
            };
            var userId = Guid.NewGuid();
            var correctPasswordSalt = Guid.NewGuid().ToByteArray();
            var correctPasswordHash = CreatePassHashAndPassSalt("oldPassword", out correctPasswordSalt);

            var user = new User
            {
                Id = userId,
                PasswordHash = correctPasswordHash,
                PasswordSalt = Convert.ToBase64String(correctPasswordSalt),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _helpperServiceMock.Setup(x => x.GetAccIdFromLogged())
                .Returns(userId);

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _userService.UpdatePasswordAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CRENEDTIAL_IS_WRONG, result.Message);
        }

        // Failure
        [Fact]
        public async Task UpdatePasswordAsync_ShouldReturnFailure_WhenUpdateFails()
        {
            // Arrange
            var req = new UpdateUserPasswordReqDto
            {
                OldPassword = "oldPassword",
                Password = "newPassword"
            };
            var userId = Guid.NewGuid();
            var passwordSalt = Guid.NewGuid().ToByteArray();
            var passwordHash = CreatePassHashAndPassSalt("oldPassword", out passwordSalt);

            var user = new User
            {
                Id = userId,
                PasswordHash = passwordHash,
                PasswordSalt = Convert.ToBase64String(passwordSalt),
                StatusUser = StatusUserEnum.ACTIVE,
                Email = "user@gmail.com",
                FullName = "User"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _helpperServiceMock.Setup(x => x.GetAccIdFromLogged())
                .Returns(userId);

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _userService.UpdatePasswordAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // DeleteUserByIdAsync
        // Successful
        [Fact]
        public async Task DeleteUserByIdAsync_ShouldReturnSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var managerId = Guid.NewGuid();

            var manager = new User
            {
                Id = managerId,
                FullName = "Manager User",
                Email = "manager@gmail.com",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userToDelete = new User
            {
                Id = userId,
                FullName = "User",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
                Email = "user@gmail.com"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = manager,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _helpperServiceMock.Setup(x => x.GetAccIdFromLogged())
                .Returns(managerId);

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = userToDelete,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
                });

            // Act
            var result = await _userService.DeleteUserByIdAsync(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteUserByIdAsync_ShouldReturnFailure_WhenUserTriesToDeleteTheirOwnAccount()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User 
                    {
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                        Email = "user@gmail.com"
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _helpperServiceMock.Setup(x => x.GetAccIdFromLogged())
                .Returns(userId);

            // Act
            var result = await _userService.DeleteUserByIdAsync(userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CAN_NOT_DELETE_YOUR_ACCOUNT, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteUserByIdAsync_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = new User 
                    {
                        FullName = "User",
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = "",
                        Email = "user@gmail.com"
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _helpperServiceMock.Setup(x => x.GetAccIdFromLogged())
                .Returns(Guid.NewGuid());

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.USER_NOT_EXIST
                });

            // Act
            var result = await _userService.DeleteUserByIdAsync(userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.USER_NOT_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteUserByIdAsync_ShouldReturnFailure_WhenAuthorizationFails()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _userService.DeleteUserByIdAsync(userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task DeleteUserByIdAsync_ShouldReturnFailure_WhenDeleteFail()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var managerId = Guid.NewGuid();

            var manager = new User
            {
                Id = managerId,
                FullName = "Manager User",
                Email = "manager@gmail.com",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userToDelete = new User
            {
                Id = userId,
                FullName = "User",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
                Email = "user@gmail.com"
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = manager,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _helpperServiceMock.Setup(x => x.GetAccIdFromLogged())
                .Returns(managerId);

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Data = userToDelete,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });

            // Act
            var result = await _userService.DeleteUserByIdAsync(userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetAllRoleAsync()
        // Successful
        [Fact]
        public async Task GetAllRoleAsync_ShouldReturnSuccess_WhenRolesRetrievedSuccessfully()
        {
            // Arrange
            var roles = new List<Role>
            {
                new Role { Id = Guid.NewGuid(), Name = RoleEnum.MANAGER, Description = "Manager" },
                new Role { Id = Guid.NewGuid(), Name = RoleEnum.STAFF, Description = "Staff" },
                new Role { Id = Guid.NewGuid(), Name = RoleEnum.SUPERVISOR, Description = "Supervisor" }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _roleRepositoryMock.Setup(x => x.GetAllRoleAsync())
                .ReturnsAsync(new Return<IEnumerable<Role>>
                {
                    IsSuccess = true,
                    Data = roles,
                    TotalRecord = roles.Count,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _userService.GetAllRoleAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.FOUND_OBJECT, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllRoleAsync_ShouldReturnFailure_WhenAuthFails()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION,
                });

            // Act
            var result = await _userService.GetAllRoleAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // Failure
        [Fact]
        public async Task GetAllRoleAsync_ShouldReturnFailure_WhenGetFails()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _roleRepositoryMock.Setup(x => x.GetAllRoleAsync())
                .ReturnsAsync(new Return<IEnumerable<Role>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                });

            // Act
            var result = await _userService.GetAllRoleAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // CreateListUserAsync
        // Successful
        [Fact]
        public async Task CreateListUserAsync_ShouldReturnSuccess_WhenAllUsersCreatedSuccessfully()
        {
            // Arrange
            var request = new CreateListUserReqDto
            {
                Users = new[]
                {
                    new CreateUsersReqDto
                    {
                        Email = "new@gmail.com",
                        FullName = "New",
                        Password = "password123",
                        RoleId = Guid.NewGuid()
                    },
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _roleRepositoryMock.Setup(x => x.GetRoleByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Role>
                {
                    IsSuccess = true,
                    Data = new Role 
                    { 
                        Name = RoleEnum.STAFF
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY,
                    Data = new User
                    {
                        Email = request.Users[0].Email,
                        FullName = request.Users[0].FullName,
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            // Act
            var result = await _userService.CreateListUserAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateListUserAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
        {
            // Arrange
            var request = new CreateListUserReqDto
            {
                Users = new[]
                {
                    new CreateUsersReqDto
                    {
                        Email = "new@gmail.com",
                        FullName = "New",
                        Password = "password123",
                        RoleId = Guid.NewGuid()
                    },
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = new User
                    {
                        Email = request.Users[0].Email,
                        FullName = request.Users[0].FullName,
                        StatusUser = StatusUserEnum.ACTIVE,
                        PasswordHash = "",
                        PasswordSalt = ""
                    }
                });

            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.EMAIL_IS_EXIST,
                });

            // Act
            var result = await _userService.CreateListUserAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.EMAIL_IS_EXIST, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateListUserAsync_ShouldReturnFailure_WhenRoleNotFound()
        {
            // Arrange
            var request = new CreateListUserReqDto
            {
                Users = new[]
                {
                    new CreateUsersReqDto
                    {
                        Email = "new@gmail.com",
                        FullName = "New",
                        Password = "password123",
                        RoleId = Guid.NewGuid()
                    },
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _roleRepositoryMock.Setup(x => x.GetRoleByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Role>
                {
                    IsSuccess = false,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                });

            // Act
            var result = await _userService.CreateListUserAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.GET_OBJECT_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateListUserAsync_ShouldReturnFailure_WhenUserCreationFails()
        {
            // Arrange
            var request = new CreateListUserReqDto
            {
                Users = new[]
                {
                    new CreateUsersReqDto
                    {
                        Email = "user1@example.com",
                        FullName = "User 1",
                        Password = "password123",
                        RoleId = Guid.NewGuid()
                    }
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
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

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            _roleRepositoryMock.Setup(x => x.GetRoleByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<Role>
                {
                    IsSuccess = true,
                    Data = new Role
                    {
                        Name = RoleEnum.STAFF
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                });

            // Act
            var result = await _userService.CreateListUserAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // Failure
        [Fact]
        public async Task CreateListUserAsync_ShouldReturnFailure_WhenAuthFails()
        {
            // Arrange
            var request = new CreateListUserReqDto
            {
                Users = new[]
                {
                    new CreateUsersReqDto
                    {
                        Email = "new@example.com",
                        FullName = "New",
                        Password = "password123",
                        RoleId = Guid.NewGuid()
                    }
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.MANAGER))
                .ReturnsAsync(new Return<User>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.NOT_AUTHENTICATION
                });

            // Act
            var result = await _userService.CreateListUserAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

    }
}
