using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common.RequestModel.Payment;
using TerrariumGardenTech.Common.ResponseModel.Payment;
using TerrariumGardenTech.Service.Base;
using static Google.Apis.Storage.v1.ObjectsResource;

namespace TerrariumGardenTech.Service.IService
{
    public interface IMomoServices
    {
        Task<MomoQrResponse> CreateMomoPaymentUrl(MomoRequest request);

        Task<IBusinessResult> MomoReturnExecute(IQueryCollection query);
        Task<IBusinessResult> MomoIpnExecute(MomoIpnModel body);
    }
}
