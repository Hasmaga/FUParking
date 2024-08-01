using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IHelpperService _helpperService;
        private readonly IUserRepository _userRepository;

        public PaymentService(IPaymentRepository paymentRepository, IHelpperService helpperService, IUserRepository userRepository)
        {
            _paymentRepository = paymentRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
        }

        public async Task<Return<dynamic>> ChangeStatusPaymentMethodAsync(ChangeStatusPaymentMethodReqDto req)
        {
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<dynamic>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || !userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check PaymentMethodId is exist
                var isPaymentMethodExist = await _paymentRepository.GetPaymentMethodByIdAsync(req.PaymentMethodId);
                if (isPaymentMethodExist.Data == null || !isPaymentMethodExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }

                if (req.IsActive)
                {
                    if (isPaymentMethodExist.Data.DeletedDate == null)
                    {
                        return new Return<dynamic>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    else
                    {
                        isPaymentMethodExist.Data.DeletedDate = null;
                        // Update status Account
                        var isUpdate = await _paymentRepository.UpdatePaymentMethodAsync(isPaymentMethodExist.Data);
                        if (!isUpdate.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        return new Return<dynamic> { Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, IsSuccess = true };
                    }
                }
                else
                {
                    if (isPaymentMethodExist.Data.DeletedDate != null)
                    {
                        return new Return<dynamic>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY
                        };
                    }
                    else
                    {
                        isPaymentMethodExist.Data.DeletedDate = DateTime.Now;
                        // Update status Account
                        var isUpdate = await _paymentRepository.UpdatePaymentMethodAsync(isPaymentMethodExist.Data);
                        if (!isUpdate.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                        }
                        return new Return<dynamic> { Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, IsSuccess = true };
                    }
                }
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> CreatePaymentMethodAsync(CreatePaymentMethodReqDto req)
        {
            try
            {
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
                if (userlogged.Data == null || !userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                var paymentMethod = new PaymentMethod
                {
                    Name = req.Name,
                    Description = req.Description
                };

                var result = await _paymentRepository.CreatePaymentMethodAsync(paymentMethod);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR };
                }
                return new Return<dynamic> { Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, IsSuccess = true };
            }
            catch (Exception)
            {
                return new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<PaymentMethod>>> GetAllPaymentMethodAsync()
        {
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<IEnumerable<PaymentMethod>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || !userlogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<IEnumerable<PaymentMethod>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthManager.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<PaymentMethod>> { Message = ErrorEnumApplication.NOT_AUTHORITY };
                }
                return await _paymentRepository.GetAllPaymentMethodAsync();
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<PaymentMethod>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
