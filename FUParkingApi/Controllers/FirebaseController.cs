using FirebaseService;
using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject.Firebase;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [Route("api/notifications")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class FirebaseController : Controller
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<DepositController> _logger;

        public FirebaseController(IFirebaseService firebaseService, ICustomerService customerService, ILogger<DepositController> logger)
        {
            _firebaseService = firebaseService;
            _customerService = customerService;
            _logger = logger;
        }

        [HttpPost("send-notification")]
        public async Task<IActionResult> SendNotification([FromBody] FirebaseReqDto request)
        {
            var result = await _firebaseService.SendNotificationAsync(request);
            if (!result.Message.Equals(SuccessfullyEnumServer.SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when Send Notification: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }

        [HttpPut("customer/{fcmToken}")]
        public async Task<IActionResult> UpdateCustomerFcmToken([FromRoute] string fcmToken)
        {
            var result = await _customerService.UpdateCustomerFCMTokenAsync(fcmToken);
            if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when update customer FCM token: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return Ok(result);
        }
    }
}
