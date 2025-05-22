using Helper.DataContainers;

namespace POS_Server_BLL.Interfaces.OtherInterfaces;

public interface ISeeder
{
    public Task<OperationResult> SeedData();
}
