using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Customer;
using FUParkingModel.ResponseObject.Customer;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHelpperService _helpperService;

        public CustomerService(ICustomerRepository customerRepository, IUserRepository userRepository, IHelpperService helpperService)
        {
            _customerRepository = customerRepository;
            _userRepository = userRepository;
            _helpperService = helpperService;
        }

        public async Task<Return<dynamic>> ChangeStatusCustomerAsync(ChangeStatusCustomerReqDto req)
        {
            try
            {
                // Check token 
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || !userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || userlogged.Data.Role == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains((userlogged.Data.Role).Name))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                // Check CustomerId is exist
                var isCustomerExist = await _customerRepository.GetCustomerByIdAsync(req.CustomerId);
                if (isCustomerExist.Data == null || !isCustomerExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
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
                        // Update status Account
                        var isUpdate = await _customerRepository.UpdateCustomerAsync(isCustomerExist.Data);
                        if (isUpdate.Data == null || !isUpdate.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            return new Return<dynamic>
                            {
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
                        isCustomerExist.Data.StatusCustomer = StatusCustomerEnum.INACTIVE;
                        // Update status Account
                        var isUpdate = await _customerRepository.UpdateCustomerAsync(isCustomerExist.Data);
                        if (isUpdate.Data == null || !isUpdate.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            return new Return<dynamic>
                            {                                
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
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || !userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || userlogged.Data.Role == null)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                if (!Auth.AuthSupervisor.Contains((userlogged.Data.Role).Name))
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                if (userlogged.Data.StatusUser.ToLower().Equals(StatusUserEnum.INACTIVE))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }
                Return<Customer> foundCustomerRes = await _customerRepository.GetCustomerByEmailAsync(customerReq.Email);
                if (foundCustomerRes.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    res.Message = ErrorEnumApplication.EMAIL_IS_EXIST;
                    return res;
                }
                Return<CustomerType> typeFreeRes = await _customerRepository.GetCustomerTypeByNameAsync(CustomerTypeEnum.FREE);
                if (!typeFreeRes.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || typeFreeRes.Data == null)
                {                    
                    return res;
                }
                Customer newCustomer = new()
                {
                    FullName = customerReq.Name,
                    StatusCustomer = StatusCustomerEnum.ACTIVE,
                    CustomerTypeId = typeFreeRes.Data.Id,
                    Email = customerReq.Email,
                };
                var result = await _customerRepository.CreateNewCustomerAsync(newCustomer);
                if (result.Data == null || !result.Data.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
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
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || !userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || userlogged.Data.Role == null)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                if (!Auth.AuthSupervisor.Contains((userlogged.Data.Role).Name))
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                if (userlogged.Data.StatusUser.ToLower().Equals(StatusUserEnum.INACTIVE))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }
                var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
                res.Data = new GetCustomersWithFillerResDto()
                {
                    StatusCustomer = customer.Data?.StatusCustomer ?? "",
                    CustomerType = customer.Data?.CustomerType?.Name ?? "",
                    Email = customer.Data?.Email ?? "",
                    FullName = customer.Data?.FullName ?? "",
                    CreateDate = DateOnly.FromDateTime(customer.Data?.CreatedDate ?? DateTime.Now),
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
                Message = ErrorEnumApplication.SERVER_ERROR,
                IsSuccess = false
            };
            try
            {
                if (!_helpperService.IsTokenValid())
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                var userLogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userLogged.Data == null || userLogged.IsSuccess == false)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }

                if (!Auth.AuthManager.Contains(userLogged.Data.Role?.Name ?? ""))
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }

                if (userLogged.Data.StatusUser.Equals(StatusUserEnum.INACTIVE))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }

                var listCustomerRes = await _customerRepository.GetListCustomerAsync(req);
                IEnumerable<GetCustomersWithFillerResDto> listCustomer = [];
                foreach (var item in listCustomerRes.Data ?? [])
                {
                    _ = listCustomer.Append(new GetCustomersWithFillerResDto
                    {
                        CustomerId = item.Id,
                        FullName = item.FullName,
                        Email = item.Email,
                        StatusCustomer = item.StatusCustomer,
                        CustomerType = item.CustomerType?.Name ?? "",
                        CreateDate = DateOnly.FromDateTime(item.CreatedDate)
                    });
                }
                res.Data = listCustomer;
                res.TotalRecord = listCustomer.Count();
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
    }
}
