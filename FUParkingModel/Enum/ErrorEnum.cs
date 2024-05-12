namespace FUParkingModel.Enum
{
    public class ErrorEnumApplication
    {
        public const string SERVER_ERROR = "Server error";
        public const string ADD_OBJECT_ERROR = "Add object error";
        public const string GET_OBJECT_ERROR = "Get object error";
        public const string UPDATE_OBJECT_ERROR = "Update object error";
        public const string GOOGLE_LOGIN_FAILED = "Google login failed";
        public const string NOT_EMAIL_FPT_UNIVERSITY = "Only email FPT University is allow";
        public const string NOT_AUTHORITY = "You are not authority to use this function";
        public const string EMAIL_IS_EXIST = "This email is already exist";
        public const string CRENEDTIAL_IS_WRONG = "Credential is wrong";
    }

    public class MinioErrorServerDefineEnum
    {
        public const string NOT_FOUND_MINIO_SERVER = "Exception of type 'Minio.Exceptions.ObjectNotFoundException' was thrown.";
        public const string NOT_FOUND_OBJECT = "Object not found";
    }

    public class MinioErrorApplicationDefineEnum
    {
        public const string NOT_FOUND = "Not found";
        public const string FILE_NAME_IS_EXIST = "File name is exist";
    }    

    public class LoginEnum
    {
        public const string Name = "NO_NAME";
        public const string Email = "NO_EMAIL";
        public const string GivenName = "NO_GIVEN_NAME";
    }
}
