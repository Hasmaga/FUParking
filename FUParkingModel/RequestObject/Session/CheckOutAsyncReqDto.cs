using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject.Session
{
    public class CheckOutAsyncReqDto
    {
        [Required]
        public string CardNumber { get; set; } = null!;

        [Required]
        public Guid GateOutId { get; set; }

        [Required]
        public DateTime TimeOut { get; set; }

        [Required]
        public IFormFile ImageOut { get; set; } = null!;
    }
}
