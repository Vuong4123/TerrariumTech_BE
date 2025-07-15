using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService
{
    public interface IOrderItemDetailService
    {
        Task<IBusinessResult> CreateOrderItemDetailAsync(int orderItemId, string detailKey, string detailValue, int quantity, decimal unitPrice);
        Task<IBusinessResult> GetOrderItemDetailsByOrderItemIdAsync(int orderItemId);
        Task<IBusinessResult> UpdateOrderItemDetailAsync(int orderItemDetailId, string detailKey, string detailValue, int quantity, decimal unitPrice);
        Task<IBusinessResult> DeleteOrderItemDetailAsync(int orderItemDetailId);
    }
}
