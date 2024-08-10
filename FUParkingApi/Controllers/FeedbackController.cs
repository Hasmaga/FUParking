using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ResponseObject.Feedback;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/feedbacks")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;        
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(IFeedbackService feedbackService, ILogger<FeedbackController> logger)
        {
            _feedbackService = feedbackService;            
            _logger = logger;
        }

        [HttpGet("customers")]
        public async Task<IActionResult> CustomerViewFeedbacksAsync([FromQuery] int pageIndex = Pagination.PAGE_INDEX, [FromQuery] int pageSize = Pagination.PAGE_SIZE)
        {
            var result = await _feedbackService.GetFeedbacksByCustomerIdAsync(pageSize, pageIndex);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when get feedbacks by customer id: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpPost("customers")]
        public async Task<IActionResult> CustomerCreateFeedbackAsync([FromBody] FeedbackReqDto request)
        {
            var result = await _feedbackService.CreateFeedbackAsync(request);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when create feedback: {ex}", result.InternalErrorMessage);
                }
                return result.Message switch
                {
                    ErrorEnumApplication.NOT_AUTHENTICATION => StatusCode(401, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHENTICATION }),
                    ErrorEnumApplication.NOT_AUTHORITY => StatusCode(409, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY }),
                    ErrorEnumApplication.ACCOUNT_IS_BANNED => StatusCode(403, new Return<dynamic> { Message = ErrorEnumApplication.ACCOUNT_IS_BANNED }),
                    ErrorEnumApplication.NOT_FOUND_OBJECT => StatusCode(404, new Return<dynamic> { Message = ErrorEnumApplication.NOT_FOUND_OBJECT }),
                    _ => StatusCode(500, new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR }),
                };
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetFeedbacksAsync([FromQuery] int pageIndex = Pagination.PAGE_INDEX, [FromQuery] int pageSize = Pagination.PAGE_SIZE)
        {
            var result = await _feedbackService.GetFeedbacksAsync(pageSize, pageIndex);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when get feedbacks: {ex}", result.InternalErrorMessage);
                }
                return result.Message switch
                {
                    ErrorEnumApplication.NOT_AUTHENTICATION => StatusCode(401, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHENTICATION }),
                    ErrorEnumApplication.NOT_AUTHORITY => StatusCode(409, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY }),
                    ErrorEnumApplication.ACCOUNT_IS_LOCK => StatusCode(403, new Return<dynamic> { Message = ErrorEnumApplication.ACCOUNT_IS_LOCK }),
                    ErrorEnumApplication.ACCOUNT_IS_BANNED => StatusCode(403, new Return<dynamic> { Message = ErrorEnumApplication.ACCOUNT_IS_BANNED }),
                    _ => StatusCode(500, new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR }),
                };
            }
            return Ok(result);
        }
    }
}
