using System.ComponentModel.DataAnnotations;

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
        [Range(0, int.MaxValue, ErrorMessage = "Price must be a positive number")]
        public required int Price { get; set; }
    }
}
