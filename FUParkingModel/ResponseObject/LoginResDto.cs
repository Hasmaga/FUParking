namespace FUParkingModel.ResponseObject
{
    public class LoginResDto
    {
        public string? BearerToken { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
