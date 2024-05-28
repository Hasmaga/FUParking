using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IParkingAreaRepository _parkingAreaRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository, ICustomerRepository customerRepository,IParkingAreaRepository parkingAreaRepository)
        {
            _feedbackRepository = feedbackRepository;
            _customerRepository = customerRepository;
            _parkingAreaRepository = parkingAreaRepository;
        }

        public async Task<Return<Feedback>> CreateFeedbackAsync(FeedbackReqDto request, Guid customerId)
        {
            Return<Feedback> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                Return<Customer> customerRes = await _customerRepository.GetCustomerByIdAsync(customerId);
                if(customerRes.Data == null)
                {
                    res.Message = customerRes.Message;
                    return res;
                }

                if((customerRes.Data.StatusCustomer ?? "").ToLower().Equals(StatusCustomerEnum.INACTIVE))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }

                if(!Guid.TryParse(request.ParkingAreaId, out var parkingAreaGuId)) {
                    res.Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST;
                    return res;
                }

                Return<ParkingArea> foundParkingAreaRes = await _parkingAreaRepository.GetParkingAreaByIdAsync(parkingAreaGuId);

                if(foundParkingAreaRes.Data == null)
                {
                    res.Message = foundParkingAreaRes.Message;
                    return res;
                }

                Feedback newFeedback = new()
                {
                    CustomerId = customerId,
                    Description = request.Description?.Trim(),
                    ParkingAreaId = foundParkingAreaRes.Data.Id,
                    Title = request.Title?.Trim() ?? "Untitled"
                };
                Return<Feedback> createFeedbackRes = await _feedbackRepository.CreateFeedbackAsync(newFeedback);
                if(createFeedbackRes.Data == null)
                {
                    res.Message = createFeedbackRes.Message;
                    return res;
                }

                res.Message = createFeedbackRes.Message;
                res.IsSuccess = true;
                res.Data = createFeedbackRes.Data;

                return res;
            }catch (Exception)
            {
                throw;
            }
        }

        public async Task<Return<List<Feedback>>> GetFeedbacksByCustomerIdAsync(Guid customerId, int pageSize, int pageIndex)
        {
            Return<List<Feedback>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                Return<Customer> customerRes = await _customerRepository.GetCustomerByIdAsync(customerId);
                if (customerRes.Data == null)
                {
                    res.Message = customerRes.Message;
                    return res;
                }

                if ((customerRes.Data.StatusCustomer ?? "").ToLower().Equals(StatusCustomerEnum.INACTIVE.ToLower()))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }

                res = await _feedbackRepository.GetCustomerFeedbacksByCustomerIdAsync(customerId,pageIndex,pageSize);
                return res;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
