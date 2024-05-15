namespace FUParkingModel.Enum
{
    public static class Auth
    {
        public static readonly List<string> AuthStaff =
        [            
            RoleEnum.STAFF,
            RoleEnum.MANAGER,
            RoleEnum.SUPERVISOR
        ];

        public static readonly List<string> AuthSupervisor =
        [
            RoleEnum.MANAGER,
            RoleEnum.SUPERVISOR
        ];

        public static readonly List<string> AuthManager =
        [            
            RoleEnum.MANAGER
        ];
    }
}
