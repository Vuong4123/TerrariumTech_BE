using AutoMapper;
using Google.Cloud.Firestore.V1;
using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Common.ResponseModel.OrderItem;
using TerrariumGardenTech.Repositories.Entity;

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
        .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity ?? 0))
        .ForMember(dest => dest.AccessoryQuantity, opt => opt.MapFrom(src => src.AccessoryQuantity ?? 0))
        .ForMember(dest => dest.TerrariumVariantQuantity, opt => opt.MapFrom(src => src.TerrariumVariantQuantity ?? 0));
        CreateMap<Payment, PaymentResponse>();
        CreateMap<Address, AddressResponse>();

    }
}