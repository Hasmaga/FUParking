using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
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

        public async Task<Return<bool>> ChangeStatusCustomerAsync(ChangeStatusCustomerReqDto req)
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
                if (Auth.AuthStaff.Contains((userlogged.Data.Role ?? new Role()).Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                // Check CustomerId is exist
                var isCustomerExist = await _customerRepository.GetCustomerByIdAsync(req.CustomerId);
                if (isCustomerExist.Data == null || isCustomerExist.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.CUSTOMER_NOT_EXIST
                    };
                }
                if (req.IsActive)
                {
                    if ((isCustomerExist.Data.StatusCustomer ?? "").Equals(StatusCustomerEnum.ACTIVE))
                    {
                        return new Return<bool>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    else
                    {
                        isCustomerExist.Data.StatusCustomer = StatusCustomerEnum.ACTIVE;
                        // Update status Account
                        var isUpdate = await _customerRepository.UpdateCustomerAsync(isCustomerExist.Data);
                        if (isUpdate.Data == null || isUpdate.IsSuccess == false)
                        {
                            return new Return<bool>
                            {
                                IsSuccess = false,
                                Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR
                            };
                        }
                        else
                        {
                            return new Return<bool> { IsSuccess = true, Data = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
                        }
                    }
                }
                else
                {
                    if ((isCustomerExist.Data.StatusCustomer ?? "").Equals(StatusCustomerEnum.INACTIVE))
                    {
                        return new Return<bool>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    else
                    {
                        isCustomerExist.Data.StatusCustomer = StatusCustomerEnum.ACTIVE;
                        // Update status Account
                        var isUpdate = await _customerRepository.UpdateCustomerAsync(isCustomerExist.Data);
                        if (isUpdate.Data == null || isUpdate.IsSuccess == false)
                        {
                            return new Return<bool>
                            {
                                IsSuccess = false,
                                Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR
                            };
                        }
                        else
                        {
                            return new Return<bool> { IsSuccess = true, Data = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex.Message
                };
            }
        }
    }
}
