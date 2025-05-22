using AutoMapper;
using POS_Domains.DTOs;
using POS_Domains.Models;
using POS_Server_DAL.Models;

namespace POS_Server_BLL.Misc;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserModel, UserDTO>().ReverseMap();
        CreateMap<RoleModel, RoleDTO>().ReverseMap();
        CreateMap<ApplicationUserModel, UserModel>();
        CreateMap<UserModel, ApplicationUserModel>().ForMember(u => u.Id, op => op.Ignore());
        CreateMap<ApplicationRoleModel, RoleModel>();
        CreateMap<RoleModel, ApplicationRoleModel>().ForMember(r => r.Id, op => op.Ignore());
        CreateMap<ApplicationUserModel, UserDTO>();
        CreateMap<ApplicationRoleModel, RoleDTO>();

        CreateMap<ProductModel, ProductDTO>().ReverseMap();
        CreateMap<CategoryModel, CategoryDTO>().ReverseMap();
        CreateMap<OrderModel, OrderDTO>().ReverseMap();
        CreateMap<OrderItemModel, OrderItemDTO>().ReverseMap();
    }
}
