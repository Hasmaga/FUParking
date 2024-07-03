using System.Runtime.CompilerServices;

namespace FUParkingModel.Enum
{
    public class StatusCustomerEnum
    {
        public const string ACTIVE = "ACTIVE";
        public const string INACTIVE = "INACTIVE";
    }

    public class StatusVehicleEnum
    {
        public const string PENDING = "PENDING";
        public const string ACTIVE = "ACTIVE";        
        public const string INACTIVE = "INACTIVE";
    }

    public class StatusPriceTableEnum
    {
        public const string ACTIVE = "ACTIVE";
        public const string INACTIVE = "INACTIVE";
    }

    public class StatusWalletEnum
    {
        public const string ACTIVE = "ACTIVE";
        public const string INACTIVE = "INACTIVE";
    }

    public class StatusPackageEnum
    {
        public const string ACTIVE = "ACTIVE";
        public const string INACTIVE = "INACTIVE";
    }

    public class StatusTransactionEnum
    {
        public const string PENDING = "PENDING";
        public const string SUCCEED = "SUCCEED";
        public const string FAILED = "FAILED";
    }

    public class DescriptionTransactionEnum
    {
        public const string DENY_TRANSACTION = "DENY_TRANSACTION";
        public const string DONT_HAVE_ENOUGH_COIN = "DONT_HAVE_ENOUGH_COIN";
    }

    public class StatusParkingEnum
    {
        public const string ACTIVE = "ACTIVE";
        public const string INACTIVE = "INACTIVE";
    }

    public class StatusGateEnum
    {
        public const string ACTIVE = "ACTIVE";
        public const string INACTIVE = "INACTIVE";
    }

    public class StatusPaymentEnum
    {
        public const string PENDING = "PENDING";
        public const string SUCCESSED = "SUCCESSED";
        public const string FAILED = "FAILED";
    }

    public class CustomerTypeEnum
    {
        public const string PAID = "PAID";
        public const string FREE = "FREE";
    }

    public class GateTypeEnum
    {
        public const string IN = "IN";
        public const string OUT = "OUT";
    }

    public class VehicleTypeEnum
    {
        public const string MANUAL_TRANSMISSION_MOTORCYCLE = "Manual Transmission Motorcycle";
        public const string AUTOMATIC_TRANSMISSION_MOTORCYCLE = "Automatic Transmission Motorcycle";
        public const string ELECTRIC_MOTORCYCLE = "Electric Motorcycle";
        public const string ELECTRIC_BICYCLE = "Electric Bicycle";
        public const string BICYCLE = "Bicycle";
    }

    public class RoleEnum
    {
        public const string STAFF = "STAFF";
        public const string SUPERVISOR = "SUPERVISOR";
        public const string MANAGER = "MANAGER";
    }

    public class StatusUserEnum
    {
        public const string ACTIVE = "ACTIVE";
        public const string INACTIVE = "INACTIVE";
    }

    public class DefaultType
    {
        public readonly static DateTime DefaultDateTime = new(0, 0, 0);
        public readonly static TimeOnly DefaultTimeOnly = new(0, 0);
    }

    public class WalletType
    {
        public const string MAIN = "MAIN";
        public const string EXTRA = "EXTRA";
    }

    public class PaymentMethods
    {
        public const string CASH = "CASH";
        public const string WALLET = "WALLET";
        public const string ZALOPAY = "ZALOPAY";
    }

    public class SessionEnum
    {
        public const string PARKED = "PARKED";
        public const string CLOSED = "CLOSED";
        public const string CANCELLED = "CANCELLED";
    }
}
