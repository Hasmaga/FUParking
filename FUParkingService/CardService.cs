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
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
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
                if (card.Message.Equals(ErrorEnumApplication.SERVER_ERROR))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = card.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                Card newCard = new()
                {
                    PlateNumber = req.PlateNumber,
                    CardNumber = req.CardNumber,
                    CreatedById = accountLogin.Data.Id,
                };
                var res = await _cardRepository.CreateCardAsync(newCard);
                if (!res.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = res.InternalErrorMessage,
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

        public async Task<Return<IEnumerable<GetCardResDto>>> GetListCardAsync(GetCardsWithFillerReqDto req)
        {
            try
            {                
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<IEnumerable<GetCardResDto>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }                
                var accountLogin = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!accountLogin.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || accountLogin.Data == null)
                {
                    return new Return<IEnumerable<GetCardResDto>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthSupervisor.Contains(accountLogin.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<GetCardResDto>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                var res = await _cardRepository.GetAllCardsAsync(req);
                if (!res.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || !res.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<IEnumerable<GetCardResDto>>
                    {
                        InternalErrorMessage = res.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<IEnumerable<GetCardResDto>>
                {
                    IsSuccess = true,
                    Data = res.Data?.Select(x => new GetCardResDto
                    {
                        Id = x.Id,
                        PlateNumber = x.PlateNumber,
                        CardNumber = x.CardNumber,
                        CreatedDate = x.CreatedDate,
                    }),
                    TotalRecord = res.TotalRecord,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetCardResDto>>
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
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }                
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
                var card = await _cardRepository.GetCardByIdAsync(id);
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                {
                    if (card.InternalErrorMessage != null)
                    {
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = card.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.CARD_NOT_EXIST
                    };
                }
                card.Data.DeletedDate = DateTime.Now;
                card.Data.LastModifyById = accountLogin.Data.Id;
                card.Data.LastModifyDate = DateTime.Now;
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

        public async Task<Return<dynamic>> UpdatePlateNumberInCardAsync(string PlateNumber, Guid CardId)
        {
            try
            {                
                if (!_helpperService.IsTokenValid())
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }                
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
                        Message = ErrorEnumApplication.NOT_AUTHORITY,
                    };
                }                
                var card = await _cardRepository.GetCardByIdAsync(CardId);
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                {
                    if (card.InternalErrorMessage != null)
                    {
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = card.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.CARD_NOT_EXIST
                    };
                }
                card.Data.PlateNumber = PlateNumber;
                card.Data.LastModifyById = accountLogin.Data.Id;
                card.Data.LastModifyDate = DateTime.Now;
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
            catch (Exception ex)
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
