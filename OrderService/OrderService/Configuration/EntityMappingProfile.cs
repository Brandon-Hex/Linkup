using AutoMapper;
using OrderService.DTO;
using OrderService.Entities;

namespace OrderService.Configuration
{
    public class EntityMappingProfile : Profile
    {
        public EntityMappingProfile()
        {
            CreateMap<CreateOrderDTO, OrderEntity>();
            CreateMap<UpdateOrderDTO, OrderEntity>();
            CreateMap<CreateOrderDTO, SendOrderNotificationDTO>();
            CreateMap<UserDetailsDTO, SendOrderNotificationDTO>();
        }
    }
}
