﻿using FUParkingModel.Enum;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.Transaction;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHelpperService _helpperService;

        public TransactionService(ITransactionRepository transactionRepository, IHelpperService helpperService)
        {
            _transactionRepository = transactionRepository;
            _helpperService = helpperService;
        }

        public async Task<Return<IEnumerable<GetTransactionPaymentResDto>>> GetListTransactionPaymentAsync(GetListObjectWithFillerAttributeAndDateReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetTransactionPaymentResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _transactionRepository.GetListTransactionPaymentAsync(req.PageSize, req.PageIndex, req.StartDate, req.EndDate, req.SearchInput, req.Attribute);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetTransactionPaymentResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<IEnumerable<GetTransactionPaymentResDto>>
                {
                    Data = result.Data?.Select(p => new GetTransactionPaymentResDto
                    {
                        Amount = p.Amount,
                        CreateDateTime = p.CreatedDate,
                        Email = p.Wallet?.Customer?.Email ?? "",
                        PackageName = p.Deposit?.Package?.Name ?? "",
                        PaymentMethod = p.Payment?.PaymentMethod?.Name ?? "",
                        TransactionDescription = p.TransactionDescription,
                        TransactionStatus = p.TransactionStatus,
                        WalletType = p.Wallet?.WalletType ?? "",
                    }),
                    TotalRecord = result.TotalRecord,
                    IsSuccess = true,
                    Message = result.TotalRecord > 0 ? SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetTransactionPaymentResDto>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
