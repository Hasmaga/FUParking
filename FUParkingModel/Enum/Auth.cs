namespace FUParkingModel.Enum
{
    public static class Auth
    {
        public static readonly List<string> AuthStaff =
        [
            RoleEnum.STAFF
        ];

        public static readonly List<string> AuthSupervisor =
        [
            RoleEnum.STAFF,
            RoleEnum.SUPERVISOR
        ];

        public static readonly List<string> AuthManager =
        [
            RoleEnum.STAFF,
            RoleEnum.SUPERVISOR,
            RoleEnum.MANAGER
        ];
    }
}
