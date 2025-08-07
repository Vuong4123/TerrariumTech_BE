using AutoMapper;
using Google.Cloud.Firestore.V1;
using TerrariumGardenTech.Common.ResponseModel.Address;
using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Common.ResponseModel.OrderItem;
using TerrariumGardenTech.Repositories.Entity;
using VNPAY.NET.Models;

namespace TerrariumGardenTech.Service.Configs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        OrderMapping();
    }

    private void OrderMapping()
    {
        CreateMap<Order, OrderResponse>().ReverseMap();
        CreateMap<OrderItem, OrderItemResponse>()
        .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity ?? 0));
        CreateMap<Payment, PaymentResponse>();
        CreateMap<Address, AddressResponse>();
    }
}