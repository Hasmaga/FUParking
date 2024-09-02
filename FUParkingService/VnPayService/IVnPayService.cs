using FUParkingModel.ReturnCommon;
using Microsoft.AspNetCore.Http;
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
        Task<Return<bool>> CallbackVnPayIPNUrl(IQueryCollection queryStringParameters);
    }
}
