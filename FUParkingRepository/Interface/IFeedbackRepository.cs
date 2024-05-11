using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IFeedbackRepository
    {
        Task<Return<Feedback>> CreateFeedbackAsync(Feedback feedback);
    }
}
