using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public FeedbackRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Feedback>> CreateFeedbackAsync(Feedback feedback)
        {
            try
            {
                await _db.Feedbacks.AddAsync(feedback);
                await _db.SaveChangesAsync();
                return new Return<Feedback>
                {
                    Data = feedback,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Feedback>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<List<Feedback>>> GetCustomerFeedbacksByCustomerIdAsync(Guid customerGuiId, int pageIndex, int pageSize)
        {
            Return<List<Feedback>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                List<Feedback> feedbacks = await _db.Feedbacks.Where(f => f.CustomerId.Equals(customerGuiId))
                                                                .OrderByDescending(t => t.CreatedDate)
                                                                .Skip((pageIndex - 1) * pageSize)
                                                                .Take(pageSize)
                                                                .ToListAsync();
                res.Data = feedbacks;
                res.IsSuccess = true;
                res.Message = SuccessfullyEnumServer.SUCCESSFULLY;
                return res;

            }
            catch (Exception)
            {
                return res;
            }
        }

        public async Task<Return<List<Feedback>>> GetFeedbacksAsync(int pageSize, int pageIndex)
        {
            try
            {
                var feedbacks = await _db.Feedbacks
                    .OrderByDescending(t => t.CreatedDate)
                    .Include(f => f.Customer)
                    .Include(f => f.ParkingArea)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new Return<List<Feedback>>()
                {
                    Data = feedbacks,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<List<Feedback>>()
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR
                };
            }
        }
    }
}
