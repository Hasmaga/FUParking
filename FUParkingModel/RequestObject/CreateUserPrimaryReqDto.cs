namespace FUParkingModel.RequestObject
{
    public class CreateUserPrimaryReqDto
    {
        public Guid RoleId { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public Guid CreateById { get; set; }
    }
}
