using Helper.DataContainers;
using Helper.Implementations;
using Helper.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using POS_Domains.Models;
using POS_Server_BLL.Implementations.BaseImplementations;
using POS_Server_BLL.Implementations.OtherImplementations;
using POS_Server_BLL.Interfaces.BaseInterfaces;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_BLL.Misc;
using POS_Server_DAL.Models;
using POS_Server_DAL.Repositories.Implementations.EFRepositories;
using POS_Server_DAL.Repositories.Implementations.EFRepositories.Context;
using POS_Server_DAL.Repositories.Interfaces;
using POS_Server_DAL.Services;
using POS_Server_DAL.Settings;
using POS_Server_PL.Models;
using POS_Server_PL.Repositories;
using POS_Server_PL.Services;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Text;

namespace POS_Server_PL;

public class Program
{
    static POSSettingsModel POSSettings { get; set; } = new();
    static string connectionString = string.Empty;

    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        ChkExternalConfigAdded(builder, builder.Configuration["SensitiveValuesPlaceholder"]!,
            "POSSettings:ConnectionStrings:DefaultConnection", "POSSettings:JwtSettings:Key");

        RegisterAndSetPOSSettings(builder);
        SetConnString();
        RegisterSerilog(builder);
        RegisterDbContext(builder);
        RegisterDALComponents(builder);
        RegisterBLLComponents(builder);
        RegisterHelperComponents(builder);
        RegisterAutoMapper(builder);
        RegisterAspNetCoreIdentity(builder);
        RegisterJWT(builder);
        RegisterSwagger(builder);
        SetPort(builder);

        builder.Services.AddControllers();
        builder.Services.AddAuthorization();

        WebApplication app = builder.Build();

        await ChkSeedData(app);

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        await app.RunAsync();
    }

    static void ChkExternalConfigAdded(WebApplicationBuilder builder, string placeHolder, params string[] keysToChk)
    {
        List<string> errors = new();
        string? value;
        foreach (string key in keysToChk)
        {
            value = builder.Configuration[key];
            if (string.IsNullOrWhiteSpace(value) || value == placeHolder)
                errors.Add($"Missing or placeholder value for configuration key: '{key}'");
        }
        if (errors.Count > 0) throw new InvalidOperationException("Critical configuration settings are not properly set, " +
            "you must configure them in user secrets or environment variables:\r\n" + string.Join("\r\n", errors));
    }

    static void RegisterAndSetPOSSettings(WebApplicationBuilder builder)
    {
        IConfigurationSection POSSettingsSection = builder.Configuration.GetSection("POSSettings");
        builder.Services.Configure<POSSettingsModel>(POSSettingsSection);

        //POSSettings = POSSettingsSection.Get<POSSettingsModel>();
        POSSettingsSection.Bind(POSSettings);
    }

    static void SetConnString()
    {
        connectionString = POSSettings.ConnectionStrings.DefaultConnection;
    }

    static void RegisterSerilog(WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.MSSqlServer(connectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "Logs",
                    AutoCreateSqlTable = true
                },
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
            .Enrich.FromLogContext()
            .CreateLogger();
        builder.Host.UseSerilog(); // Use Serilog instead of default logging
    }

    static void RegisterDbContext(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<EFDbContext>(options => options.UseSqlServer(connectionString));
    }

    static void RegisterDALComponents(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(EFBaseRepository<>));
        builder.Services.AddScoped(typeof(IHardDeletableRepository<>), typeof(EFHardDeletableRepository<>));
        builder.Services.AddScoped(typeof(ISoftDeletableRepository<>), typeof(EFSoftDeletableRepository<>));
        builder.Services.AddScoped<IImagesRepository, ASPImagesRepository>();

        builder.Services.AddScoped<IIncludeNavPropsProvider<OrderModel>, EFOrderIncludeNavPropsProvider>();
        builder.Services.Configure<ImagesSettings>(builder.Configuration.GetSection("ImagesSettings"));
        builder.Services.AddHttpContextAccessor();
    }

    static void RegisterBLLComponents(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        //builder.Services.AddScoped<IOrderItemService, OrderItemService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IUsersRolesService, UsersRolesService>();
        builder.Services.AddScoped(typeof(IImageService<>), typeof(ImageService<>));
        builder.Services.AddScoped<ISeeder, Seeder>();
    }

    static void RegisterHelperComponents(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUnitOfWork, EFUnitOfWork>();
        builder.Services.AddScoped<ICustomLogger, CustomLogger>();
        builder.Services.AddScoped(typeof(IEntityValidator<>), typeof(EntityValidator<>));
        builder.Services.AddScoped(typeof(IExpressionCombiner<>), typeof(ExpressionCombiner<>));
        builder.Services.AddScoped(typeof(IExpressionConverter<,>), typeof(ExpressionConverter<,>));
    }

    static void RegisterAutoMapper(WebApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(typeof(MappingProfile));
    }

    static void RegisterAspNetCoreIdentity(WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<ApplicationUserModel, ApplicationRoleModel>(options =>
        {
            // Configure Identity options if needed
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddEntityFrameworkStores<EFDbContext>()
        .AddDefaultTokenProviders();
    }

    static void RegisterJWT(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = POSSettings.JwtSettings.Issuer,
                ValidAudience = POSSettings.JwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(POSSettings.JwtSettings.Key))
            };
        });
    }

    static void RegisterSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    static void SetPort(WebApplicationBuilder builder)
    {
        builder.WebHost.UseUrls($"http://0.0.0.0:{POSSettings.Port}");
    }

    static async Task ChkSeedData(WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        ISeeder seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        OperationResult result = await seeder.SeedData();
        if (!result.Succeeded) throw new InvalidOperationException(CombineMessages(result.Errors));
    }

    static string CombineMessages(List<string> messages)
    {
        StringBuilder sb = new();
        foreach (string message in messages) if (!string.IsNullOrWhiteSpace(message)) sb.AppendLine(message);
        if (sb.Length > 2) sb.Replace("\r\n", "", sb.Length - 2, 2);
        return sb.ToString();
    }

}