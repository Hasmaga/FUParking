using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Card;
using FUParkingModel.ResponseObject.Card;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IHelpperService _helpperService;
        private readonly ISessionRepository _sessionRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IVehicleRepository _vehicleRepository;

        public CardService(ICardRepository cardRepository, IHelpperService helpperService, ISessionRepository sessionRepository, ICustomerRepository customerRepository, IVehicleRepository vehicleRepository)
        {
            _cardRepository = cardRepository;
            _helpperService = helpperService;
            _sessionRepository = sessionRepository;
            _customerRepository = customerRepository;
            _vehicleRepository = vehicleRepository;
        }

        public async Task<Return<dynamic>> CreateNewCardAsync(CreateNewCardReqDto req)
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
                var isExist = await _cardRepository.GetCardByCardNumberAsync(req.CardNumber);
                if (!isExist.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.CARD_IS_EXIST
                    };
                }
                if (req.PlateNumber is not null)
                {
                    var isExistPlateNumber = await _cardRepository.GetCardByPlateNumberAsync(req.PlateNumber);
                    if (!isExistPlateNumber.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                    {
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = isExistPlateNumber.InternalErrorMessage,
                            Message = ErrorEnumApplication.PLATE_NUMBER_IS_EXIST
                        };
                    }
                }
                Card newCard = new()
                {
                    PlateNumber = req.PlateNumber,
                    CardNumber = req.CardNumber,
                    CreatedById = checkAuth.Data.Id,
                    Status = StatusCustomerEnum.ACTIVE
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
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
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
                if (isCardInUse.Data is not null)
                {
                    if (isCardInUse.Data?.GateOutId is null)
                    {
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = isCardInUse.InternalErrorMessage,
                            Message = ErrorEnumApplication.CARD_IN_USE
                        };
                    }
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
                if (isCardInUse.Data is not null)
                {
                    if (isCardInUse.Data?.GateOutId is null)
                    {
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = isCardInUse.InternalErrorMessage,
                            Message = ErrorEnumApplication.CARD_IN_USE
                        };
                    }
                }
                // Check plate number is exist in other card
                var isExist = await _cardRepository.GetCardByPlateNumberAsync(PlateNumber);
                if (!isExist.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.PLATE_NUMBER_IS_EXIST_IN_OTHER_CARD
                    };
                }
                card.Data.PlateNumber = PlateNumber;
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
                if (isCardInUse.Data is not null)
                {
                    if (isCardInUse.Data?.GateOutId is null)
                    {
                        return new Return<bool>
                        {
                            InternalErrorMessage = isCardInUse.InternalErrorMessage,
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
                    return new Return<GetCardByCardNumberResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };

                var card = await _cardRepository.GetCardByCardNumberAsync(cardNumber);
                if (!card.IsSuccess)
                    return new Return<GetCardByCardNumberResDto>
                    {
                        InternalErrorMessage = card.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };

                if (card.Data == null)
                    return new Return<GetCardByCardNumberResDto>
                    {
                        Message = ErrorEnumApplication.NOT_FOUND_OBJECT
                    };

                var session = await _sessionRepository.GetNewestSessionByCardIdAsync(card.Data.Id);
                if (!session.IsSuccess)
                    return new Return<GetCardByCardNumberResDto>
                    {
                        InternalErrorMessage = session.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };

                var responseDto = new GetCardByCardNumberResDto
                {
                    cardNumber = card.Data.CardNumber,
                    plateNumber = card.Data.PlateNumber,
                    status = card.Data.Status.ToString()
                };

                // If the session exists and is PARKED, populate session-related data
                if (session.Data != null && session.Data.Status == SessionEnum.PARKED)
                {
                    var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(session.Data.VehicleTypeId);

                    responseDto.sessionId = session.Data.Id;
                    responseDto.sessionPlateNumber = session.Data.PlateNumber;
                    responseDto.sessionVehicleType = vehicleType.Data?.Name;
                    responseDto.sessionTimeIn = session.Data.TimeIn.ToString();
                    responseDto.sessionGateIn = session.Data.GateIn?.Name;

                    if (session.Data.CustomerId.HasValue)
                    {
                        var customer = await _customerRepository.GetCustomerByIdAsync(session.Data.CustomerId.Value);
                        if (!customer.IsSuccess)
                            return new Return<GetCardByCardNumberResDto>
                            {
                                InternalErrorMessage = customer.InternalErrorMessage,
                                Message = ErrorEnumApplication.SERVER_ERROR
                            };

                        responseDto.sessionCustomerName = customer.Data?.FullName;
                        responseDto.sessionCustomerEmail = customer.Data?.Email;
                    }
                }

                return new Return<GetCardByCardNumberResDto>
                {
                    IsSuccess = true,
                    Data = responseDto,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                };
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
    }
}
