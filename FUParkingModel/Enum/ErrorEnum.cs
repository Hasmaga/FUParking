using System.Data;

namespace FUParkingModel.Enum
{
    public class ErrorEnumApplication
    {
        public const string SERVER_ERROR = "Server error";
        public const string ADD_OBJECT_ERROR = "Add object error";
        public const string OBJECT_EXISTED = "Object is existed";
        public const string GET_OBJECT_ERROR = "Get object error";
        public const string UPDATE_OBJECT_ERROR = "Update object error";
        public const string DELETE_OBJECT_ERROR = "Delete object error";
        public const string GOOGLE_LOGIN_FAILED = "Google login failed";
        public const string NOT_EMAIL_FPT_UNIVERSITY = "Only email FPT University is allow";
        public const string NOT_AUTHORITY = "You are not authority to use this function";
        public const string EMAIL_IS_EXIST = "This email is already exist";
        public const string CRENEDTIAL_IS_WRONG = "Credential is wrong";
        public const string INVALID_INPUT = "Invalid input data";
        public const string CUSTOMER_NOT_EXIST = "Customr not exist in the system";
        public const string STATUS_IS_ALREADY_APPLY = "Status is already apply in the system";
        public const string VEHICLE_TYPE_NOT_EXIST = "Vehicle type not exist in application";
        public const string PRICE_TABLE_NOT_EXIST = "Price table is not exist in application";
        public const string PRICE_ITEM_NOT_EXIST = "Price item is not exist in application";
        public const string PACKAGE_NOT_EXIST = "Package is not exist";
        public const string WALLET_NOT_EXIST = "Wallet not exist";
        public const string GATE_NOT_EXIST = "Gate not exist";
        public const string PARKING_AREA_NOT_EXIST = "Parking Area is not exist";
        public const string BANNED = "User is inactive";
        public const string DATE_OVERLAPSED = "Date is overlapsed";
        public const string IN_USE = "This object is in use";
        public const string USER_NOT_EXIST = "User is not exist";
        public const string UPLOAD_IMAGE_FAILED = "Upload image fail";
        public const string CARD_IS_EXIST = "Card Number is exist in system";
        public const string CARD_NOT_EXIST = "Card is not exist in system";
        public const string CARD_IN_USE = "This card is already use";
        public const string PLATE_NUMBER_IN_USE = "This plate number is already in system";
        public const string NOT_FOUND_OBJECT = "Not found object";
        public const string GATE_TYPE_NOT_EXIST = "Gate type is not exist in system";
        public const string PLATE_NUMBER_IS_EXIST = "Plate Number is exist in system";
        public const string VEHICLE_NOT_EXIST = "Vehicle is not exist in system";
        public const string SESSION_CLOSE = "This session is allready close";
        public const string SESSION_CANCELLED = "This session is canceled";
        public const string NOT_ENOUGH_MONEY = "This account is not have enough money in account";
        public const string PARKING_AREA_INACTIVE = "This parking area is inactive";
        public const string PRIORITY_IS_EXIST = "This priority of vehicle type is exist";
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
