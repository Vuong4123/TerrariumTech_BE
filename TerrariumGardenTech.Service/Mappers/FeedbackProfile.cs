using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Common.RequestModel.Feedback;
using TerrariumGardenTech.Common.ResponseModel.Feedback;

namespace TerrariumGardenTech.Service.Mappers
{
    public class FeedbackProfile : Profile
    {
        public FeedbackProfile()
        {
            // Create
            CreateMap<FeedbackCreateRequest, Feedback>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            // Update (chỉ map field non-null, luôn cập nhật UpdatedAt)
            CreateMap<FeedbackUpdateRequest, Feedback>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null)
                );

            // Entity -> DTO
            CreateMap<Feedback, FeedbackResponse>()
                // Ảnh feedback
                .ForMember(d => d.Images,
                    opt => opt.MapFrom(s => s.FeedbackImages.Select(i => i.ImageUrl)))
                // Terrarium info (qua OrderItem)
                .ForMember(d => d.TerrariumId,
                    opt => opt.MapFrom(s => s.OrderItem != null ? s.OrderItem.TerrariumVariantId : 0))
                .ForMember(d => d.TerrariumName,
                    opt => opt.MapFrom(s => s.OrderItem != null && s.OrderItem.TerrariumVariant != null
                        ? s.OrderItem.TerrariumVariant.Terrarium
                        : null));
            // OrderItemId, Rating, Comment, CreatedAt, UpdatedAt map mặc định.
        }
    }
}
