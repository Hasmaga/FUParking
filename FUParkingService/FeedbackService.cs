using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ResponseObject.Feedback;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IHelpperService _helpperService;
        private readonly IParkingAreaRepository _parkingAreaRepository;        
        private readonly ISessionRepository _sessionRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository, IParkingAreaRepository parkingAreaRepository, IHelpperService helpperService, ISessionRepository sessionRepository)
        {
            _feedbackRepository = feedbackRepository;
            _parkingAreaRepository = parkingAreaRepository;
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
                var validateCustomer = await _helpperService.ValidateCustomerAsync();
                if (!validateCustomer.IsSuccess || validateCustomer.Data is null)
                {
                    res.InternalErrorMessage = validateCustomer.InternalErrorMessage;
                    res.Message = validateCustomer.Message;
                    return res;
                }
                var isSessionExist = await _sessionRepository.GetSessionByIdAsync(request.SessionId);
                if (!isSessionExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || isSessionExist.Data == null)
                {
                    res.InternalErrorMessage = isSessionExist.InternalErrorMessage;
                    res.Message = ErrorEnumApplication.NOT_FOUND_OBJECT;
                    return res;
                }
                // Get parking area by session
                var foundParkingAreaRes = await _parkingAreaRepository.GetParkingAreaByIdAsync(isSessionExist.Data.GateIn?.ParkingAreaId ?? Guid.Empty);
                if (!foundParkingAreaRes.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || foundParkingAreaRes.Data == null)
                {
                    res.InternalErrorMessage = foundParkingAreaRes.InternalErrorMessage;
                    res.Message = ErrorEnumApplication.NOT_FOUND_OBJECT;
                    return res;
                }
                Feedback newFeedback = new()
                {
                    CustomerId = validateCustomer.Data.Id,
                    Description = request.Description.Trim(),
                    ParkingAreaId = foundParkingAreaRes.Data.Id,
                    SessionId = isSessionExist.Data.Id,
                    Title = request.Title.Trim()
                };
                var createFeedbackRes = await _feedbackRepository.CreateFeedbackAsync(newFeedback);
                if (!createFeedbackRes.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    res.InternalErrorMessage = createFeedbackRes.InternalErrorMessage;
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

        public async Task<Return<IEnumerable<GetFeedbackByCustomerResDto>>> GetFeedbacksByCustomerIdAsync(int pageSize, int pageIndex)
        {
            Return<IEnumerable<GetFeedbackByCustomerResDto>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                var validateCustomer = await _helpperService.ValidateCustomerAsync();
                if (!validateCustomer.IsSuccess || validateCustomer.Data is null)
                {
                    res.InternalErrorMessage = validateCustomer.InternalErrorMessage;
                    res.Message = validateCustomer.Message;
                    return res;
                }
                var result = await _feedbackRepository.GetCustomerFeedbacksByCustomerIdAsync(validateCustomer.Data.Id, pageIndex, pageSize);
                if (!result.IsSuccess)
                {
                    res.InternalErrorMessage = result.InternalErrorMessage;
                    res.Message = result.Message;
                    return res;
                }
                res.Data = result.Data?.Select(fb => new GetFeedbackByCustomerResDto
                {
                    Id = fb.Id,
                    Title = fb.Title,
                    CreatedDate = fb.CreatedDate,
                    Description = fb.Description,
                    ParkingAreaName = fb.ParkingArea?.Name ?? "N/A"
                });
                res.Message = result.Data?.Count() > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT;
                res.IsSuccess = true;
                res.TotalRecord = result.TotalRecord;
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
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetListFeedbacksResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var res = await _feedbackRepository.GetFeedbacksAsync(pageSize, pageIndex);
                if (!res.IsSuccess)
                {
                    return new Return<IEnumerable<GetListFeedbacksResDto>>
                    {
                        InternalErrorMessage = res.InternalErrorMessage,
                        Message = res.Message
                    };
                }
                return new Return<IEnumerable<GetListFeedbacksResDto>>
                {
                    IsSuccess = true,
                    Data = res.Data?.Select(fb => new GetListFeedbacksResDto
                    {
                        CustomerName = fb.Customer?.FullName ?? "N/A",
                        ParkingAreaName = fb.ParkingArea?.Name ?? "N/A",
                        Title = fb.Title ?? "N/A",
                        Description = fb.Description ?? "N/A",
                        CreatedDate = fb.CreatedDate.ToString("dd/MM/yyyy")
                    }),
                    Message = res.TotalRecord > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    TotalRecord = res.TotalRecord
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetListFeedbacksResDto>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

    }
}
