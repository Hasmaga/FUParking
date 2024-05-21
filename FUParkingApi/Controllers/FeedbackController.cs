using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingService;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/feedback")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IHelpperService _helpperService;

        public FeedbackController(IFeedbackService feedbackService, IHelpperService helpperService)
        {
            _feedbackService = feedbackService;
            _helpperService = helpperService;
        }

        [HttpPost]
        public async Task<IActionResult> CustomerCreateFeedbackAsync([FromBody] FeedbackReqDto request)
        {
            Return<Feedback> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                Guid customerGuid = _helpperService.GetAccIdFromLogged();
                if (customerGuid == Guid.Empty)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return Unauthorized(res);
                };

                if(!ModelState.IsValid)
                {
                    return UnprocessableEntity(Helper.GetValidationErrors(ModelState));
                }

                Return<Feedback> createFeedbackRes = await _feedbackService.CreateFeedbackAsync(request, customerGuid);
                res.Message = createFeedbackRes.Message;
                if(!createFeedbackRes.IsSuccess)
                {
                    return BadRequest(res);
                }
                return Ok(createFeedbackRes);
            }
            catch (Exception)
            {
                return StatusCode(502, res);
            }
        }

    }
}
