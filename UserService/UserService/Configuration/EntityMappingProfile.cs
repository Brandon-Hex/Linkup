using AutoMapper;
using UserService.DTO;
using UserService.Entities;

namespace UserService.Configuration
{
    public class EntityMappingProfile : Profile
    {
        public EntityMappingProfile()
        {
            CreateMap<CreateUserDTO, UserEntity>();
            CreateMap<UpdateUserDTO, UserEntity>();
        }
    }
}
