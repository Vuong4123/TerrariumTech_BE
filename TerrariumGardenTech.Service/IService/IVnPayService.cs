using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Common.RequestModel.Payment;
using TerrariumGardenTech.Common.ResponseModel;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface IVnPayService
{
    Task<IBusinessResult> CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
    Task<IBusinessResult> PaymentExecute(IQueryCollection collections);
}