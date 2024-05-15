using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository;
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

        public async Task<Return<bool>> CreatePriceItem(CreatePriceItemResDto req)
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
    }
}
