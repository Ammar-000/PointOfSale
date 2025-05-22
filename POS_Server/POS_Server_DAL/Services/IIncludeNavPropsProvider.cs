using POS_Domains.Models;

namespace POS_Server_DAL.Services;

public interface IIncludeNavPropsProvider<T> where T : BaseModel
{
    IQueryable<T> ApplyIncludesNavigationProps(IQueryable<T> query);
}
