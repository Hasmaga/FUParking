using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHelpperService _helpperService;

        public PaymentMethodService(IPaymentMethodRepository paymentMethodRepository, IUserRepository userRepository, IHelpperService helpperService)
        {
            _paymentMethodRepository = paymentMethodRepository;
            _userRepository = userRepository;
            _helpperService = helpperService;
        }

        public async Task<Return<bool>> CreatePaymentMethodAsync(CreatePaymentMethodReqDto req)
        {
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

                var paymentMethod = new PaymentMethod
                {
                    Name = req.Name,
                    Description = req.Description                    
                };

                var result = await _paymentMethodRepository.CreatePaymentMethodAsync(paymentMethod);
                if (result.IsSuccess)
                {
                    return new Return<bool>
                    {
                        IsSuccess = true,
                        Data = true,
                        Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                    };
                } else
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
            } catch (Exception)
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
