using FUParkingModel.Enum;
using FUParkingModel.ReturnCommon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FUParkingApi.HelperClass
{
    public class Helper()
    {
        public static Return<Dictionary<string, List<string>?>> GetValidationErrors(ModelStateDictionary modelState)
        {
            var errors = modelState.ToDictionary(
                entry => entry.Key,
                entry => entry.Value?.Errors.Select(error => error.ErrorMessage).ToList()
            );

            return new Return<Dictionary<string, List<string>?>>
            {
                Data = errors,
                IsSuccess = false,
                Message = ErrorEnumApplication.INVALID_INPUT
            };
        }

        public static IActionResult GetErrorResponse(string error)
        {
            var result = new Return<dynamic> { Message = error };

            return error switch
            {
                ErrorEnumApplication.NOT_AUTHENTICATION => new ObjectResult(result) { StatusCode = 401 },
                ErrorEnumApplication.NOT_AUTHORITY => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.ACCOUNT_IS_LOCK => new ObjectResult(result) { StatusCode = 403 },
                ErrorEnumApplication.ACCOUNT_IS_BANNED => new ObjectResult(result) { StatusCode = 403 },                
                ErrorEnumApplication.PACKAGE_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.ADD_OBJECT_ERROR => new ObjectResult(result) { StatusCode = 500 },
                ErrorEnumApplication.OBJECT_EXISTED => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.GET_OBJECT_ERROR => new ObjectResult(result) { StatusCode = 500 },
                ErrorEnumApplication.UPDATE_OBJECT_ERROR => new ObjectResult(result) { StatusCode = 500 },
                ErrorEnumApplication.DELETE_OBJECT_ERROR => new ObjectResult(result) { StatusCode = 500 },
                ErrorEnumApplication.GOOGLE_LOGIN_FAILED => new ObjectResult(result) { StatusCode = 500 },
                ErrorEnumApplication.NOT_EMAIL_FPT_UNIVERSITY => new ObjectResult(result) { StatusCode = 400 },
                ErrorEnumApplication.EMAIL_IS_EXIST => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.CRENEDTIAL_IS_WRONG => new ObjectResult(result) { StatusCode = 401 },
                ErrorEnumApplication.INVALID_INPUT => new ObjectResult(result) { StatusCode = 400 },
                ErrorEnumApplication.CUSTOMER_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.STATUS_IS_ALREADY_APPLY => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.PRICE_TABLE_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.PRICE_ITEM_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.WALLET_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.GATE_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.PARKING_AREA_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.BANNED => new ObjectResult(result) { StatusCode = 403 },
                ErrorEnumApplication.DATE_OVERLAPSED => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.IN_USE => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.USER_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.UPLOAD_IMAGE_FAILED => new ObjectResult(result) { StatusCode = 500 },
                ErrorEnumApplication.CARD_IS_EXIST => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.CARD_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.CARD_IN_USE => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.PLATE_NUMBER_IN_USE => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.NOT_FOUND_OBJECT => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.GATE_TYPE_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.PLATE_NUMBER_IS_EXIST => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.VEHICLE_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.SESSION_CLOSE => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.SESSION_CANCELLED => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.NOT_ENOUGH_MONEY => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.PARKING_AREA_INACTIVE => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.PRIORITY_IS_EXIST => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.DEFAULT_PRICE_TABLE_IS_EXIST => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.DEFAULT_PRICE_TABLE_IS_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.PRICE_ITEM_IS_EXIST => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.MUST_HAVE_ONLY_ONE_PLATE_NUMBER => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.NOT_A_PLATE_NUMBER => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.FILE_EXTENSION_NOT_SUPPORT => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.CANNOT_READ_TEXT_FROM_IMAGE => new ObjectResult(result) { StatusCode = 409 },                
                ErrorEnumApplication.VEHICLE_IS_IN_SESSION => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.PARKING_AREA_IS_USING => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.DEFAULT_PRICE_ITEM_NOT_EXIST => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.VEHICLE_TYPE_IS_IN_USE => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.ACCOUNT_IS_INACTIVE => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.CAN_NOT_DELETE_DEFAULT_PRICE_TABLE => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.CAN_NOT_UPDATE_STATUS_DEFAULT_PRICE_TABLE => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.CANNOT_DELETE_VIRTUAL_GATE => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.CANNOT_DELETE_VIRTUAL_PARKING_AREA => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.NOT_FOUND_SESSION_WITH_PLATE_NUMBER => new ObjectResult(result) { StatusCode = 404 },
                ErrorEnumApplication.VEHICLE_IS_ACTIVE => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.CARD_IS_INACTIVE => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.PLATE_NUMBER_NOT_MATCH => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.CAN_NOT_CHANGE_STATUS_YOURSELF => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.CAN_NOT_DELETE_YOUR_ACCOUNT => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.PLATE_NUMBER_IS_EXIST_IN_OTHER_CARD => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.CANNOT_UPDATE_STATUS_VIRTUAL_GATE => new ObjectResult(result) { StatusCode = 409 },
                ErrorEnumApplication.NOT_FOUND_SESSION_WITH_CARD_NUMBER => new ObjectResult(result) { StatusCode = 404 },
                _ => new ObjectResult(result) { StatusCode = 500 },
            };
        }
    }
}
