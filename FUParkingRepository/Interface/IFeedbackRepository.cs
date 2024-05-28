using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IFeedbackRepository
    {
        Task<Return<Feedback>> CreateFeedbackAsync(Feedback feedback);
        Task<Return<List<Feedback>>> GetCustomerFeedbacksByCustomerIdAsync(Guid customerGuiId, int pageIndex, int pageSize);
        Task<Return<List<Feedback>>> GetFeedbacksAsync(int pageSize, int pageIndex);
    }
}
