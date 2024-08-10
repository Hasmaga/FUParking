using FUParkingModel.Enum;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IHelpperService _helpperService;
        

        public PaymentService(IPaymentRepository paymentRepository, IHelpperService helpperService)
        {
            _paymentRepository = paymentRepository;
            _helpperService = helpperService;            
        }   

        public async Task<Return<IEnumerable<StatisticPaymentByCustomerResDto>>> StatisticPaymentByCustomerAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<StatisticPaymentByCustomerResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _paymentRepository.StatisticPaymentByCustomerAsync(checkAuth.Data.Id);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<StatisticPaymentByCustomerResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                return result;
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<StatisticPaymentByCustomerResDto>>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<StatisticSessionPaymentMethodByCustomerResDto>>> StatisticSessionPaymentMethodByCustomerAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<StatisticSessionPaymentMethodByCustomerResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _paymentRepository.StatisticSessionPaymentMethodByCustomerAsync(checkAuth.Data.Id);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<StatisticSessionPaymentMethodByCustomerResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                return result;
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<StatisticSessionPaymentMethodByCustomerResDto>>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
