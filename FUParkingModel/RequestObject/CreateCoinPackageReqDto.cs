using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class CreateCoinPackageReqDto
    {
        [Required(ErrorMessage = "Name is required")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Coin Amount is required")]
        public required int CoinAmount { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public required decimal Price { get; set; }
    }
}
