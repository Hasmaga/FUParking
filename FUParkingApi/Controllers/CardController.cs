﻿using FUParkingModel.Enum;
using FUParkingModel.RequestObject.Card;
using FUParkingModel.ReturnCommon;
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
            try
            {
                var result = await _cardService.GetListCardAsync(req);
                if (!result.Message.Equals(SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY))
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(409, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY });
                        default:
                            _logger.LogError("Error when get list card: " + result.InternalErrorMessage);
                            return StatusCode(500, new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR });
                    }
                }
                return StatusCode(200, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error when get list card: {ex}", ex.Message);
                return StatusCode(500, new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpDelete("{CardId}")]
        [Authorize]
        public async Task<IActionResult> DeleteCardByIdAsync(Guid CardId)
        {
            try
            {
                var result = await _cardService.DeleteCardByIdAsync(CardId);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpPut("{CardId}")]
        [Authorize]
        public async Task<IActionResult> UpdatePlateNumberCard([FromBody] UpdatePlateNumberReqDto req, [FromRoute] Guid CardId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                    return StatusCode(422, new Return<Dictionary<string, List<string>?>>
                    {
                        Data = errors,
                        IsSuccess = false,
                        Message = ErrorEnumApplication.INVALID_INPUT
                    });
                }
                var result = await _cardService.UpdatePlateNumberCard(req.PlateNumber, CardId);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateNewCardAsync([FromBody] CreateNewCardReqDto req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                    return StatusCode(422, new Return<Dictionary<string, List<string>?>>
                    {
                        Data = errors,
                        IsSuccess = false,
                        Message = ErrorEnumApplication.INVALID_INPUT
                    });
                }
                var result = await _cardService.CreateNewCardAsync(req);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    switch (result.Message)
                    {
                        case ErrorEnumApplication.CARD_IS_EXIST:
                            return StatusCode(422, new Return<dynamic> { Message = ErrorEnumApplication.CARD_IS_EXIST });
                        case ErrorEnumApplication.NOT_AUTHORITY:
                            return StatusCode(409, new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY });
                        default:
                            _logger.LogError("Error when create new card: " + result.InternalErrorMessage);
                            return StatusCode(500, new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR });
                    }
                }
                return StatusCode(201, new Return<dynamic> { Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error when create new card: {ex}", ex.Message);
                return StatusCode(500, new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                });
            }
        }
    }
}
