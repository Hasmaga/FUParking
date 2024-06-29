using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Card;
using FUParkingModel.ResponseObject.Card;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IHelpperService _helpperService;
        private readonly IUserRepository _userRepository;

        public CardService(ICardRepository cardRepository, IHelpperService helpperService, IUserRepository userRepository)
        {
            _cardRepository = cardRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
        }

        public async Task<Return<dynamic>> CreateNewCardAsync(CreateNewCardReqDto req)
        {
            try
            {
                // Check token valid
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY                        
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!accountLogin.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || accountLogin.Data == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY                        
                    };
                }
                if (!Auth.AuthSupervisor.Contains(accountLogin.Data.Role?.Name ?? ""))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY                        
                    };
                }

                // Check card number is exist
                var card = await _cardRepository.GetCardByCardNumberAsync(req.CardNumber);
                if (card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.CARD_IS_EXIST                        
                    };
                }
                else if (card.Message.Equals(ErrorEnumApplication.SERVER_ERROR))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR                        
                    };
                }
                // Create new card
                Card newCard = new()
                {
                    PlateNumber = req.PlateNumber,
                    CardNumber = req.CardNumber
                };

                var res = await _cardRepository.CreateCardAsync(newCard);
                if (!res.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<List<GetCardResDto>>> GetListCardAsync(GetCardsWithFillerReqDto req)
        {
            try
            {
                // Check token valid
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<List<GetCardResDto>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY                        
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                {
                    return new Return<List<GetCardResDto>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY                        
                    };
                }
                if (!Auth.AuthSupervisor.Contains(accountLogin.Data.Role?.Name ?? ""))
                {
                    return new Return<List<GetCardResDto>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY                        
                    };
                }

                var res = await _cardRepository.GetAllCardsAsync(req);
                if (!res.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || !res.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<List<GetCardResDto>>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR                        
                    };
                }
                var listCard = new List<GetCardResDto>();
                foreach (var item in res.Data ?? [])
                {
                    listCard.Add(new GetCardResDto
                    {
                        Id = item.Id,
                        CardNumber = item.CardNumber,
                        PlateNumber = item.PlateNumber
                    });
                }
                return new Return<List<GetCardResDto>>
                {
                    IsSuccess = true,
                    Data = listCard,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<List<GetCardResDto>>
                {                    
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> DeleteCardByIdAsync(Guid id)
        {
            try
            {
                // Check token valid
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY                        
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY                        
                    };
                }
                if (!Auth.AuthSupervisor.Contains(accountLogin.Data.Role?.Name ?? ""))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY                        
                    };
                }

                // Check card is exist
                var card = await _cardRepository.GetCardByIdAsync(id);
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.CARD_NOT_EXIST                        
                    };
                }
                card.Data.DeletedDate = DateTime.Now;
                var res = await _cardRepository.UpdateCardAsync(card.Data);
                if (!res.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = res.InternalErrorMessage
                    };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {         
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> UpdatePlateNumberCard(string PlateNumber, Guid CardId)
        {
            try
            {
                // Check token valid
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY                       
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY                        
                    };
                }
                if (!Auth.AuthSupervisor.Contains(accountLogin.Data.Role?.Name ?? ""))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,                        
                    };
                }
                // Check card is exist
                var card = await _cardRepository.GetCardByIdAsync(CardId);
                if (!card.IsSuccess || card.Data == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.CARD_NOT_EXIST,                        
                    };
                }
                card.Data.PlateNumber = PlateNumber;
                var res = await _cardRepository.UpdateCardAsync(card.Data);
                if (!res.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = res.InternalErrorMessage
                    };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch(Exception ex)
            {
                return new Return<dynamic>
                {
                    InternalErrorMessage = ex,                    
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
