using AutoMapper;
using Interview.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDTO>();
        CreateMap<UserDTO, User>();
        CreateMap<Role, RoleDTO>();
        CreateMap<RoleDTO, Role>();
        CreateMap<PermissionDTO, Permission>();
        CreateMap<Permission, PermissionDTO>();
        CreateMap<UpdateUserRequest, User>();
    }
}