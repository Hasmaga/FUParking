using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IHelpperService _helpperService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IParkingAreaRepository _parkingAreaRepository;
        private readonly IUserRepository _userRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository, ICustomerRepository customerRepository,IParkingAreaRepository parkingAreaRepository, IUserRepository userRepository, IHelpperService helpperService)
        {
            _feedbackRepository = feedbackRepository;
            _customerRepository = customerRepository;
            _parkingAreaRepository = parkingAreaRepository;
            _userRepository = userRepository;
            _helpperService = helpperService;
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

        public async Task<Return<IEnumerable<GetListFeedbacksResDto>>> GetFeedbacksAsync(int pageSize, int pageIndex, Guid userGuid)
        {
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<IEnumerable<GetListFeedbacksResDto>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                // check role
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || !userlogged.IsSuccess)
                {
                    return new Return<IEnumerable<GetListFeedbacksResDto>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                if (!Auth.AuthSupervisor.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<GetListFeedbacksResDto>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                var res = await _feedbackRepository.GetFeedbacksAsync(pageSize, pageIndex);
                return new Return<IEnumerable<GetListFeedbacksResDto>>
                {
                    IsSuccess = res.IsSuccess,
                    Data = res.Data?.Select(fb => new GetListFeedbacksResDto
                    {
                        CustomerName = fb.Customer?.FullName ?? "N/A",
                        ParkingAreaName = fb.ParkingArea?.Name ?? "N/A",
                        Title = fb.Title ?? "N/A",
                        Description = fb.Description ?? "N/A",
                        createdDate = fb.CreatedDate.ToString("dd/MM/yyyy")
                    }),
                    Message = res.Message
                };
            }
            catch
            {
                return new Return<IEnumerable<GetListFeedbacksResDto>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

    }
}
