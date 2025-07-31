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
            // Map khi tạo mới
            CreateMap<FeedbackCreateRequest, Feedback>()
                .ForMember(dest => dest.CreatedAt,
                           opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt,
                           opt => opt.MapFrom(_ => DateTime.UtcNow));

            // Map khi update: chỉ override UpdatedAt và những field non‑null
            CreateMap<FeedbackUpdateRequest, Feedback>()
                .ForMember(dest => dest.UpdatedAt,
                           opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null)
                );

            // Map entity → DTO trả về, gom List<string> từ FeedbackImages
            CreateMap<Feedback, FeedbackResponse>()
                .ForMember(dest => dest.Images,
                           opt => opt.MapFrom(src => src.FeedbackImages
                                                         .Select(i => i.ImageUrl)));
        }
    }
}
