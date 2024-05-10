using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.Transactions;

namespace FUParkingService
{
    public class InitializeDataService : IInitializeDataService
    {
        private readonly ICustomerTypeRepository _customerTypeRepository;
        private readonly IGateTypeRepository _gateTypeRepository;
        private readonly IVehicleTypeRepository _vehicleTypeRepository;
        private readonly IRoleRepository _roleRepository;

        public InitializeDataService(ICustomerTypeRepository customerTypeRepository, IGateTypeRepository gateTypeRepository, IVehicleTypeRepository vehicleTypeRepository, IRoleRepository roleRepository)
        {
            _customerTypeRepository = customerTypeRepository;
            _gateTypeRepository = gateTypeRepository;
            _vehicleTypeRepository = vehicleTypeRepository;
            _roleRepository = roleRepository;
        }

        public async Task<Return<bool>> InitializeDatabase()
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                // Initialize data Table CustomerType
                var isCustomerTypeHadData = await _customerTypeRepository.GetAllCustomerType();
                if (isCustomerTypeHadData.Data == null || !isCustomerTypeHadData.Data.Any())
                {
                    var customerType = new List<CustomerType>
                    {
                        new() {
                            Name = CustomerTypeEnum.PAID,
                            Description = "Paid customer"
                        },
                        new() {
                            Name = CustomerTypeEnum.FREE,
                            Description = "Free customer"
                        }
                    };
                    foreach (var item in customerType)
                    {
                        var isSuccessfully = await _customerTypeRepository.CreateCustomerTypeAsync(item);
                        if (!isSuccessfully.IsSuccess)
                        {
                            transaction.Dispose();
                            return new Return<bool>
                            {
                                Data = false,
                                ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                                IsSuccess = false,
                                InternalErrorMessage = isSuccessfully.InternalErrorMessage
                            };
                        }
                    }
                }
                // Initialize data Table GateType
                var isGateTypeHadData = await _gateTypeRepository.GetAllGateTypeAsync();
                if (isGateTypeHadData.Data == null || !isGateTypeHadData.Data.Any())
                {
                    var gateType = new List<GateType>
                    {
                        new() {
                            Name = GateTypeEnum.IN,
                            Descriptipn = "Gate in"
                        },
                        new() {
                            Name = GateTypeEnum.OUT,
                            Descriptipn = "Gate out"
                        }
                    };
                    foreach (var item in gateType)
                    {
                        var isSuccessfully = await _gateTypeRepository.CreateGateTypeAsync(item);
                        if (!isSuccessfully.IsSuccess)
                        {
                            transaction.Dispose();
                            return new Return<bool>
                            {
                                Data = false,
                                ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                                IsSuccess = false,
                                InternalErrorMessage = isSuccessfully.InternalErrorMessage
                            };
                        }
                    }
                }
                // Initialize data Table VehicleType
                var isVehicleTypeHadData = await _vehicleTypeRepository.GetAllVehicleTypeAsync();
                if (isVehicleTypeHadData.Data == null || !isVehicleTypeHadData.Data.Any())
                {
                    var vehicleType = new List<VehicleType>
                    {
                        new() {
                            Name = VehicleTypeEnum.BICYCLE,
                            Description = "Xe đạp"
                        },
                        new()
                        {
                            Name = VehicleTypeEnum.AUTOMATIC_TRANSMISSION_MOTORCYCLE,
                            Description = "Xe Tay Ga"
                        },
                        new()
                        {
                            Name = VehicleTypeEnum.MANUAL_TRANSMISSION_MOTORCYCLE,
                            Description = "Xe Máy Số"
                        },
                        new()
                        {
                            Name = VehicleTypeEnum.ELECTRIC_BICYCLE,
                            Description = "Xe đạp điện"
                        },
                        new()
                        {
                            Name = VehicleTypeEnum.ELECTRIC_MOTORCYCLE,
                            Description = "Xe máy điện"
                        }
                    };
                    foreach (var item in vehicleType)
                    {
                        var isSuccessfully = await _vehicleTypeRepository.CreateVehicleTypeAsync(item);
                        if (!isSuccessfully.IsSuccess)
                        {
                            transaction.Dispose();
                            return new Return<bool>
                            {
                                Data = false,
                                ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                                IsSuccess = false,
                                InternalErrorMessage = isSuccessfully.InternalErrorMessage
                            };
                        }
                    }
                }
                // Initialize data Table Role
                var isRoleHadData = await _roleRepository.GetAllRoleAsync();
                if (isRoleHadData.Data == null || !isRoleHadData.Data.Any())
                {
                    var role = new List<Role>
                    {
                        new()
                        {
                            Name = RoleEnum.STAFF,
                            Description = "STAFF"
                        },
                        new()
                        {
                            Name = RoleEnum.MANAGER,
                            Description = "MANAGER"
                        },
                        new()
                        {
                            Name = RoleEnum.SUPERVISOR,
                            Description = "SUPERVISOR"
                        }
                    };
                    foreach (var item in role)
                    {
                        var isSuccessfully = await _roleRepository.CreateRoleAsync(item);
                        if (!isSuccessfully.IsSuccess)
                        {
                            transaction.Dispose();
                            return new Return<bool>
                            {
                                Data = false,
                                ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                                IsSuccess = false,
                                InternalErrorMessage = isSuccessfully.InternalErrorMessage
                            };
                        }
                    }
                }
                transaction.Complete();
                return new Return<bool>
                {
                    Data = true,
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                transaction.Dispose();
                return new Return<bool>
                {
                    Data = false,
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }
    }
}
