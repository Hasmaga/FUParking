namespace FUParkingModel.ResponseObject.Customer
{
    public class LoginWithGoogleMoblieResDto
    {
        public string BearerToken { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string CustomerType { get; set; } = null!;
    }
}
