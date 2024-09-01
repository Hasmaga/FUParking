﻿using FUParkingModel.Enum;
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
        private readonly ICardRepository _cardRepository;

        public CustomerService(ICustomerRepository customerRepository, IHelpperService helpperService, IVehicleRepository vehicleRepository, ICardRepository cardRepository)
        {
            _customerRepository = customerRepository;
            _helpperService = helpperService;
            _vehicleRepository = vehicleRepository;
            _cardRepository = cardRepository;
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

                if (req.PlateNumber != null)
                {
                    req.PlateNumber = req.PlateNumber.Trim().Replace("-", "").Replace(".", "").Replace(" ", "");
                    Regex regex = new(@"^[0-9]{2}[A-ZĐ]{1,2}[0-9]{4,6}$");
                    if (!regex.IsMatch(req.PlateNumber))
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.NOT_A_PLATE_NUMBER
                        };
                    }
                    var vehicle = new Vehicle
                    {
                        PlateNumber = req.PlateNumber,
                        CustomerId = customer.Id,
                        StatusVehicle = StatusVehicleEnum.ACTIVE,
                        VehicleTypeId = req.VehicleTypeId ?? Guid.Empty,
                        LastModifyById = checkAuth.Data.Id,                        
                    };

                    var vehicleResult = await _vehicleRepository.CreateVehicleAsync(vehicle);
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

                if (req.CardNumber != null)
                {
                    var card = new Card
                    {
                        CardNumber = req.CardNumber,
                        PlateNumber = req.PlateNumber,
                    };

                    var cardResult = await _cardRepository.CreateCardAsync(card);
                    if (!cardResult.IsSuccess)
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = cardResult.InternalErrorMessage,
                            Message = cardResult.Message
                        };
                    }
                }
                else if (req.CardId != null)
                {
                    var card = await _cardRepository.GetCardByIdAsync(req.CardId.Value);
                    if (card.Data == null || !card.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = card.InternalErrorMessage,
                            Message = card.Message
                        };
                    }

                    card.Data.PlateNumber = req.PlateNumber;
                    card.Data.LastModifyById = checkAuth.Data.Id;
                    card.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                    var cardResult = await _cardRepository.UpdateCardAsync(card.Data);
                    if (cardResult.Data == null || !cardResult.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = cardResult.InternalErrorMessage,
                            Message = cardResult.Message
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

    }
}
