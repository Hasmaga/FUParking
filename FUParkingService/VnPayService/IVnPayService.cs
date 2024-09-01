using FUParkingModel.ReturnCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingService.VnPayService
{
    public interface IVnPayService
    {
        Task<Return<dynamic>> CustomerCreateRequestBuyPackageByVnPayAsync(Guid packageId, IPAddress ipAddress);
        Task<Return<bool>> CallbackVnPayIPNUrl(string vnp_TmnCode, string vnp_Amount, string vnp_BankCode, string vnp_OrderInfo, string vnp_TransactionNo, string vnp_ResponseCode, string vnp_TransactionStatus, Guid vnp_TxnRef, string vnp_SecureHash);
    }
}
