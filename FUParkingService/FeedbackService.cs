using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;
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
        private readonly ISessionRepository _sessionRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository, ICustomerRepository customerRepository, IParkingAreaRepository parkingAreaRepository, IUserRepository userRepository, IHelpperService helpperService, ISessionRepository sessionRepository)
        {
            _feedbackRepository = feedbackRepository;
            _customerRepository = customerRepository;
            _parkingAreaRepository = parkingAreaRepository;
            _userRepository = userRepository;
            _helpperService = helpperService;
            _sessionRepository = sessionRepository;
        }

        public async Task<Return<dynamic>> CreateFeedbackAsync(FeedbackReqDto request)
        {
            Return<dynamic> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                if (!_helpperService.IsTokenValid())
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                var customerLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!customerLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customerLogged.Data == null)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }

                if (customerLogged.Data.StatusCustomer.Equals(StatusCustomerEnum.INACTIVE))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }

                // Check session is exist
                var isSessionExist = await _sessionRepository.GetSessionByIdAsync(request.SessionId);
                if (!isSessionExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || isSessionExist.Data == null)
                {
                    res.Message = ErrorEnumApplication.NOT_FOUND_OBJECT;
                    return res;
                }

                // Get parking area by session
                var foundParkingAreaRes = await _parkingAreaRepository.GetParkingAreaByIdAsync(isSessionExist.Data.GateIn?.ParkingAreaId ?? Guid.Empty);
                if (!foundParkingAreaRes.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || foundParkingAreaRes.Data == null)
                {
                    res.Message = ErrorEnumApplication.NOT_FOUND_OBJECT;
                    return res;
                }

                Feedback newFeedback = new()
                {
                    CustomerId = customerLogged.Data.Id,
                    Description = request.Description.Trim(),
                    ParkingAreaId = foundParkingAreaRes.Data.Id,
                    SessionId = isSessionExist.Data.Id,
                    Title = request.Title.Trim()
                };
                Return<Feedback> createFeedbackRes = await _feedbackRepository.CreateFeedbackAsync(newFeedback);
                if (createFeedbackRes.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    res.Message = ErrorEnumApplication.SERVER_ERROR;
                    return res;
                }
                res.Message = SuccessfullyEnumServer.SUCCESSFULLY;
                res.IsSuccess = true;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
                return res;
            }
        }

        public async Task<Return<IEnumerable<Feedback>>> GetFeedbacksByCustomerIdAsync(int pageSize, int pageIndex)
        {
            Return<IEnumerable<Feedback>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                if (!_helpperService.IsTokenValid())
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                var customerLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!customerLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customerLogged.Data == null)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }

                if (customerLogged.Data.StatusCustomer.Equals(StatusCustomerEnum.INACTIVE))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }
                res = await _feedbackRepository.GetCustomerFeedbacksByCustomerIdAsync(customerLogged.Data.Id, pageIndex, pageSize);
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
                return res;
            }
        }

        public async Task<Return<IEnumerable<GetListFeedbacksResDto>>> GetFeedbacksAsync(int pageSize, int pageIndex)
        {
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<IEnumerable<GetListFeedbacksResDto>>
                    {                       
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                // check role
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || !userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
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
                        CreatedDate = fb.CreatedDate.ToString("dd/MM/yyyy")
                    }),
                    Message = res.Message
                };
            }
            catch
            {
                return new Return<IEnumerable<GetListFeedbacksResDto>>
                {                    
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

    }
}
