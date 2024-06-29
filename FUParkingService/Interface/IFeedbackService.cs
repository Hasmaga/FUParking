using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IFeedbackService
    {
        Task<Return<dynamic>> CreateFeedbackAsync(FeedbackReqDto request);
        Task<Return<IEnumerable<Feedback>>> GetFeedbacksByCustomerIdAsync(int pageSize, int pageIndex);
        Task<Return<IEnumerable<GetListFeedbacksResDto>>> GetFeedbacksAsync(int pageSize, int pageIndex);
    }
}
