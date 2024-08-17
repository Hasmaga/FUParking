using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
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
        public async Task<IActionResult> CustomerViewFeedbacksAsync([FromQuery] GetListObjectWithPageReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _feedbackService.GetFeedbacksByCustomerIdAsync(req.PageSize, req.PageIndex);
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
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _feedbackService.CreateFeedbackAsync(request);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when create feedback: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetFeedbacksAsync([FromQuery] string? cusName, [FromQuery] string? parkName, [FromQuery] int pageIndex = Pagination.PAGE_INDEX, [FromQuery] int pageSize = Pagination.PAGE_SIZE)
        {
            var result = await _feedbackService.GetFeedbacksAsync(pageSize, pageIndex, cusName, parkName);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when get feedbacks: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }
    }
}
