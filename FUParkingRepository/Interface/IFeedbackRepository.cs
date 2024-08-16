using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IFeedbackRepository
    {
        Task<Return<Feedback>> CreateFeedbackAsync(Feedback feedback);
        Task<Return<IEnumerable<Feedback>>> GetCustomerFeedbacksByCustomerIdAsync(Guid customerGuiId, int pageIndex, int pageSize);
        Task<Return<IEnumerable<Feedback>>> GetFeedbacksAsync(int pageSize, int pageIndex, string? cusName, string? parkName);
    }
}
