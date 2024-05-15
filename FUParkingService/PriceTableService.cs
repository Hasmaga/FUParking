using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class PriceTableService : IPriceTableService
    {
        private readonly IPriceTableRepository _priceTableRepository;
        private readonly IHelpperService _helpperService;
        private readonly IUserRepository _userRepository;
        private readonly IVehicleTypeRepository _vehicleTypeRepository;

        public PriceTableService(IPriceTableRepository priceTableRepository, IHelpperService helpperService, IUserRepository userRepository, IVehicleTypeRepository vehicleTypeRepository)
        {
            _priceTableRepository = priceTableRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
            _vehicleTypeRepository = vehicleTypeRepository;
        }

        public async Task<Return<bool>> CreatePriceTableAsync(CreatePriceTableResDto req)
        {
            try
            {
                // Check token 
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (Auth.AuthSupervisor.Contains((userlogged.Data.Role ?? new Role()).Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                // Check VehicleType is exist
                var isVehicleTypeExist = await _vehicleTypeRepository.GetVehicleTypeByIdAsync(req.VehicleTypeId);
                if (isVehicleTypeExist.Data == null || isVehicleTypeExist.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }

                var priceTable = new PriceTable
                {
                    VehicleTypeId = req.VehicleTypeId,
                    Priority = req.Priority,
                    Name = req.Name,
                    ApplyFromDate = req.ApplyFromDate ?? DefaultType.DefaultDateTime,
                    ApplyToDate = req.ApplyToDate ?? DefaultType.DefaultDateTime,
                    StatusPriceTable = StatusPriceTableEnum.ACTIVE
                };

                var result = await _priceTableRepository.CreatePriceTableAsync(priceTable);
                if (result.IsSuccess)
                {
                    return new Return<bool>
                    {
                        Data = true,
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                    };
                }
                else
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.ADD_OBJECT_ERROR
                    };
                }                
            } catch (Exception e)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
