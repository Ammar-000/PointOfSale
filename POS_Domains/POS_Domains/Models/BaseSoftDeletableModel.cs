namespace POS_Domains.Models;

public abstract class BaseSoftDeletableModel : BaseModel
{
    public bool IsActive { get; set; } = true;
}
