using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common.RequestModel.PaymentConfig;
using TerrariumGardenTech.Common.ResponseModel.PaymentConfig;

namespace TerrariumGardenTech.Service.IService;

public interface IPaymentConfigService
{
    Task<List<PaymentConfigResponse>> GetAllAsync();
    Task<PaymentConfigResponse?> GetByIdAsync(int id);
    Task<int> CreateAsync(PaymentConfigRequest request);
    Task<bool> UpdateAsync(int id, PaymentConfigRequest request);
    Task<bool> DeleteAsync(int id);
}
