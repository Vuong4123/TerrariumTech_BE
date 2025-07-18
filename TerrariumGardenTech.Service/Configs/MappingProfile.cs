using AutoMapper;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.ResponseModel.Order;

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
    }

}