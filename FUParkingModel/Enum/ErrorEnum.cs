namespace FUParkingModel.Enum
{
    public class ErrorEnumApplication
    {
        public const string SERVER_ERROR = "Server error";
        public const string ADD_OBJECT_ERROR = "Add object error";
        public const string GET_OBJECT_ERROR = "Get object error";
        public const string UPDATE_OBJECT_ERROR = "Update object error";
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
}
