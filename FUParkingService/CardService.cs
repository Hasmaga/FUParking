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

        public async Task<Return<bool>> CreateNewCardAsync(CreateNewCardReqDto req)
        {
            try
            {
                // Check token valid
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                if (!Auth.AuthSupervisor.Contains(accountLogin.Data.Role?.Name ?? ""))
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }

                // Check card number is exist
                var card = await _cardRepository.GetCardByCardNumberAsync(req.CardNumber);
                if (card.Data?.CardNumber != null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.CARD_IS_EXIST,
                        IsSuccess = false
                    };
                }

                // Create new card
                Card newCard = new()
                {
                    PlateNumber = req.PlateNumber,
                    CardNumber = req.CardNumber
                };

                var res = await _cardRepository.CreateCardAsync(newCard);
                if (res.IsSuccess)
                {
                    return new Return<bool>
                    {
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                    };
                }
                else
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.ADD_OBJECT_ERROR
                    };
                }
            }
            catch
            {
                return new Return<bool>
                {
                    IsSuccess = false,
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
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                {
                    return new Return<List<GetCardResDto>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                if (!Auth.AuthSupervisor.Contains(accountLogin.Data.Role?.Name ?? ""))
                {
                    return new Return<List<GetCardResDto>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }

                var res = await _cardRepository.GetAllCardsAsync(req);
                if (res.IsSuccess)
                {
                    List<GetCardResDto> listCard = [];
                    foreach (var item in res.Data ?? [])
                    {
                        listCard.Add(new GetCardResDto
                        {
                            Id = item.Id,
                            CardNumber = item.CardNumber,
                            CreatedDate = item.CreatedDate,
                            PlateNumber = item.PlateNumber
                        });
                    }
                    return new Return<List<GetCardResDto>>
                    {
                        TotalRecord = listCard.Count,
                        IsSuccess = true,
                        Data = listCard,
                        Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                    };
                }
                else
                {
                    return new Return<List<GetCardResDto>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.GET_OBJECT_ERROR
                    };
                }
            }
            catch (Exception ex)
            {
                return new Return<List<GetCardResDto>>
                {
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<bool>> DeleteCardByIdAsync(Guid id)
        {
            try
            {
                // Check token valid
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                if (!Auth.AuthSupervisor.Contains(accountLogin.Data.Role?.Name ?? ""))
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }

                // Check card is exist
                var card = await _cardRepository.GetCardByIdAsync(id);
                if (!card.IsSuccess || card.Data == null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.CARD_NOT_EXIST,
                        IsSuccess = false
                    };
                }
                card.Data.DeletedDate = DateTime.Now;
                var res = await _cardRepository.UpdateCardAsync(card.Data);
                if (res.IsSuccess)
                {
                    return new Return<bool>
                    {
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                    };
                }
                else
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR
                    };
                }
            }
            catch
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<bool>> UpdatePlateNumberCard(string PlateNumber, Guid CardId)
        {
            try
            {
                // Check token valid
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check logged in account is manager or supervisor
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (accountLogin.IsSuccess == false || accountLogin.Data == null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                if (!Auth.AuthSupervisor.Contains(accountLogin.Data.Role?.Name ?? ""))
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                        IsSuccess = false
                    };
                }
                // Check card is exist
                var card = await _cardRepository.GetCardByIdAsync(CardId);
                if (!card.IsSuccess || card.Data == null)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.CARD_NOT_EXIST,
                        IsSuccess = false
                    };
                }
                card.Data.PlateNumber = PlateNumber;
                var res = await _cardRepository.UpdateCardAsync(card.Data);
                if (res.IsSuccess)
                {
                    return new Return<bool>
                    {
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                    };
                }
                else
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR
                    };
                }
            }
            catch
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
