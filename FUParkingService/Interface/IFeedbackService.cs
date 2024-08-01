using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ResponseObject.Feedback;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IFeedbackService
    {
        Task<Return<dynamic>> CreateFeedbackAsync(FeedbackReqDto request);
        Task<Return<IEnumerable<GetFeedbackByCustomerResDto>>> GetFeedbacksByCustomerIdAsync(int pageSize, int pageIndex);
        Task<Return<IEnumerable<GetListFeedbacksResDto>>> GetFeedbacksAsync(int pageSize, int pageIndex);
    }
}
