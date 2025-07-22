using TerrariumGardenTech.Common.RequestModel.Payment;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface IPayOsService
{
    Task<IBusinessResult> CreatePaymentLink(int orderId, string description);
    Task<IBusinessResult> ProcessPaymentCallback(PaymentReturnModel returnModel);
}