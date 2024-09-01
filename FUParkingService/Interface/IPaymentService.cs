using FUParkingModel.Object;
using FUParkingModel.ResponseObject.Payment;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPaymentService
    {        
        Task<Return<StatisticPaymentByCustomerResDto>> StatisticPaymentByCustomerAsync();
        Task<Return<IEnumerable<StatisticSessionPaymentMethodResDto>>> StatisticSessionPaymentMethodByCustomerAsync();
        Task<Return<IEnumerable<StatisticSessionPaymentMethodResDto>>> StatisticSessionPaymentMethodAsync();
        Task<Return<StatisticPaymentTodayResDto>> GetStatisticPaymentTodayForGateAsync(Guid gateId, DateTime? startDate, DateTime? endDate);
    }
}
