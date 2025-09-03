using AutoMapper;
using E_CommerceSystem.Models;
using static E_CommerceSystem.Models.UserDTO;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserReadDto>();
        CreateMap<User, UserReadDto>();
        CreateMap<UserRegisterDto, User>()
            .ForMember(d => d.Password, o => o.Ignore()); // hash elsewhere; don't map plain password

    }
}