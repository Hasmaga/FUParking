using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService;
using FUParkingService.Interface;
using Moq;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using System.Text;
using Xunit;
using FUParkingService.MailService;

namespace FUParkingTesting
{
    public class AuthServiceTesting
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly Mock<IWalletRepository> _walletRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IHelpperService> _helpperServiceMock = new();
        private readonly Mock<IMailService> _mailServiceMock = new();
        private readonly AuthService _authService;

        public AuthServiceTesting()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _configurationMock = new Mock<IConfiguration>();
            _walletRepositoryMock = new Mock<IWalletRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _mailServiceMock = new Mock<IMailService>();
            _helpperServiceMock = new Mock<IHelpperService>();
            _authService = new AuthService(_customerRepositoryMock.Object, _configurationMock.Object, _walletRepositoryMock.Object, _userRepositoryMock.Object, _helpperServiceMock.Object, _mailServiceMock.Object);
        }

        // LoginWithCredentialAsync
        // Failure
        [Fact]
        public async Task LoginWithCredentialAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var loginRequest = new LoginWithCredentialReqDto { Email = "user@gmail.com", Password = "password" };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null,
                    IsSuccess = false
                });

            // Act
            var result = await _authService.LoginWithCredentialAsync(loginRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CRENEDTIAL_IS_WRONG, result.Message);
        }

        // Failure
        [Fact]
        public async Task LoginWithCredentialAsync_ShouldReturnFailure_WhenAccountIsLocked()
        {
            // Arrange
            var loginRequest = new LoginWithCredentialReqDto { Email = "user@gmail.com", Password = "password" };

            var user = new User 
            { 
                WrongPassword = 5, 
                StatusUser = StatusUserEnum.ACTIVE,
                Email = loginRequest.Email,
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = ""
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = user,
                    IsSuccess = true
                });

            // Act
            var result = await _authService.LoginWithCredentialAsync(loginRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.ACCOUNT_IS_LOCK, result.Message);
        }

        // Failure
        [Fact]
        public async Task LoginWithCredentialAsync_ShouldReturnFailure_WhenAccountIsInactive()
        {
            // Arrange
            var loginRequest = new LoginWithCredentialReqDto { Email = "user@gmail.com", Password = "password" };
            var user = new User 
            {
                StatusUser = StatusUserEnum.INACTIVE,
                Email = loginRequest.Email,
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = ""
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = user,
                    IsSuccess = true
                });

            // Act
            var result = await _authService.LoginWithCredentialAsync(loginRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.ACCOUNT_IS_INACTIVE, result.Message);
        }

        // Failure
        [Fact]
        public async Task LoginWithCredentialAsync_ShouldReturnFailure_PasswordIsWrong()
        {
            // Arrange
            var loginRequest = new LoginWithCredentialReqDto { Email = "user@gmail.com", Password = "passwordsai" };

            var user = new User
            {
                StatusUser = StatusUserEnum.ACTIVE,
                Email = loginRequest.Email,
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = ""
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = user
                });

            _userRepositoryMock.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User> 
                { 
                    IsSuccess = true,
                    Message = ErrorEnumApplication.CRENEDTIAL_IS_WRONG,
                });

            // Act
            var result = await _authService.LoginWithCredentialAsync(loginRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CRENEDTIAL_IS_WRONG, result.Message);
        }

        // Failure
        [Fact]
        public async Task LoginWithCredentialAsync_ShouldReturnFailure_WhenUpdatingWrongPasswordFails()
        {
            // Arrange
            var loginRequest = new LoginWithCredentialReqDto { Email = "user@gmail.com", Password = "passwordsai" };
            var user = new User
            {
                StatusUser = StatusUserEnum.ACTIVE,
                Email = loginRequest.Email,
                FullName = "User",
                PasswordHash = "",
                PasswordSalt = ""
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = user
                });

            _userRepositoryMock.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new Return<User> 
                { 
                    IsSuccess = false, 
                    Message = ErrorEnumApplication.CRENEDTIAL_IS_WRONG
                });

            // Act
            var result = await _authService.LoginWithCredentialAsync(loginRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CRENEDTIAL_IS_WRONG, result.Message);
        }

        // Successful
        [Fact]
        public async Task LoginWithCredentialAsync_ShouldReturnSuccess()
        {
            // Arrange
            var loginRequest = new LoginWithCredentialReqDto
            {
                Email = "user@gmail.com",
                Password = "password"
            };

            var PasswordSalt = Guid.NewGuid().ToByteArray();
            var PasswordHash = CreatePassHashAndPassSalt("password", out PasswordSalt);

            var user = new User
            {
                Email = "user@gmail.com",
                FullName = "User",
                PasswordHash = PasswordHash,
                PasswordSalt = Convert.ToBase64String(PasswordSalt),
                StatusUser = StatusUserEnum.ACTIVE,
                Role = new Role { Name = "User" }
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginRequest.Email))
                .ReturnsAsync(new Return<User>
                {
                    Data = user,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    IsSuccess = true
                });

            var sectionMock = new Mock<IConfigurationSection>();
            sectionMock.Setup(s => s.Value).Returns("ThisIsAReallyLongSuperSecretTokenKeyThatIsAtLeast64CharactersLong");

            _configurationMock.Setup(config => config.GetSection("AppSettings:Token"))
                .Returns(sectionMock.Object);

            // Act
            var result = await _authService.LoginWithCredentialAsync(loginRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.LOGIN_SUCCESSFULLY, result.Message);
            Assert.NotNull(result.Data?.BearerToken);
        }

        private static string CreatePassHashAndPassSalt(string password, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(passwordHash);
        }

        // CheckRoleByTokenAsync
        // Successful
        [Fact]
        public async Task CheckRoleByTokenAsync_ShouldReturnSuccess_WhenValidTokenAndActiveAccount()
        {
            // Arrange
            _helpperServiceMock.Setup(x => x.IsTokenValid()).Returns(true);
            _helpperServiceMock.Setup(x => x.GetAccIdFromLogged()).Returns(Guid.NewGuid());

            var user = new User
            {
                FullName = "user",
                Email = "user@gmail.com",
                WrongPassword = 0,
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
                Role = new Role
                {
                    Name = RoleEnum.MANAGER,
                }
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = user,
                    IsSuccess = true
                });

            // Act
            var result = await _authService.CheckRoleByTokenAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckRoleByTokenAsync_ShouldReturnFailure_WhenAccountIsBanned()
        {
            // Arrange
            _helpperServiceMock.Setup(service => service.IsTokenValid()).Returns(true);
            _helpperServiceMock.Setup(service => service.GetAccIdFromLogged()).Returns(Guid.NewGuid());

            var user = new User
            {
                FullName = "user",
                Email = "user@gmail.com",
                WrongPassword = 0,
                StatusUser = StatusUserEnum.INACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
                Role = new Role
                {
                    Name = RoleEnum.MANAGER,
                }
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = user
                });

            // Act
            var result = await _authService.CheckRoleByTokenAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.ACCOUNT_IS_BANNED, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckRoleByTokenAsync_ShouldReturnFailure_WhenAccountIsLocked()
        {
            // Arrange
            _helpperServiceMock.Setup(service => service.IsTokenValid()).Returns(true);
            _helpperServiceMock.Setup(service => service.GetAccIdFromLogged()).Returns(Guid.NewGuid());

            var user = new User
            {
                FullName = "user",
                Email = "user@gmail.com",
                WrongPassword = 6,
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
                Role = new Role
                {
                    Name = RoleEnum.MANAGER,
                }
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = user
                });

            // Act
            var result = await _authService.CheckRoleByTokenAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.ACCOUNT_IS_LOCK, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckRoleByTokenAsync_ShouldReturnFailure_WhenAccountHasNoRole()
        {
            // Arrange
            _helpperServiceMock.Setup(service => service.IsTokenValid()).Returns(true);
            _helpperServiceMock.Setup(service => service.GetAccIdFromLogged()).Returns(Guid.NewGuid());

            var user = new User
            {
                FullName = "user",
                Email = "user@gmail.com",
                WrongPassword = 0,
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = SuccessfullyEnumServer.FOUND_OBJECT,
                    Data = user
                });

            // Act
            var result = await _authService.CheckRoleByTokenAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHORITY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckRoleByTokenAsync_ShouldReturnFailure_WhenAccountNotFound()
        {
            // Arrange
            _helpperServiceMock.Setup(service => service.IsTokenValid()).Returns(true);
            _helpperServiceMock.Setup(service => service.GetAccIdFromLogged()).Returns(Guid.NewGuid());

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Return<User>
                {
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT,
                    Data = null,
                    IsSuccess = false
                });

            // Act
            var result = await _authService.CheckRoleByTokenAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHORITY, result.Message);
        }

        // Failure
        [Fact]
        public async Task CheckRoleByTokenAsync_ShouldReturnFailure_WhenTokenIsInvalid()
        {
            // Arrange
            _helpperServiceMock.Setup(service => service.IsTokenValid()).Returns(false);

            // Act
            var result = await _authService.CheckRoleByTokenAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }
    }
}
