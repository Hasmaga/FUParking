using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject.Card;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [Route("api/cards")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class CardController : Controller
    {
        private readonly ICardService _cardService;
        private readonly ILogger<CardController> _logger;

        public CardController(ICardService cardService, ILogger<CardController> logger)
        {
            _cardService = cardService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetListCardAsync(GetCardsWithFillerReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _cardService.GetListCardAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when get list card: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpDelete("{CardId}")]
        [Authorize]
        public async Task<IActionResult> DeleteCardByIdAsync(Guid CardId)
        {
            var result = await _cardService.DeleteCardByIdAsync(CardId);
            if (!result.Message.Equals(SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when delete card by id: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPut("{CardId}")]
        [Authorize]
        public async Task<IActionResult> UpdatePlateNumberInCardAsync([FromBody] UpdatePlateNumberReqDto req, [FromRoute] Guid CardId)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _cardService.UpdatePlateNumberInCardAsync(req.PlateNumber, CardId);
            if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when update plate number in card: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateNewCardAsync([FromBody] CreateNewCardReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _cardService.CreateNewCardAsync(req);
            if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when create new card: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPut("status/{CardId}/missing")]
        [Authorize]
        public async Task<IActionResult> ChangeStatusCardToMissingAsync(Guid CardId)
        {
            var result = await _cardService.ChangeStatusCardToMissingAsync(CardId);
            if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when change status card to missing: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPut("status/{CardId}/{isActive}")]
        [Authorize]
        public async Task<IActionResult> ChangeStatusCardAsync(Guid CardId, bool isActive)
        {
            var result = await _cardService.ChangeStatusCardAsync(CardId, isActive);
            if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when change status card: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }
    }
}
