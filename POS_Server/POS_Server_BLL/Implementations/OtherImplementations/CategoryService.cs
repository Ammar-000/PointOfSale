using AutoMapper;
using Helper.Interfaces;
using POS_Domains.DTOs;
using POS_Domains.Models;
using POS_Server_BLL.Implementations.BaseImplementations;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_DAL.Repositories.Interfaces;

namespace POS_Server_BLL.Implementations.OtherImplementations;

public class CategoryService : SoftDeletableService<CategoryModel>, ICategoryService
{
    public CategoryService(ISoftDeletableRepository<CategoryModel> repository,
        IEntityValidator<CategoryModel> validator, IMapper mapper, ICustomLogger logger,
        IUserService userService) : base(repository, validator, mapper, logger, userService) { }

    public override void RemoveNavigationProps(CategoryModel category)
    {
        category.Products = null;
    }

}
