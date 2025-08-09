using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Apis.Storage.v1.ObjectsResource;
using TerrariumGardenTech.Common.RequestModel.Payment;

namespace TerrariumGardenTech.Service.IService
{
    public interface IMomoServices
    {
        Task<MomoQrResponse> CreateMomoPaymentUrl(MomoRequest request);
    }
}
