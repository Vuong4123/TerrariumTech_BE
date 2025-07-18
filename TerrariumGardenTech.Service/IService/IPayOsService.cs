using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Payment;

namespace TerrariumGardenTech.Service.IService;

public interface IPayOsService
{
    Task<IBusinessResult> CreatePaymentLink(int orderId, string description);
    Task<IBusinessResult> ProcessPaymentCallback(PaymentReturnModel returnModel);
}