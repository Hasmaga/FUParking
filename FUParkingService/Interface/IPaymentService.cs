using FUParkingModel.Object;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IPaymentService
    {        
        Task<Return<IEnumerable<StatisticPaymentByCustomerResDto>>> StatisticPaymentByCustomerAsync();
        Task<Return<IEnumerable<StatisticSessionPaymentMethodByCustomerResDto>>> StatisticSessionPaymentMethodByCustomerAsync();
    }
}
