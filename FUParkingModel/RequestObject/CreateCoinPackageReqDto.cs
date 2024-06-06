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
        [Range(0, int.MaxValue, ErrorMessage = "Coin Amount must be a positive number")]
        public required int CoinAmount { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Extra Coin must be a positive number")]
        public int? ExtraCoin { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "EXP Package must be a positive number")]
        public int? EXPPackage { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        public required decimal Price { get; set; }
    }
}
