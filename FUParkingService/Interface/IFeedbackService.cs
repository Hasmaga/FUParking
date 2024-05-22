using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IFeedbackService
    {
        Task<Return<Feedback>> CreateFeedbackAsync(FeedbackReqDto request, Guid customerId);        
        Task<Return<List<Feedback>>> GetFeedbacksByCustomerIdAsync(Guid customerId, int pageSize, int pageIndex);
    }
}
