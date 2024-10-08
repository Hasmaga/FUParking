﻿using FUParkingModel.Enum;
using FUParkingService.MailObject;
using FUParkingService.MailService;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/mail")]
    public class MailController : Controller
    {
        private readonly IMailService _mailService;

        public MailController(IMailService mailService)
        {
            _mailService = mailService;
        }

        [HttpPost("test")]
        public async Task<IActionResult> SendMail()
        {
            try
            {
                MailRequest mailRequest = new()
                {
                    ToEmail = "phucbhse160537@fpt.edu.vn",
                    ToUsername = "Phuc Bui",
                    Subject = "Test mail",
                    Body = "Hello! Welcome back,"
                };

                await _mailService.SendEmailAsync(mailRequest);
                return Ok();
            }
            catch (Exception ex)
            {
               return BadRequest(ex.Message);
            }
        }
    }
}
