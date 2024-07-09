﻿using FUParkingModel.Object;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.Transaction;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface IWalletService
    {
        Task<Return<List<Transaction>>> GetWalletTransactionByCustomerIdAsync(string? customerId, int pageIndex, int pageSize, int numberOfDate);
        Task<Return<GetWalletTransResDto>> GetTransactionWalletMainAsync(GetListObjectWithFillerDateReqDto req);
        Task<Return<GetWalletTransResDto>> GetTransactionWalletExtraAsync(GetListObjectWithFillerDateReqDto req);
    }
}
