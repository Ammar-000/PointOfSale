using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace POS_Server_DAL.Repositories.Implementations.EFRepositories.Context;

public class EFDbContextFactory : IDesignTimeDbContextFactory<EFDbContext>
{
    public EFDbContext CreateDbContext(string[] args)
    {
        // Read config from appsettings.json or another path
        IConfigurationRoot configuration = new ConfigurationBuilder()
            // Must be directory of project that has appsettings.json, Directory.GetCurrentDirectory() gets startup project path
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        DbContextOptionsBuilder<EFDbContext> optionsBuilder = new();
        //string connectionString = configuration.GetConnectionString("DefaultConnection");
        string connectionString = configuration.GetSection("POSSettings").GetSection("ConnectionStrings").GetValue<string>("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);

        return new EFDbContext(optionsBuilder.Options);
    }
}