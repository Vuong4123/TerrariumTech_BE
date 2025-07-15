using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class OrderItemDetailService : IOrderItemDetailService
    {
        private readonly UnitOfWork _unitOfWork;

        public OrderItemDetailService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Tạo OrderItemDetail mới
        public async Task<IBusinessResult> CreateOrderItemDetailAsync(int orderItemId, string detailKey, string detailValue, int quantity, decimal unitPrice)
        {
            var orderItemDetail = new OrderItemDetail
            {
                OrderItemId = orderItemId,
                DetailKey = detailKey,
                DetailValue = detailValue,
                Quantity = quantity,
                UnitPrice = unitPrice
            };

            await _unitOfWork.OrderItemDetailRepository.CreateAsync(orderItemDetail);
            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Tạo chi tiết đơn hàng thành công", new { orderItemDetailId = orderItemDetail.OrderItemDetailId });
        }

        // Lấy OrderItemDetails theo OrderItemId
        public async Task<IBusinessResult> GetOrderItemDetailsByOrderItemIdAsync(int orderItemId)
        {
            var orderItemDetails = await _unitOfWork.OrderItemDetailRepository.GetOrderItemDetailsByOrderItemIdAsync(orderItemId);
            if (orderItemDetails == null || !orderItemDetails.Any())
                return new BusinessResult(Const.FAIL_READ_CODE, "Không tìm thấy chi tiết cho OrderItem này");

            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, orderItemDetails);
        }

        // Cập nhật OrderItemDetail
        public async Task<IBusinessResult> UpdateOrderItemDetailAsync(int orderItemDetailId, string detailKey, string detailValue, int quantity, decimal unitPrice)
        {
            var orderItemDetail = await _unitOfWork.OrderItemDetailRepository.GetByIdAsync(orderItemDetailId);
            if (orderItemDetail == null)
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Chi tiết đơn hàng không tồn tại");

            orderItemDetail.DetailKey = detailKey;
            orderItemDetail.DetailValue = detailValue;
            orderItemDetail.Quantity = quantity;
            orderItemDetail.UnitPrice = unitPrice;

            await _unitOfWork.OrderItemDetailRepository.UpdateAsync(orderItemDetail);
            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, orderItemDetail);
        }

        // Xóa OrderItemDetail
        public async Task<IBusinessResult> DeleteOrderItemDetailAsync(int orderItemDetailId)
        {
            var orderItemDetail = await _unitOfWork.OrderItemDetailRepository.GetByIdAsync(orderItemDetailId);
            if (orderItemDetail == null)
                return new BusinessResult(Const.FAIL_DELETE_CODE, "Chi tiết đơn hàng không tồn tại");

            await _unitOfWork.OrderItemDetailRepository.RemoveAsync(orderItemDetail);
            return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Chi tiết đơn hàng đã được xóa");
        }
    }

}
