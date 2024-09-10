using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Customer;
using FUParkingModel.ResponseObject.Customer;
using FUParkingModel.ResponseObject.Session;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.Text.RegularExpressions;
using System.Transactions;

namespace FUParkingService
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;        
        private readonly IHelpperService _helpperService;
        private readonly IVehicleRepository _vehicleRepository;        

        public CustomerService(ICustomerRepository customerRepository, IHelpperService helpperService, IVehicleRepository vehicleRepository)
        {
            _customerRepository = customerRepository;
            _helpperService = helpperService;
            _vehicleRepository = vehicleRepository;            
        }

        public async Task<Return<dynamic>> ChangeStatusCustomerAsync(ChangeStatusCustomerReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check CustomerId is exist
                var isCustomerExist = await _customerRepository.GetCustomerByIdAsync(req.CustomerId);
                if (isCustomerExist.Data == null || !isCustomerExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isCustomerExist.InternalErrorMessage,
                        Message = ErrorEnumApplication.CUSTOMER_NOT_EXIST
                    };
                }
                if (req.IsActive)
                {
                    if ((isCustomerExist.Data.StatusCustomer ?? "").Equals(StatusCustomerEnum.ACTIVE))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    else
                    {
                        isCustomerExist.Data.StatusCustomer = StatusCustomerEnum.ACTIVE;
                        isCustomerExist.Data.LastModifyById = checkAuth.Data.Id;
                        isCustomerExist.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                        // Update status Account
                        var isUpdate = await _customerRepository.UpdateCustomerAsync(isCustomerExist.Data);
                        if (isUpdate.Data == null || !isUpdate.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            return new Return<dynamic>
                            {
                                InternalErrorMessage = isUpdate.InternalErrorMessage,
                                Message = ErrorEnumApplication.SERVER_ERROR
                            };
                        }
                        return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
                    }
                }
                else
                {
                    if ((isCustomerExist.Data.StatusCustomer ?? "").Equals(StatusCustomerEnum.INACTIVE))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    else
                    {
                        isCustomerExist.Data.CreatedById = checkAuth.Data.Id;
                        isCustomerExist.Data.CreatedDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                        isCustomerExist.Data.StatusCustomer = StatusCustomerEnum.INACTIVE;
                        // Update status Account
                        var isUpdate = await _customerRepository.UpdateCustomerAsync(isCustomerExist.Data);
                        if (isUpdate.Data == null || !isUpdate.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            return new Return<dynamic>
                            {
                                InternalErrorMessage = isUpdate.InternalErrorMessage,
                                Message = ErrorEnumApplication.SERVER_ERROR
                            };
                        }
                        return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
                    }
                }
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<dynamic>> CreateCustomerAsync(CustomerReqDto customerReq)
        {
            Return<dynamic> res = new() { Message = ErrorEnumApplication.SERVER_ERROR };
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    res.InternalErrorMessage = checkAuth.InternalErrorMessage;
                    res.Message = checkAuth.Message;
                    return res;
                }
                Return<Customer> foundCustomerRes = await _customerRepository.GetCustomerByEmailAsync(customerReq.Email);
                if (!foundCustomerRes.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    res.InternalErrorMessage = foundCustomerRes.InternalErrorMessage;
                    res.Message = ErrorEnumApplication.EMAIL_IS_EXIST;
                    return res;
                }
                Return<CustomerType> typeFreeRes = await _customerRepository.GetCustomerTypeByNameAsync(CustomerTypeEnum.FREE);
                if (!typeFreeRes.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || typeFreeRes.Data == null)
                {
                    res.InternalErrorMessage = typeFreeRes.InternalErrorMessage;
                    return res;
                }
                Customer newCustomer = new()
                {
                    FullName = customerReq.Name,
                    StatusCustomer = StatusCustomerEnum.ACTIVE,
                    CustomerTypeId = typeFreeRes.Data.Id,
                    Email = customerReq.Email,
                    CreatedById = checkAuth.Data.Id
                };
                var result = await _customerRepository.CreateNewCustomerAsync(newCustomer);
                if (result.Data == null || !result.Data.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    res.InternalErrorMessage = result.InternalErrorMessage;
                    return res;
                }
                res.Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY;
                res.IsSuccess = true;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
                return res;
            }
        }

        public async Task<Return<GetCustomersWithFillerResDto>> GetCustomerByIdAsync(Guid customerId)
        {
            Return<GetCustomersWithFillerResDto> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    res.InternalErrorMessage = checkAuth.InternalErrorMessage;
                    res.Message = checkAuth.Message;
                    return res;
                }
                var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
                res.Data = new GetCustomersWithFillerResDto()
                {
                    StatusCustomer = customer.Data?.StatusCustomer ?? "",
                    CustomerType = customer.Data?.CustomerType?.Name ?? "",
                    Email = customer.Data?.Email ?? "",
                    FullName = customer.Data?.FullName ?? "",
                    CreateDate = DateOnly.FromDateTime(customer.Data?.CreatedDate ?? TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
                    CustomerId = customer.Data?.Id ?? Guid.Empty,
                };
                res.Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY;
                res.IsSuccess = true;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
                return res;
            }
        }

        public async Task<Return<IEnumerable<GetCustomersWithFillerResDto>>> GetListCustomerAsync(GetCustomersWithFillerReqDto req)
        {
            Return<IEnumerable<GetCustomersWithFillerResDto>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    res.InternalErrorMessage = checkAuth.InternalErrorMessage;
                    res.Message = checkAuth.Message;
                    return res;
                }
                var listCustomerRes = await _customerRepository.GetListCustomerAsync(req);
                if (!listCustomerRes.IsSuccess)
                {
                    res.InternalErrorMessage = listCustomerRes.InternalErrorMessage;
                    return res;
                }
                res.Data = listCustomerRes.Data?.Select(b => new GetCustomersWithFillerResDto
                {
                    CustomerId = b.Id,
                    Email = b.Email,
                    CreateDate = DateOnly.FromDateTime(b.CreatedDate),
                    CustomerType = b.CustomerType?.Name ?? "",
                    FullName = b.FullName,
                    StatusCustomer = b.StatusCustomer
                }).ToList();
                res.TotalRecord = listCustomerRes.TotalRecord;
                res.Message = listCustomerRes.Data?.Count() > 0 ? SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY : ErrorEnumApplication.NOT_FOUND_OBJECT;
                res.IsSuccess = true;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
                return res;
            }
        }

        public async Task<Return<StatisticCustomerResDto>> StatisticCustomerAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<StatisticCustomerResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _customerRepository.StatisticCustomerAsync();
                if (!result.IsSuccess)
                {
                    return new Return<StatisticCustomerResDto>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                return new Return<StatisticCustomerResDto>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<StatisticCustomerResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<bool>> UpdateCustomerAccountAsync(UpdateCustomerAccountReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                // update account that are calling this medthod
                var account = checkAuth.Data;

                if (account.FullName.Equals(req.FullName))
                {
                    return new Return<bool>
                    {
                        Data = true,
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                    };
                }

                account.FullName = req.FullName;

                var result = await _customerRepository.UpdateCustomerAsync(account);
                if (result.Data == null || !result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<bool> 
                { 
                    Data = true,
                    IsSuccess = true, 
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY 
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<dynamic>> CreateNonPaidCustomerAsync(CreateNonPaidCustomerReqDto req)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                // Check email is exist
                var isEmailCustomerExist = await _customerRepository.GetCustomerByEmailAsync(req.Email);
                if (isEmailCustomerExist.Data != null && isEmailCustomerExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.EMAIL_IS_EXIST
                    };
                }

                var customerType = await _customerRepository.GetCustomerTypeByNameAsync(CustomerTypeEnum.FREE);
                if (!customerType.IsSuccess || customerType.Data is null)
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = customerType.InternalErrorMessage,
                        Message = customerType.Message
                    };
                }

                var customer = new Customer
                {
                    FullName = req.Name,
                    Email = req.Email,
                    StatusCustomer = StatusCustomerEnum.ACTIVE,
                    CustomerTypeId = customerType.Data.Id,
                    CreatedById = checkAuth.Data.Id
                };

                var result = await _customerRepository.CreateNewCustomerAsync(customer);
                if (!result.IsSuccess)
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {                        
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }

                foreach (var vehicle in req.Vehicles ?? [])
                {
                    if (string.IsNullOrEmpty(vehicle.PlateNumber) && vehicle.PlateNumber == null)
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.NOT_A_PLATE_NUMBER,
                        };
                    }
                    // Check Plate Number is valid
                    vehicle.PlateNumber = vehicle.PlateNumber.Trim().Replace("-", "").Replace(".", "").Replace(" ", "");
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                    Regex regex = new(@"^[0-9]{2}[A-ZĐ]{1,2}[0-9]{4,6}$");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                    if (!regex.IsMatch(vehicle.PlateNumber))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.NOT_A_PLATE_NUMBER
                        };
                    }
                    // Check plate number is exist
                    var isPlateNumberExist = await _vehicleRepository.GetVehicleByPlateNumberAsync(vehicle.PlateNumber);
                    if (isPlateNumberExist.Data != null && isPlateNumberExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.PLATE_NUMBER_IS_EXIST
                        };
                    }

                    var newVehicle = new Vehicle
                    {
                        PlateNumber = vehicle.PlateNumber,
                        VehicleTypeId = vehicle.VehicleTypeId,
                        CustomerId = customer.Id,
                        StatusVehicle = StatusVehicleEnum.ACTIVE,
                        StaffId = checkAuth.Data.Id
                    };

                    var vehicleResult = await _vehicleRepository.CreateVehicleAsync(newVehicle);
                    if (!vehicleResult.IsSuccess)
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = vehicleResult.InternalErrorMessage,
                            Message = vehicleResult.Message
                        };
                    }
                }
                
                scope.Complete();
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                scope.Dispose();
                return new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }


        public async Task<Return<GetCustomerTypeByPlateNumberResDto>> GetCustomerTypeByPlateNumberAsync(string PlateNumber)
        {
            try
            {
                // Check Auth
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<GetCustomerTypeByPlateNumberResDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check Input Plate Number
                if (string.IsNullOrEmpty(PlateNumber) && PlateNumber == null)
                {
                    return new Return<GetCustomerTypeByPlateNumberResDto>
                    {
                        Message = ErrorEnumApplication.NOT_A_PLATE_NUMBER,
                    };
                }
                // Check Plate Number is valid
                PlateNumber = PlateNumber.Trim().Replace("-", "").Replace(".", "").Replace(" ", "");
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                Regex regex = new(@"^[0-9]{2}[A-ZĐ]{1,2}[0-9]{4,6}$");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                if (!regex.IsMatch(PlateNumber))
                {
                    return new Return<GetCustomerTypeByPlateNumberResDto>
                    {
                        Message = ErrorEnumApplication.NOT_A_PLATE_NUMBER
                    };
                }
                // Get Customer Type By Plate Number
                var result = await _customerRepository.GetCustomerByPlateNumberAsync(PlateNumber);
                if (!result.IsSuccess)
                {
                    return new Return<GetCustomerTypeByPlateNumberResDto>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                if (result.Data == null)
                {
                    return new Return<GetCustomerTypeByPlateNumberResDto>
                    {
                        Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                        Data = new GetCustomerTypeByPlateNumberResDto
                        {
                            CustomerType = CustomerTypeEnum.GUEST
                        },
                        IsSuccess = true
                    };
                }

                if (result.Data.CustomerType?.Name.Equals(CustomerTypeEnum.PAID) ?? false)
                {
                    return new Return<GetCustomerTypeByPlateNumberResDto>
                    {
                        Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                        Data = new GetCustomerTypeByPlateNumberResDto
                        {
                            CustomerType = CustomerTypeEnum.PAID
                        },
                        IsSuccess = true
                    };
                }
                return new Return<GetCustomerTypeByPlateNumberResDto>
                {
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    Data = new GetCustomerTypeByPlateNumberResDto
                    {
                        CustomerType = CustomerTypeEnum.FREE
                    },
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new Return<GetCustomerTypeByPlateNumberResDto> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = ex };
            }
        }

        public async Task<Return<bool>> UpdateCustomerFCMTokenAsync(string fcmToken)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateCustomerAsync();
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var account = checkAuth.Data;

                if (account.FCMToken is not null && account.FCMToken.Equals(fcmToken))
                {
                    return new Return<bool>
                    {
                        Data = true,
                        IsSuccess = true,
                        Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                    };
                }

                account.FCMToken = fcmToken;

                var result = await _customerRepository.UpdateCustomerAsync(account);
                if (result.Data == null || !result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<bool>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<bool>
                {
                    Data = true,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<dynamic>> DeleteCustomerByStaff(Guid id)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var customer = await _customerRepository.GetCustomerByIdAsync(id);
                if (customer.Data == null || !customer.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = customer.InternalErrorMessage,
                        Message = ErrorEnumApplication.CUSTOMER_NOT_EXIST
                    };
                }

                customer.Data.DeletedDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                customer.Data.LastModifyById = checkAuth.Data.Id;
                customer.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                // Get list vehicle own by user
                var listVehicle = await _vehicleRepository.GetAllCustomerVehicleByCustomerIdAsync(id);
                if (listVehicle.Data == null || !listVehicle.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = listVehicle.InternalErrorMessage,
                        Message = listVehicle.Message
                    };
                }

                foreach (var vehicle in listVehicle.Data)
                {
                    vehicle.DeletedDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                    vehicle.LastModifyById = checkAuth.Data.Id;
                    vehicle.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                    var resultDelete = await _vehicleRepository.UpdateVehicleAsync(vehicle);
                    if (resultDelete.Data == null || !resultDelete.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = resultDelete.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                }
                var result = await _customerRepository.UpdateCustomerAsync(customer.Data);
                if (result.Data == null || !result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                scope.Complete();
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<dynamic>> UpdateInformationCustomerByStaff(UpdateInformationCustomerResDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var customer = await _customerRepository.GetCustomerByIdAsync(req.CustomerId);
                if (customer.Data == null || !customer.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = customer.InternalErrorMessage,
                        Message = ErrorEnumApplication.CUSTOMER_NOT_EXIST
                    };
                }

                if (req.CustomerTypeId != null)
                {
                    var customerType = await _customerRepository.GetCustomerTypeByIdAsync(req.CustomerTypeId.Value);
                    if (customerType.Data == null || !customerType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    {
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = customerType.InternalErrorMessage,
                            Message = customerType.Message
                        };
                    }
                    customer.Data.CustomerTypeId = customerType.Data.Id;
                }

                if (req.FullName != null)
                {
                    customer.Data.FullName = req.FullName;
                }

                if (req.Email != null) {
                    var isEmailCustomerExist = await _customerRepository.GetCustomerByEmailAsync(req.Email);
                    if (isEmailCustomerExist.Data != null && isEmailCustomerExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.EMAIL_IS_EXIST
                        };
                    }
                    customer.Data.Email = req.Email;
                }

                customer.Data.LastModifyById = checkAuth.Data.Id;
                customer.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var result = await _customerRepository.UpdateCustomerAsync(customer.Data);
                if (result.Data == null || !result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<IEnumerable<GetCustomerTypeOptionResDto>>> GetCustomerTypeOptionAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetCustomerTypeOptionResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _customerRepository.GetAllCustomerTypeAsync();
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetCustomerTypeOptionResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = result.Message
                    };
                }
                return new Return<IEnumerable<GetCustomerTypeOptionResDto>>
                {
                    Data = result.Data?.Select(b => new GetCustomerTypeOptionResDto
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Description = b.Description
                    }),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    TotalRecord = result.TotalRecord
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetCustomerTypeOptionResDto>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }
    }
}
