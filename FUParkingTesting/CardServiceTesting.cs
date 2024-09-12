using FUParkingRepository.Interface;
using FUParkingService.Interface;
using FUParkingService;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Card;
using FUParkingModel.ReturnCommon;
using Xunit;
using Bogus;
using FUParkingModel.ResponseObject.Card;
using FUParkingRepository;
using FUParkingModel.ResponseObject.Statistic;

namespace FUParkingTesting
{
    public class CardServiceTesting
    {
        private readonly Mock<IHelpperService> _helpperServiceMock;
        private readonly Mock<ICardRepository> _cardRepositoryMock;
        private readonly Mock<ISessionRepository> _sessionRepositoryMock;
        private readonly CardService _cardService;

        public CardServiceTesting()
        {
            _helpperServiceMock = new Mock<IHelpperService>();
            _cardRepositoryMock = new Mock<ICardRepository>();
            _sessionRepositoryMock = new Mock<ISessionRepository>();
            _cardService = new CardService(_cardRepositoryMock.Object, _helpperServiceMock.Object, _sessionRepositoryMock.Object);
        }

        // CreateNewCardAsync
        // Successfully
        [Fact]
        public async Task CreateNewCardAsync_ShouldReturnSuccess()
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

            var req = new CreateNewCardReqDto
            {
                CardNumbers = new string[]
                {
                    "1234567890"
                }
            };

            var existingCardReturn = new Return<Card>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var createReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card
                {
                    CardNumber = req.CardNumbers[0]
                },
                Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<string>())).ReturnsAsync(existingCardReturn);

            _cardRepositoryMock.Setup(x => x.CreateCardAsync(It.IsAny<Card>())).ReturnsAsync(createReturn);

            // Act
            var result = await _cardService.CreateNewCardAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task CreateNewCardAsync_ShouldReturnError_WhenAuthenticationFails()
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

            var req = new CreateNewCardReqDto
            {
                CardNumbers = new string[]
                {
                    "1234567890"
                }
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            // Act
            var result = await _cardService.CreateNewCardAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task CreateNewCardAsync_ShouldReturnError_WhenCardAlreadyExists()
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

            var req = new CreateNewCardReqDto
            {
                CardNumbers = new string[]
                {
                    "1234567890"
                }
            };

            var existingCardReturn = new Return<Card>
            {
                IsSuccess = true,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var createReturn = new Return<Card>
            {
                IsSuccess = false,
                Data = new Card
                {
                    CardNumber = req.CardNumbers[0]
                },
                Message = ErrorEnumApplication.CARD_IS_EXIST
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<string>())).ReturnsAsync(existingCardReturn);

            _cardRepositoryMock.Setup(x => x.CreateCardAsync(It.IsAny<Card>())).ReturnsAsync(createReturn);

            // Act
            var result = await _cardService.CreateNewCardAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_IS_EXIST, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task CreateNewCardAsync_ShouldReturnError_WhenCreateCardFails()
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

            var req = new CreateNewCardReqDto
            {
                CardNumbers = new string[]
                {
                    "1234567890"
                }
            };

            var existingCardReturn = new Return<Card>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var createReturn = new Return<Card>
            {
                IsSuccess = false,
                Data = new Card
                {
                    CardNumber = req.CardNumbers[0]
                },
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(It.IsAny<string>())).ReturnsAsync(existingCardReturn);

            _cardRepositoryMock.Setup(x => x.CreateCardAsync(It.IsAny<Card>())).ReturnsAsync(createReturn);

            // Act
            var result = await _cardService.CreateNewCardAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetListCardAsync
        // Successfully
        [Fact]
        public async Task GetListCardAsync_ShouldReturnSuccess()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.STAFF.ToString() };
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

            var req = new GetCardsWithFillerReqDto();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);

            var cards = new List<GetCardResDto>
            {
                new GetCardResDto { Id = Guid.NewGuid(), CardNumber = "1234567890" },
                new GetCardResDto { Id = Guid.NewGuid(), CardNumber = "0987654321" }
            };

            var listResult = new Return<IEnumerable<GetCardResDto>>
            {
                IsSuccess = true,
                Data = cards,
                Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
            };

            _cardRepositoryMock.Setup(x => x.GetAllCardsAsync(req)).ReturnsAsync(listResult);

            // Act
            var result = await _cardService.GetListCardAsync(req);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
            Assert.Equal(cards, result.Data);
        }

        // ReturnError
        [Fact]
        public async Task GetListCardAsync_ShouldReturnError_WhenAuthenticationFails()
        {
            // Arrange
            var req = new GetCardsWithFillerReqDto();

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

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);

            // Act
            var result = await _cardService.GetListCardAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
            Assert.Null(result.Data);
        }

        // ReturnError
        [Fact]
        public async Task GetListCardAsync_ShouldReturnError_WhenServerError()
        {
            // Arrange
            var role = new Role { Name = RoleEnum.STAFF.ToString() };
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

            var req = new GetCardsWithFillerReqDto();

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);

            var listResult = new Return<IEnumerable<GetCardResDto>>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _cardRepositoryMock.Setup(x => x.GetAllCardsAsync(req)).ReturnsAsync(listResult);

            // Act
            var result = await _cardService.GetListCardAsync(req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // DeleteCardByIdAsync
        // Successfully
        [Fact]
        public async Task DeleteCardByIdAsync_ShouldReturnSuccess()
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

            var card = new Card
            {
                Id = Guid.NewGuid()
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var existingCardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = card,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var sessionReturn = new Return<Session>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var updateReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card(),
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(It.IsAny<Guid>())).ReturnsAsync(existingCardReturn);

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id)).ReturnsAsync(sessionReturn);

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _cardService.DeleteCardByIdAsync(card.Id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task DeleteCardByIdAsync_ShouldReturnError_WhenAuthenticationFails()
        {
            // Arrange
            var cardId = Guid.NewGuid();

            var user = new User
            {
                FullName = "user",
                Email = "user@localhost.com",
                StatusUser = StatusUserEnum.ACTIVE,
                PasswordHash = "",
                PasswordSalt = "",
            };

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Data = user,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            // Act
            var result = await _cardService.DeleteCardByIdAsync(cardId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task DeleteCardByIdAsync_ShouldReturnError_WhenCardNotFound()
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

            var card = new Card
            {
                Id = Guid.NewGuid()
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardReturn = new Return<Card>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(card.Id)).ReturnsAsync(cardReturn);

            // Act
            var result = await _cardService.DeleteCardByIdAsync(card.Id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_NOT_EXIST, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task DeleteCardByIdAsync_ShouldReturnError_WhenCardInUse()
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

            var card = new Card
            {
                Id = Guid.NewGuid()
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var cardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = card,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var session = new Session
            {
                Status = SessionEnum.PARKED,
                Block = 30,
                Mode = "MODE1",
                TimeIn = DateTime.Now,
                TimeOut = DateTime.Now.AddMinutes(30),
                PlateNumber = "99L999999",
                ImageInUrl = "https://localhost.com/image1.jpg",
            };

            var existingCardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = card,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var sessionReturn = new Return<Session>
            {
                IsSuccess = true,
                Data = session,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var updateReturn = new Return<Card>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.CARD_IN_USE
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(card.Id)).ReturnsAsync(cardReturn);

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(It.IsAny<Guid>())).ReturnsAsync(existingCardReturn);

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id)).ReturnsAsync(sessionReturn);

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _cardService.DeleteCardByIdAsync(card.Id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_IN_USE, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task DeleteCardByIdAsync_ShouldReturnError_WhenUpdateFails()
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

            var card = new Card
            {
                Id = Guid.NewGuid()
            };

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = user,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var existingCardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = card,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            var sessionReturn = new Return<Session>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var updateReturn = new Return<Card>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(card.Id)).ReturnsAsync(existingCardReturn);

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(card.Id)).ReturnsAsync(sessionReturn);

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _cardService.DeleteCardByIdAsync(card.Id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // UpdatePlateNumberInCardAsync
        // Successfully
        [Fact]
        public async Task UpdatePlateNumberInCardAsync_ShouldReturnSuccess()
        {
            // Arrange
            var plateNumber = "99L999999";

            var cardId = Guid.NewGuid();
            var card = new Card { Id = cardId };

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

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardResult = new Return<Card>
            {
                IsSuccess = true,
                Data = card,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(cardId)).ReturnsAsync(cardResult);

            var sessionResult = new Return<Session>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(cardId)).ReturnsAsync(sessionResult);

            var updateResult = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card(),
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateResult);

            // Act
            var result = await _cardService.UpdatePlateNumberInCardAsync(plateNumber, cardId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task UpdatePlateNumberInCardAsync_ShouldReturnError_WhenAuthenticationFails()
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

            var plateNumber = "99L999999";

            var cardId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Data = user,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            // Act
            var result = await _cardService.UpdatePlateNumberInCardAsync(plateNumber, cardId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task UpdatePlateNumberInCardAsync_ShouldReturnError_WhenCardNotFound()
        {
            // Arrange
            var plateNumber = "99L999999";
            var cardId = Guid.NewGuid();

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

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardResult = new Return<Card>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            var updateResult = new Return<Card>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.CARD_NOT_EXIST
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(cardId)).ReturnsAsync(cardResult);

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateResult);

            // Act
            var result = await _cardService.UpdatePlateNumberInCardAsync(plateNumber, cardId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_NOT_EXIST, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task UpdatePlateNumberInCardAsync_ShouldReturnError_WhenCardInUse()
        {
            // Arrange
            var plateNumber = "99L999999";

            var cardId = Guid.NewGuid();
            var card = new Card { Id = cardId };

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

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardResult = new Return<Card>
            {
                IsSuccess = true,
                Data = card,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(cardId)).ReturnsAsync(cardResult);

            var session = new Session
            {
                Status = SessionEnum.PARKED,
                Block = 30,
                Mode = "MODE1",
                TimeIn = DateTime.Now,
                TimeOut = DateTime.Now.AddMinutes(30),
                PlateNumber = "99L999999",
                ImageInUrl = "https://localhost.com/image1.jpg",
            };

            var sessionResult = new Return<Session>
            {
                IsSuccess = true,
                Data = session,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(cardId)).ReturnsAsync(sessionResult);

            var updateResult = new Return<Card>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.CARD_IN_USE
            };

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateResult);

            // Act
            var result = await _cardService.UpdatePlateNumberInCardAsync(plateNumber, cardId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_IN_USE, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task UpdatePlateNumberInCardAsync_ShouldReturnFailure_WhenUpdateFails()
        {
            // Arrange
            var plateNumber = "99L999999";

            var cardId = Guid.NewGuid();
            var card = new Card { Id = cardId };

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

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardResult = new Return<Card>
            {
                IsSuccess = true,
                Data = card,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(cardId)).ReturnsAsync(cardResult);

            var sessionResult = new Return<Session>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(cardId)).ReturnsAsync(sessionResult);

            var updateResult = new Return<Card>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateResult);

            // Act
            var result = await _cardService.UpdatePlateNumberInCardAsync(plateNumber, cardId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // ChangeStatusCardAsync
        // Successfully
        [Fact]
        public async Task ChangeStatusCardAsync_ShouldReturnSuccess_WhenActiveToInactive()
        {
            // Arrange
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    Id = userId,
                    FullName = "supervisor",
                    Email = "supervisor@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card
                {
                    Id = cardId,
                    Status = CardStatusEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(cardId)).ReturnsAsync(cardReturn);

            var newestSessionReturn = new Return<Session>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(cardId)).ReturnsAsync(newestSessionReturn);

            var updateReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card(),
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _cardService.ChangeStatusCardAsync(cardId, false);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // Successfully
        [Fact]
        public async Task ChangeStatusCardAsync_ShouldReturnSuccess_WhenInactiveToActive()
        {
            // Arrange
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    Id = userId,
                    FullName = "supervisor",
                    Email = "supervisor@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card
                {
                    Id = cardId,
                    Status = CardStatusEnum.INACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(cardId)).ReturnsAsync(cardReturn);

            var newestSessionReturn = new Return<Session>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(cardId)).ReturnsAsync(newestSessionReturn);

            var updateReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card(),
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _cardService.ChangeStatusCardAsync(cardId, true);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task ChangeStatusCardAsync_ShouldReturnFailure_WhenUnauthorizedUser()
        {
            // Arrange
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            // Act
            var result = await _cardService.ChangeStatusCardAsync(cardId, true);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task ChangeStatusCardAsync_ShouldReturnError_WhenCardNotExist()
        {
            // Arrange
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    Id = userId,
                    FullName = "supervisor",
                    Email = "supervisor@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardReturn = new Return<Card>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(cardId)).ReturnsAsync(cardReturn);

            // Act
            var result = await _cardService.ChangeStatusCardAsync(cardId, true);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_NOT_EXIST, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task ChangeStatusCardAsync_ShouldReturnFailure_WhenCardInUse()
        {
            // Arrange
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    Id = userId,
                    FullName = "supervisor",
                    Email = "supervisor@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card
                {
                    Id = cardId,
                    Status = CardStatusEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(cardId)).ReturnsAsync(cardReturn);

            var session = new Session
            {
                Status = SessionEnum.PARKED,
                Block = 30,
                Mode = "MODE1",
                TimeIn = DateTime.Now,
                TimeOut = DateTime.Now.AddMinutes(30),
                PlateNumber = "99L999999",
                ImageInUrl = "https://localhost.com/image1.jpg",
            };

            var newestSessionReturn = new Return<Session>
            {
                IsSuccess = true,
                Data = session,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(cardId)).ReturnsAsync(newestSessionReturn);

            var updateReturn = new Return<Card>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.CARD_IN_USE
            };

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _cardService.ChangeStatusCardAsync(cardId, false);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_IN_USE, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task ChangeStatusCardAsync_ShouldReturnFailure_WhenStatusAlreadyApplied()
        {
            // Arrange
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    Id = userId,
                    FullName = "supervisor",
                    Email = "supervisor@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card
                {
                    Id = cardId,
                    Status = CardStatusEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(cardId)).ReturnsAsync(cardReturn);

            var newestSessionReturn = new Return<Session>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(cardId)).ReturnsAsync(newestSessionReturn);

            // Act
            var result = await _cardService.ChangeStatusCardAsync(cardId, true);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.STATUS_IS_ALREADY_APPLY, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task ChangeStatusCardAsync_ShouldReturnFailure_WhenUpdateCardFailed()
        {
            // Arrange
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    Id = userId,
                    FullName = "supervisor",
                    Email = "supervisor@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card
                {
                    Id = cardId,
                    Status = CardStatusEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(cardId)).ReturnsAsync(cardReturn);

            var newestSessionReturn = new Return<Session>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(cardId)).ReturnsAsync(newestSessionReturn);

            var updateReturn = new Return<Card>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _cardService.ChangeStatusCardAsync(cardId, false);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // ChangeStatusCardToMissingAsync
        // Successfully
        [Fact]
        public async Task ChangeStatusCardToMissingAsync_ShouldReturnSuccess()
        {
            // Arrange
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    Id = userId,
                    FullName = "supervisor",
                    Email = "supervisor@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card
                {
                    Id = cardId,
                    Status = CardStatusEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(cardId)).ReturnsAsync(cardReturn);

            var updateReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card(),
                Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
            };

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _cardService.ChangeStatusCardToMissingAsync(cardId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task ChangeStatusCardToMissingAsync_ShouldReturnFailure_WhenAuthenticationFailed()
        {
            // Arrange
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            // Act
            var result = await _cardService.ChangeStatusCardToMissingAsync(cardId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task ChangeStatusCardToMissingAsync_ShouldReturnFailure_WhenCardNotFound()
        {
            // Arrange
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    Id = userId,
                    FullName = "supervisor",
                    Email = "supervisor@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardReturn = new Return<Card>
            {
                IsSuccess = false,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(cardId)).ReturnsAsync(cardReturn);

            var updateReturn = new Return<Card>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.CARD_NOT_EXIST
            };

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _cardService.ChangeStatusCardToMissingAsync(cardId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.CARD_NOT_EXIST, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task ChangeStatusCardToMissingAsync_UpdateFailed()
        {
            // Arrange
            var cardId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    Id = userId,
                    FullName = "supervisor",
                    Email = "supervisor@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var cardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card
                {
                    Id = cardId,
                    Status = CardStatusEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByIdAsync(cardId)).ReturnsAsync(cardReturn);

            var updateReturn = new Return<Card>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.SERVER_ERROR
            };

            _cardRepositoryMock.Setup(x => x.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updateReturn);

            // Act
            var result = await _cardService.ChangeStatusCardToMissingAsync(cardId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.SERVER_ERROR, result.Message);
        }

        // GetStatisticCardAsync
        // Successfully
        [Fact]
        public async Task GetStatisticCardAsync_ShouldReturnSuccess()
        {
            // Arrange

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    FullName = "supervisor",
                    Email = "supervisor@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            var expectedStatistic = new StatisticCardResDto
            {
                TotalCard = 10,
                TotalCardInUse = 5,
            };

            var cardReturn = new Return<StatisticCardResDto>
            {
                IsSuccess = true,
                Data = expectedStatistic,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetStatisticCardAsync()).ReturnsAsync(cardReturn);

            // Act
            var result = await _cardService.GetStatisticCardAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedStatistic, result.Data);
        }

        // ReturnError
        [Fact]
        public async Task GetStatisticCardAsync_ShouldReturnError_WhenAuthenticationFailed()
        {
            // Arrange
            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.SUPERVISOR)).ReturnsAsync(userReturn);

            // Act
            var result = await _cardService.GetStatisticCardAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // GetCardByCardNumberAsync
        // Successfully
        [Fact]
        public async Task GetCardByCardNumberAsync_ShoulReturnSuccess_WhenWithActiveSession()
        {
            // Arrange
            var cardNumber = "99L999999";
            var cardId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    FullName = "staff",
                    Email = "staff@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);

            var cardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card
                {
                    Id = cardId,
                    CardNumber = cardNumber,
                    Status = CardStatusEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(cardNumber)).ReturnsAsync(cardReturn);

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(cardId))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = new Session
                    {
                        Id = Guid.NewGuid(),
                        Status = SessionEnum.PARKED,
                        Customer = new Customer 
                        { 
                            Email = "customer@example.com", 
                            FullName = "Customer",
                            StatusCustomer = StatusCustomerEnum.ACTIVE
                        },
                        GateIn = new Gate 
                        { 
                            Name = "Gate 1",
                            StatusGate = StatusGateEnum.ACTIVE
                        },
                        PlateNumber = "99L999999",
                        TimeIn = DateTime.Now,
                        VehicleType = new VehicleType 
                        { 
                            Name = "Car" 
                        },
                        ImageInUrl = "http://example.com/image.jpg",
                        ImageInBodyUrl = "http://example.com/body.jpg",
                        Mode = "MODE1",
                        Block = 30
                    },
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                });

            // Act
            var result = await _cardService.GetCardByCardNumberAsync(cardNumber);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // ReturnErrors
        [Fact]
        public async Task GetCardByCardNumberAsync_ShouldReturnSuccess_WhenWithoutSession()
        {
            // Arrange
            var cardNumber = "99L999999";
            var cardId = Guid.NewGuid();

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    FullName = "staff",
                    Email = "staff@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);

            var cardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = new Card
                {
                    Id = cardId,
                    CardNumber = cardNumber,
                    Status = CardStatusEnum.ACTIVE
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(cardNumber)).ReturnsAsync(cardReturn);

            _sessionRepositoryMock.Setup(x => x.GetNewestSessionByCardIdAsync(cardId))
                .ReturnsAsync(new Return<Session>
                {
                    IsSuccess = true,
                    Data = null,
                    Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                });

            // Act
            var result = await _cardService.GetCardByCardNumberAsync(cardNumber);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task GetCardByCardNumberAsync_ShouldReturnFailure_WhenAuthenticationFailed()
        {
            // Arrange
            var cardNumber = "99L999999";

            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION,
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);

            // Act
            var result = await _cardService.GetCardByCardNumberAsync(cardNumber);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }

        // ReturnError
        [Fact]
        public async Task GetCardByCardNumberAsync_ShouldReturnFailure_WhenCardNotFound()
        {
            // Arrange
            var cardNumber = "99L999999";

            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    FullName = "staff",
                    Email = "staff@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);

            var cardReturn = new Return<Card>
            {
                IsSuccess = true,
                Data = null,
                Message = ErrorEnumApplication.NOT_FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardByCardNumberAsync(cardNumber)).ReturnsAsync(cardReturn);

            // Act
            var result = await _cardService.GetCardByCardNumberAsync(cardNumber);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_FOUND_OBJECT, result.Message);
        }

        // GetCardOptionAsync
        // Successfully
        [Fact]
        public async Task GetCardOptionAsync_ShouldReturnSuccess()
        {
            // Arrange
            var userReturn = new Return<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    FullName = "staff",
                    Email = "staff@localhost.com",
                    StatusUser = StatusUserEnum.ACTIVE,
                    PasswordHash = "",
                    PasswordSalt = "",
                },
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);

            var cardOptions = new List<GetCardOptionsResDto>
            {
                new GetCardOptionsResDto
                {
                    Id = Guid.NewGuid(),
                    CardNumber = "99L999999"
                },
                new GetCardOptionsResDto
                {
                    Id = Guid.NewGuid(),
                    CardNumber = "66L6666666"
                }
            };

            var cardReturn = new Return<IEnumerable<GetCardOptionsResDto>>
            {
                IsSuccess = true,
                Data = cardOptions,
                Message = SuccessfullyEnumServer.FOUND_OBJECT
            };

            _cardRepositoryMock.Setup(x => x.GetCardOptionAsync()).ReturnsAsync(cardReturn);

            // Act
            var result = await _cardService.GetCardOptionAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(cardOptions, result.Data);
        }

        // ReturnError
        [Fact]
        public async Task GetCardOptionAsync_ShouldReturnFailure_WhenAuthenticationFailed()
        {
            // Arrange
            var userReturn = new Return<User>
            {
                IsSuccess = false,
                Message = ErrorEnumApplication.NOT_AUTHENTICATION
            };

            _helpperServiceMock.Setup(x => x.ValidateUserAsync(RoleEnum.STAFF)).ReturnsAsync(userReturn);

            // Act
            var result = await _cardService.GetCardOptionAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorEnumApplication.NOT_AUTHENTICATION, result.Message);
        }
    }
}
