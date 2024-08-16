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
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = e
                };
            }
        }

        public async Task<Return<IEnumerable<Feedback>>> GetCustomerFeedbacksByCustomerIdAsync(Guid customerGuiId, int pageIndex, int pageSize)
        {
            Return<IEnumerable<Feedback>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR,
            };
            try
            {
                var query = _db.Feedbacks.Where(f => f.CustomerId.Equals(customerGuiId))
                                         .Include(t => t.ParkingArea)
                                         .Where(t => t.DeletedDate == null)
                                         .OrderByDescending(t => t.CreatedDate)
                                         .AsQueryable();                                         

                var result = await query                                                                
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                res.Data = result;
                res.IsSuccess = true;
                res.TotalRecord = query.Count();
                res.Message = query.Any() ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
                return res;
            }
        }

        public async Task<Return<IEnumerable<Feedback>>> GetFeedbacksAsync(int pageSize, int pageIndex, string? cusName, string? parkName)
        {
            try
            {
                var query = _db.Feedbacks
                    .OrderByDescending(t => t.CreatedDate)
                    .Include(f => f.Customer)
                    .Include(f => f.ParkingArea)
                    .Where(t => t.DeletedDate == null)
                    .AsQueryable();

                if(!string.IsNullOrEmpty(cusName))
                {
                    query = query.Where(t => t.Customer.FullName.Contains(cusName));
                }
                if(!string.IsNullOrEmpty(parkName))
                {
                    query = query.Where(t => t.ParkingArea.Name.Contains(parkName));
                }
                if(!string.IsNullOrEmpty(parkName) && !string.IsNullOrEmpty(cusName))
                {
                    query = query
                        .Where(t => t.ParkingArea.Name.Contains(parkName))
                        .Where(t => t.Customer.FullName.Contains(cusName));
                }

                var result = await query                        
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                return new Return<IEnumerable<Feedback>>()
                {
                    Data = result,
                    IsSuccess = true,
                    TotalRecord = query.Count(),
                    Message = query.Any() ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Feedback>>()
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
