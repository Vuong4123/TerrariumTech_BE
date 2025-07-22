using AutoMapper;
using Google.Cloud.Firestore.V1;
using TerrariumGardenTech.Common.ResponseModel.Order;

namespace TerrariumGardenTech.Service.Configs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        OrderMapping();
    }

    private void OrderMapping()
    {
        CreateMap<StructuredQuery.Types.Order, OrderResponse>().ReverseMap();
    }
}