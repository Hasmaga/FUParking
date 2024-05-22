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

        [HttpGet]
        public async Task<IActionResult> CustomerViewFeedbacksAsync([FromQuery]int pageIndex=Pagination.PAGE_INDEX, [FromQuery]int pageSize=Pagination.PAGE_SIZE)
        {
            Return<List<Feedback>> res = new()
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

                if (!ModelState.IsValid)
                {
                    return UnprocessableEntity(Helper.GetValidationErrors(ModelState));
                }

                res = await _feedbackService.GetFeedbacksByCustomerIdAsync(customerGuid, pageSize, pageIndex);

                if(res.Message.Equals(ErrorEnumApplication.BANNED))
                {
                    return  Forbid();
                }
                if(!res.IsSuccess)
                {
                    return BadRequest(res);
                }
                return Ok(res);
            }
            catch(Exception)
            {
                return StatusCode(502, res);
            }
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

        [HttpGet]
        public async Task<IActionResult> CustomerViewFeedbacksAsync([FromQuery]int pageIndex=Pagination.PAGE_INDEX, [FromQuery]int pageSize=Pagination.PAGE_SIZE)
        {
            Return<List<Feedback>> res = new()
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

                if (!ModelState.IsValid)
                {
                    return UnprocessableEntity(Helper.GetValidationErrors(ModelState));
                }

                res = await _feedbackService.GetFeedbacksByCustomerIdAsync(customerGuid, pageSize, pageIndex);

                if(res.Message.Equals(ErrorEnumApplication.BANNED))
                {
                    return  Forbid();
                }
                if(!res.IsSuccess)
                {
                    return BadRequest(res);
                }
                return Ok(res);
            }
            catch(Exception)
            {
                return StatusCode(502, res);
            }
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
