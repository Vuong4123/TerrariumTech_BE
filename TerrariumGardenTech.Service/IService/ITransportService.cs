using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Transports;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService
{
    public interface ITransportService
    {
        Task<(int, IEnumerable<OrderTransport>)> Paging(int? orderId, int? shipperId, TransportStatusEnum? status, bool? isRefund, int pageIndex = 1, int pageSize = 10);
        Task<IBusinessResult> CreateTransport(CreateTransportModel request, int currentUserId);
        Task<IBusinessResult> UpdateTransport(UpdateTransportModel request, int currentUserId);
        Task<IBusinessResult> DeleteTransport(int transportId);
        Task<OrderTransport> GetById(int transportId);
        Task<IEnumerable<OrderTransport>> GetByOrderId(int orderId);
    }
}
