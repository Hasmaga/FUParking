using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IFeedbackService
    {
        public Task<Return<Feedback>> CreateFeedbackAsync(FeedbackReqDto request, Guid customerId);
    }
}
