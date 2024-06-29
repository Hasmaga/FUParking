namespace FUParkingModel.ResponseObject
{
    public class LoginResDto
    {
        public string BearerToken { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
