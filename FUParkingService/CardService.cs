using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Card;
using FUParkingModel.ResponseObject.Card;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.Transactions;

namespace FUParkingService
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IHelpperService _helpperService;
        private readonly ISessionRepository _sessionRepository;        

        public CardService(ICardRepository cardRepository, IHelpperService helpperService, ISessionRepository sessionRepository)
        {
            _cardRepository = cardRepository;
            _helpperService = helpperService;
            _sessionRepository = sessionRepository;            
        }

        public async Task<Return<dynamic>> CreateNewCardAsync(CreateNewCardReqDto req)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                foreach (var item in req.CardNumbers)
                {
                    var card = await _cardRepository.GetCardByCardNumberAsync(item);
                    if (!card.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.CARD_IS_EXIST,
                            InternalErrorMessage = card.InternalErrorMessage
                        };
                    }
                    var newCard = new Card()
                    {
                        CardNumber = item,
                        CreatedById = checkAuth.Data.Id,
                        Status = CardStatusEnum.ACTIVE
                    };
                    var result = await _cardRepository.CreateCardAsync(newCard);
                    if (!result.IsSuccess)
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.SERVER_ERROR,
                            InternalErrorMessage = result.InternalErrorMessage
                        };
                    }
                }
                scope.Complete();
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
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetCardResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var res = await _cardRepository.GetAllCardsAsync(req);
                if (!res.IsSuccess)
                {
                    return new Return<IEnumerable<GetCardResDto>>
                    {
                        InternalErrorMessage = res.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return res;
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
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var card = await _cardRepository.GetCardByIdAsync(id);
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = card.InternalErrorMessage,
                        Message = ErrorEnumApplication.CARD_NOT_EXIST
                    };
                }
                // Check card is in use
                var isCardInUse = await _sessionRepository.GetNewestSessionByCardIdAsync(id);
                if (!isCardInUse.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isCardInUse.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }                
                if (isCardInUse.Data != null)
                {
                    if (isCardInUse.Data.Status.Equals(SessionEnum.PARKED))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.CARD_IN_USE
                        };
                    }
                }
                card.Data.DeletedDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                card.Data.LastModifyById = checkAuth.Data.Id;
                card.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var res = await _cardRepository.UpdateCardAsync(card.Data);
                if (!res.IsSuccess)
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
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var card = await _cardRepository.GetCardByIdAsync(CardId);
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = card.InternalErrorMessage,
                        Message = ErrorEnumApplication.CARD_NOT_EXIST
                    };
                }
                // Check card is in use
                var isCardInUse = await _sessionRepository.GetNewestSessionByCardIdAsync(CardId);
                if (!isCardInUse.IsSuccess)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isCardInUse.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (isCardInUse.Data != null)
                {
                    if (isCardInUse.Data.Status.Equals(SessionEnum.PARKED))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.CARD_IN_USE
                        };
                    }
                }                
                card.Data.LastModifyById = checkAuth.Data.Id;
                card.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var res = await _cardRepository.UpdateCardAsync(card.Data);
                if (!res.IsSuccess)
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

        public async Task<Return<bool>> ChangeStatusCardAsync(Guid cardId, bool isActive)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var card = await _cardRepository.GetCardByIdAsync(cardId);
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = card.InternalErrorMessage,
                        Message = ErrorEnumApplication.CARD_NOT_EXIST
                    };
                }
                // Check card is in use
                var isCardInUse = await _sessionRepository.GetNewestSessionByCardIdAsync(cardId);
                if (!isCardInUse.IsSuccess)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = isCardInUse.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (isCardInUse.Data != null)
                {
                    if (isCardInUse.Data.Status.Equals(SessionEnum.PARKED))
                    {
                        return new Return<bool>
                        {
                            Message = ErrorEnumApplication.CARD_IN_USE
                        };
                    }
                }

                if (isActive)
                {                    
                    if (card.Data.Status.Equals(CardStatusEnum.ACTIVE))
                    {
                        return new Return<bool>
                        {
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    card.Data.Status = CardStatusEnum.ACTIVE;
                }
                else if (!isActive)
                {
                    if (card.Data.Status.Equals(CardStatusEnum.INACTIVE))
                    {
                        return new Return<bool>
                        {
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    card.Data.Status = CardStatusEnum.INACTIVE;
                }                
                card.Data.LastModifyById = checkAuth.Data.Id;
                card.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var res = await _cardRepository.UpdateCardAsync(card.Data);
                if (!res.IsSuccess)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = res.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<bool>> ChangeStatusCardToMissingAsync(Guid cardId)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var card = await _cardRepository.GetCardByIdAsync(cardId);
                if (!card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || card.Data == null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = card.InternalErrorMessage,
                        Message = ErrorEnumApplication.CARD_NOT_EXIST
                    };
                }               

                card.Data.Status = CardStatusEnum.MISSING;
                card.Data.LastModifyById = checkAuth.Data.Id;
                card.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var res = await _cardRepository.UpdateCardAsync(card.Data);
                if (!res.IsSuccess)
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        InternalErrorMessage = res.InternalErrorMessage
                    };
                }
                return new Return<bool>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<StatisticCardResDto>> GetStatisticCardAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<StatisticCardResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _cardRepository.GetStatisticCardAsync();
                if (!result.IsSuccess)
                {
                    return new Return<StatisticCardResDto>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Return<StatisticCardResDto>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<GetCardByCardNumberResDto>> GetCardByCardNumberAsync(string cardNumber)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<GetCardByCardNumberResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var card = await _cardRepository.GetCardByCardNumberAsync(cardNumber);
                if (!card.IsSuccess)
                {
                    return new Return<GetCardByCardNumberResDto>
                    {
                        InternalErrorMessage = card.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }

                if (card.Data == null)
                {
                    return new Return<GetCardByCardNumberResDto>
                    {
                        Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                    };
                }

                var session = await _sessionRepository.GetNewestSessionByCardIdAsync(card.Data.Id);
                if (!session.IsSuccess)
                {
                    return new Return<GetCardByCardNumberResDto>
                    {
                        InternalErrorMessage = session.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }

                var response = new Return<GetCardByCardNumberResDto>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data = new GetCardByCardNumberResDto
                    {
                        CardId = card.Data.Id,
                        CardNumber = card.Data.CardNumber,
                        Status = card.Data.Status
                    }
                };

                if (session.Data is not null &&
                    (session.Data.Status.Equals(SessionEnum.PARKED) || session.Data.Status.Equals(SessionEnum.CLOSED)))
                {
                    response.Data.SessionGateIn = session.Data.GateIn?.Name;
                    response.Data.SessionId = session.Data.Id;
                    response.Data.SessionPlateNumber = session.Data.PlateNumber;
                    response.Data.SessionTimeIn = session.Data.TimeIn;
                    response.Data.SessionVehicleType = session.Data.VehicleType?.Description;
                    response.Data.ImageInUrl = session.Data.ImageInUrl;
                    response.Data.ImageInBodyUrl = session.Data.ImageInBodyUrl;
                    response.Data.SessionStatus = session.Data.Status;

                    if (session.Data.Status.Equals(SessionEnum.CLOSED))
                    {
                        response.Data.SessionTimeOut = session.Data.TimeOut;
                        response.Data.SessionGateOut = session.Data.GateOut?.Name;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                return new Return<GetCardByCardNumberResDto>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }


        public async Task<Return<IEnumerable<GetCardOptionsResDto>>> GetCardOptionAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetCardOptionsResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _cardRepository.GetCardOptionAsync();
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetCardOptionsResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetCardOptionsResDto>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
