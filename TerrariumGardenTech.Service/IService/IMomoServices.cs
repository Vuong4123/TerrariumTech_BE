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
        Task<MomoQrResponse> CreateMomoWalletTopupUrl(MomoWalletTopupRequest request);
        Task<IBusinessResult> MomoReturnExecute(IQueryCollection query);
        Task<IBusinessResult> MomoWalletReturnExecute(IQueryCollection query);
        Task<IBusinessResult> MomoIpnExecute(MomoIpnModel body);
        Task<IBusinessResult> MomoWalletIpnExecute(MomoIpnModel body);

        // NEW — Membership direct (không Order)
        Task<MomoQrResponse> CreateMomoMembershipDirectPaymentUrl(DirectPaymentRequest request);
        Task<IBusinessResult> MomoMembershipReturnExecute(IQueryCollection query);
        Task<IBusinessResult> MomoMembershipIpnExecute(MomoIpnModel body);
    }
}
