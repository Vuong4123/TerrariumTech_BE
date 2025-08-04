using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Transports;

namespace TerrariumGardenTech.Service.IService
{
    public interface ITransportService
    {
        Task<(bool, string)> CreateTransport(CreateTransportModel request, int currentUserId);
        Task<(bool, string)> UpdateTransport(UpdateTransportModel request, int currentUserId);
        Task<(bool, string)> DeleteTransport(int transportId);
        Task<OrderTransport> GetById(int transportId);
        Task<IEnumerable<OrderTransport>> GetByOrderId(int orderId);
    }
}
