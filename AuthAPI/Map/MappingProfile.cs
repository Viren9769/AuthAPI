using AuthAPI.DTO;
using AuthAPI.Model;
using AutoMapper;

namespace AuthAPI.Map
{
    public class MappingProfile: Profile
    {
        public MappingProfile() {

            // mapping from RegisteruserDTO to USer entity
            CreateMap<RegisterUserDTO, User>();

            // mapping user entity to logindto 
            CreateMap<User, LoginDTO>();
        }
    }
}
