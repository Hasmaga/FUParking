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
        public const string DEFAULT_PRICE_TABLE_IS_EXIST = "This vehicle type is exist default price table";
        public const string DEFAULT_PRICE_TABLE_IS_NOT_EXIST = "This vehicle type is not exist default price table";
        public const string PRICE_ITEM_IS_EXIST = "Price item is allready exist in database";
        public const string MUST_HAVE_ONLY_ONE_PLATE_NUMBER = "Must only one plate number in image";
        public const string NOT_A_PLATE_NUMBER = "This Image not contain any plate number";
        public const string FILE_EXTENSION_NOT_SUPPORT = "File extension not support";
        public const string CANNOT_READ_TEXT_FROM_IMAGE = "Cannot read text from image";
        public const string ACCOUNT_IS_BANNED = "Account is banned";
        public const string NOT_AUTHENTICATION = "Not authentication";
        public const string ACCOUNT_IS_LOCK = "Account is lock";
        public const string VEHICLE_IS_IN_SESSION = "Vehicle is in anthor session";
        public const string PARKING_AREA_IS_USING = "Parking area is using";
        public const string DEFAULT_PRICE_ITEM_NOT_EXIST = "Default price item is not exist";
        public const string VEHICLE_TYPE_IS_IN_USE = "Vehicle Type is in use";
        public const string ACCOUNT_IS_INACTIVE = "Account is inactive";
        public const string CAN_NOT_DELETE_DEFAULT_PRICE_TABLE = "Can not delete default price table";
        public const string CAN_NOT_UPDATE_STATUS_DEFAULT_PRICE_TABLE = "Can not update status default price table";
        public const string CANNOT_DELETE_VIRTUAL_GATE = "Can not delete virual gate";
        public const string CANNOT_DELETE_VIRTUAL_PARKING_AREA = "Can not delete virtual parking area";
        public const string NOT_FOUND_SESSION_WITH_PLATE_NUMBER = "Not found session with plate number";
        public const string VEHICLE_IS_ACTIVE = "Vehicle is active";
        public const string CARD_IS_INACTIVE = "Card is inactive";
        public const string PLATE_NUMBER_NOT_MATCH = "Plate number is not match";
        public const string CAN_NOT_CHANGE_STATUS_YOURSELF = "Can not change status yourself";
        public const string CAN_NOT_DELETE_YOUR_ACCOUNT = "Can not delete your account";
        public const string PLATE_NUMBER_IS_EXIST_IN_OTHER_CARD = "Plate number is existed in other card";
        public const string CANNOT_UPDATE_STATUS_VIRTUAL_PARKING_AREA = "Cannot update status virual parking area";
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
