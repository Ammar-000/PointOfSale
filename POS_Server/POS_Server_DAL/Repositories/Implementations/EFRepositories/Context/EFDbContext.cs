using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using POS_Domains.Models;
using POS_Server_DAL.Models;

namespace POS_Server_DAL.Repositories.Implementations.EFRepositories.Context;

public class EFDbContext : IdentityDbContext<ApplicationUserModel, ApplicationRoleModel, string>
{
    public EFDbContext(DbContextOptions<EFDbContext> options) : base(options) { }

    #region DbSets
    public DbSet<CategoryModel> Categories { get; set; }
    public DbSet<ProductModel> Products { get; set; }
    public DbSet<OrderModel> Orders { get; set; }
    public DbSet<OrderItemModel> OrderItems { get; set; }
    #endregion

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(12, 2);
        configurationBuilder.Properties<decimal?>().HavePrecision(12, 2);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        #region Relationships

        builder.Entity<ProductModel>().HasOne(p => p.Category).WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<OrderItemModel>().HasOne(oi => oi.Product).WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<OrderItemModel>().HasOne(oi => oi.Order).WithMany(o => o.OrderItems)
            .HasForeignKey(o => o.OrderId).OnDelete(DeleteBehavior.Cascade);

        #endregion   
    }

}
