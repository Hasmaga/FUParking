using FUParkingModel.Enum;
using FUParkingModel.ReturnCommon;
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
    }
}
