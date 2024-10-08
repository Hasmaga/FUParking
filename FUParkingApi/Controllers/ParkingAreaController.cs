﻿using FUParkingApi.HelperClass;
using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.ParkingArea;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FUParkingApi.Controllers
{
    //[ApiController]
    [Route("api/areas")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class ParkingAreaController : Controller
    {
        private readonly IParkingAreaService _parkingAreaService;
        private readonly ILogger<ParkingAreaController> _logger;

        public ParkingAreaController(IParkingAreaService parkingAreaService, ILogger<ParkingAreaController> logger)
        {
            _parkingAreaService = parkingAreaService;
            _logger = logger;
        }        

        [HttpGet]
        public async Task<IActionResult> GetParkingAreasAsync(GetListObjectWithFiller req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _parkingAreaService.GetParkingAreasAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when get parking areas: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateParkingAreaAsync([FromBody] UpdateParkingAreaReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _parkingAreaService.UpdateParkingAreaAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when update parking area: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParkingArea([FromRoute] Guid id)
        {
            var result = await _parkingAreaService.DeleteParkingArea(id);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when delete parking area: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [Authorize]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatusParkingAreaAsync([FromBody] UpdateStatusParkingAreaReqDto req)
        {
            var result = await _parkingAreaService.UpdateStatusParkingAreaAsync(req.ParkingId, req.IsActive);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when update status parking area: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("option")]
        public async Task<IActionResult> GetParkingAreaOptionAsync()
        {
            var result = await _parkingAreaService.GetParkingAreaOptionAsync();
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when get parking area option: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateParkingAreaAndGateAsync([FromBody] CreateParkingAreaAndGateReqDto req)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(422, Helper.GetValidationErrors(ModelState));
            }
            var result = await _parkingAreaService.CreateParkingAreaAndGateAsync(req);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when get list parking area with gate: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetParkingAreaByIdAsync([FromRoute] Guid id)
        {
            var result = await _parkingAreaService.GetParkingAreaByParkingIdAsync(id);
            if (!result.IsSuccess)
            {
                if (result.InternalErrorMessage is not null)
                {
                    _logger.LogError("Error when get parking area by id: {ex}", result.InternalErrorMessage);
                }
                return Helper.GetErrorResponse(result.Message);
            }
            return StatusCode(200, result);
        }
    }
}
