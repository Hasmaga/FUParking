using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class PriceItemService : IPriceItemService
    {
        private readonly IPriceItemRepository _priceItemRepository;
        private readonly IPriceTableRepository _priceTableRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHelpperService _helpperService;

        public PriceItemService(IPriceItemRepository priceItemRepository, IPriceTableRepository priceTableRepository, IUserRepository userRepository, IHelpperService helpperService)
        {
            _priceItemRepository = priceItemRepository;
            _priceTableRepository = priceTableRepository;
            _userRepository = userRepository;
            _helpperService = helpperService;
        }

        public async Task<Return<bool>> CreatePriceItemAsync(CreatePriceItemResDto req)
        {
            // Check token 
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check PriceTableId is exist
                var isPriceTableExist = await _priceTableRepository.GetPriceTableByIdAsync(req.PriceTableId);
                if (isPriceTableExist.Data == null || isPriceTableExist.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.PRICE_TABLE_NOT_EXIST
                    };
                }

                // Add PriceItem
                var priceItem = new PriceItem
                {
                    PriceTableId = req.PriceTableId,
                    ApplyFromHour = req.ApplyFromHour ?? DefaultType.DefaultTimeOnly,
                    ApplyToHour = req.ApplyToHour ?? DefaultType.DefaultTimeOnly,
                    MaxPrice = req.MaxPrice,
                    MinPrice = req.MinPrice
                };

                var result = await _priceItemRepository.CreatePriceItemAsync(priceItem);
                if (result.IsSuccess)
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
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
            }
            catch (Exception e)
            {
                return new Return<bool>()
                {
                    InternalErrorMessage = e.Message,
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<bool>> DeletePriceItemAsync(Guid id)
        {
            try
            {
                // Check token 
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                // Check PriceItem is exist
                var isPriceItemExist = await _priceItemRepository.GetPriceItemByIdAsync(id);

                if (isPriceItemExist.Data == null || isPriceItemExist.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.PRICE_ITEM_NOT_EXIST
                    };
                }
                else
                {
                    var result = await _priceItemRepository.DeletePriceItemAsync(isPriceItemExist.Data);
                    if (result.IsSuccess)
                    {
                        return new Return<bool>
                        {
                            IsSuccess = true,
                            Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
                        };
                    }
                    else
                    {
                        return new Return<bool>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                }
            }
            catch (Exception e)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<PriceItem>>> GetAllPriceItemByPriceTableAsync(Guid PriceTableId)
        {
            try
            {
                // Check token 
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<IEnumerable<PriceItem>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<IEnumerable<PriceItem>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<PriceItem>> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check PriceTableId is exist
                var isPriceTableExist = await _priceTableRepository.GetPriceTableByIdAsync(PriceTableId);
                if (isPriceTableExist.Data == null || isPriceTableExist.IsSuccess == false)
                {
                    return new Return<IEnumerable<PriceItem>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.PRICE_TABLE_NOT_EXIST
                    };
                }

                var result = await _priceItemRepository.GetAllPriceItemByPriceTableAsync(PriceTableId);
                if (result.IsSuccess)
                {
                    return new Return<IEnumerable<PriceItem>>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                    };
                }
                else
                {
                    return new Return<IEnumerable<PriceItem>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<PriceItem>>
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
