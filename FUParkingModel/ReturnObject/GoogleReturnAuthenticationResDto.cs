namespace FUParkingModel.ReturnObject
{
    public class GoogleReturnAuthenticationResDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string GivenName { get; set; } = null!;
        public bool IsAuthentication { get; set; }
    }
}
