using AutoMapper;
using Google.Cloud.Firestore.V1;
using TerrariumGardenTech.Common.ResponseModel.Address;
using TerrariumGardenTech.Common.ResponseModel.Feedback;
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
        FeedbackMapping(); // <-- thêm
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
    private void FeedbackMapping()
    {
        // Ảnh feedback
        CreateMap<FeedbackImage, FeedbackImageResponse>();

        // Feedback -> FeedbackResponse
        CreateMap<Feedback, FeedbackResponse>()
            .ForMember(d => d.Images,
                o => o.MapFrom(s => s.FeedbackImages))

            // Terrarium (từ Variant → Terrarium gốc)
            .ForMember(d => d.TerrariumId,
                o => o.MapFrom(s => (int?)s.OrderItem.TerrariumVariant.TerrariumId))
            .ForMember(d => d.TerrariumName,
                o => o.MapFrom(s => s.OrderItem.TerrariumVariant.Terrarium.TerrariumName))

            // Nếu bạn chỉ muốn dùng VariantId/Name thì dùng 2 dòng dưới thay cho 2 dòng trên:
            // .ForMember(d => d.TerrariumId,   o => o.MapFrom(s => (int?)s.OrderItem.TerrariumVariantId))
            // .ForMember(d => d.TerrariumName, o => o.MapFrom(s => s.OrderItem.TerrariumVariant.Name))

            // Accessory
            .ForMember(d => d.AccessoryId,
                o => o.MapFrom(s => (int?)s.OrderItem.AccessoryId))
            .ForMember(d => d.AccessoryName,
                o => o.MapFrom(s => s.OrderItem.Accessory.Name));
    }
}