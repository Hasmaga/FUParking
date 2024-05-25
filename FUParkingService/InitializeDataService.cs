using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;

namespace FUParkingService
{
    public class InitializeDataService : IInitializeDataService
    {        
        private readonly IGateRepository _gateRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IHelpperService _helpperService;

        public InitializeDataService(IHelpperService helpperService, IGateRepository gateRepository, IVehicleRepository vehicleRepository, IRoleRepository roleRepository, IUserRepository userRepository, ICustomerRepository customerRepository)
        {
            _gateRepository = gateRepository;
            _vehicleRepository = vehicleRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _customerRepository = customerRepository;
            _helpperService = helpperService;
        }

        public async Task<Return<bool>> InitializeDatabase()
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                // Initialize data Table CustomerType
                var isCustomerTypeHadData = await _customerRepository.GetAllCustomerTypeAsync();
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
                        var isSuccessfully = await _customerRepository.CreateCustomerTypeAsync(item);
                        if (!isSuccessfully.IsSuccess)
                        {
                            transaction.Dispose();
                            return new Return<bool>
                            {
                                Data = false,
                                Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                                IsSuccess = false,
                                InternalErrorMessage = isSuccessfully.InternalErrorMessage
                            };
                        }
                    }
                }
                // Initialize data Table GateType
                var isGateTypeHadData = await _gateRepository.GetAllGateTypeAsync();
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
                        var isSuccessfully = await _gateRepository.CreateGateTypeAsync(item);
                        if (!isSuccessfully.IsSuccess)
                        {
                            transaction.Dispose();
                            return new Return<bool>
                            {
                                Data = false,
                                Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                                IsSuccess = false,
                                InternalErrorMessage = isSuccessfully.InternalErrorMessage
                            };
                        }
                    }
                }
                // Initialize data Table VehicleType
                var isVehicleTypeHadData = await _vehicleRepository.GetAllVehicleTypeAsync();
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
                        var isSuccessfully = await _vehicleRepository.CreateVehicleTypeAsync(item);
                        if (!isSuccessfully.IsSuccess)
                        {
                            transaction.Dispose();
                            return new Return<bool>
                            {
                                Data = false,
                                Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
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
                                Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                                IsSuccess = false,
                                InternalErrorMessage = isSuccessfully.InternalErrorMessage
                            };
                        }
                    }
                }
                // Initialize data Table User
                var isUserHadData = await _userRepository.GetAllUsersAsync();

                // roleStaff
                if (isUserHadData.Data == null || !isUserHadData.Data.Any())
                {
                    var roleStaff = await _roleRepository.GetRoleByNameAsync(RoleEnum.STAFF);
                    if (!roleStaff.IsSuccess || roleStaff.Data == null)
                    {
                        transaction.Dispose();
                        return new Return<bool>
                        {
                            Data = false,
                            Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = roleStaff.InternalErrorMessage
                        };
                    }
                    var passwordStaff = "Staff@123";
                    User newStaff = new()
                    {
                        Email = "staff@localhost.com",
                        PasswordHash = await _helpperService.CreatePassHashAndPassSaltAsync(passwordStaff, out byte[] passwordSaltStaff),
                        PasswordSalt = Convert.ToBase64String(passwordSaltStaff),
                        RoleId = roleStaff.Data.Id,
                        FullName = "Staff",
                        StatusUser = StatusUserEnum.ACTIVE
                    };
                    var isSuccessfullyStaff = await _userRepository.CreateUserAsync(newStaff);
                    if (!isSuccessfullyStaff.IsSuccess)
                    {
                        transaction.Dispose();
                        return new Return<bool>
                        {
                            Data = false,
                            Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = isSuccessfullyStaff.InternalErrorMessage
                        };
                    }

                    // roleManager
                    var roleManager = await _roleRepository.GetRoleByNameAsync(RoleEnum.MANAGER);
                    if (!roleManager.IsSuccess || roleManager.Data == null)
                    {
                        transaction.Dispose();
                        return new Return<bool>
                        {
                            Data = false,
                            Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = roleManager.InternalErrorMessage
                        };
                    }
                    var passwordManager = "Manager@123";
                    User newManager = new()
                    {
                        Email = "manager@localhost.com",
                        PasswordHash = await _helpperService.CreatePassHashAndPassSaltAsync(passwordManager, out byte[] passwordSaltManager),
                        PasswordSalt = Convert.ToBase64String(passwordSaltManager),
                        RoleId = roleManager.Data.Id,
                        FullName = "Manager",
                        StatusUser = StatusUserEnum.ACTIVE
                    };
                    var isSuccessfullyManager = await _userRepository.CreateUserAsync(newManager);
                    if (!isSuccessfullyManager.IsSuccess)
                    {
                        transaction.Dispose();
                        return new Return<bool>
                        {
                            Data = false,
                            Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = isSuccessfullyManager.InternalErrorMessage
                        };
                    }

                    // roleSupervisor
                    var roleSupervisor = await _roleRepository.GetRoleByNameAsync(RoleEnum.SUPERVISOR);
                    if (!roleSupervisor.IsSuccess || roleSupervisor.Data == null)
                    {
                        transaction.Dispose();
                        return new Return<bool>
                        {
                            Data = false,
                            Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = roleSupervisor.InternalErrorMessage
                        };
                    }
                    var passwordSupervisor = "Supervisor@123";
                    User newSupervisor = new()
                    {
                        Email = "supervisor@localhost.com",
                        PasswordHash = await _helpperService.CreatePassHashAndPassSaltAsync(passwordSupervisor, out byte[] passwordSaltSupervisor),
                        PasswordSalt = Convert.ToBase64String(passwordSaltSupervisor),
                        RoleId = roleSupervisor.Data.Id,
                        FullName = "Supervisor",
                        StatusUser = StatusUserEnum.ACTIVE
                    };
                    var isSuccessfullySupervisor = await _userRepository.CreateUserAsync(newSupervisor);
                    if (!isSuccessfullySupervisor.IsSuccess)
                    {
                        transaction.Dispose();
                        return new Return<bool>
                        {
                            Data = false,
                            Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = isSuccessfullySupervisor.InternalErrorMessage
                        };
                    }
                }
                transaction.Complete();
                return new Return<bool>
                {
                    Data = true,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                transaction.Dispose();
                return new Return<bool>
                {
                    Data = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
                };
            }
        }

       
    }
}
