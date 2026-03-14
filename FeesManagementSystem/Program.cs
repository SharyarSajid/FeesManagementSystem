using FeesManagementSystem.Controllers;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
var builder = WebApplication.CreateBuilder(args);

log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
// Register Email Service
builder.Services.Configure<FeesManagementSystem.Models.EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<FeesManagementSystem.Services.IEmailSender, FeesManagementSystem.Services.EmailSender>();

try
{
    builder.Services.AddDbContext<FeesManagementSystem.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions =>
    {
        //sqlOptions.CommandTimeout(500);
        //sqlOptions.EnableRetryOnFailure(
        //    maxRetryCount: 30,
        //    maxRetryDelay: TimeSpan.FromSeconds(30),
        //    errorNumbersToAdd: null
        //);
    }));
}
catch(Exception ex)
{
    
    log.Error("An error occurred while configuring the database context.", ex);
    throw;
}


var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

builder.Services.AddIdentity<FeesManagementSystem.Models.ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<FeesManagementSystem.Data.ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});

builder.Services.AddScoped<FeesManagementSystem.Services.IFeeService, FeesManagementSystem.Services.FeeService>();
builder.Services.AddScoped<FeesManagementSystem.Services.INotificationService, FeesManagementSystem.Services.NotificationService>();
builder.Services.AddHostedService<FeesManagementSystem.Background.FeeNotificationJob>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


using (var scope = app.Services.CreateScope())
{
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<FeesManagementSystem.Data.ApplicationDbContext>();
            context.Database.Migrate();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Supervisor", "Data Entry Operator" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
