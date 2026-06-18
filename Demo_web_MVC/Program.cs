using Demo_web_MVC.Data;
using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Repository;
using Demo_web_MVC.Repository.Addresss;
using Demo_web_MVC.Repository.Admin;
using Demo_web_MVC.Repository.Birth;
using Demo_web_MVC.Repository.Carts;
using Demo_web_MVC.Repository.Category;
using Demo_web_MVC.Repository.Chat;
using Demo_web_MVC.Repository.Dashboard;
using Demo_web_MVC.Repository.Notifications;
using Demo_web_MVC.Repository.Oder;
using Demo_web_MVC.Repository.OrderRisk;
using Demo_web_MVC.Repository.Paging;
using Demo_web_MVC.Repository.Payment;
using Demo_web_MVC.Repository.Product;
using Demo_web_MVC.Repository.Search;
using Demo_web_MVC.Service;
using Demo_web_MVC.Service.Address;
using Demo_web_MVC.Service.Admin;
using Demo_web_MVC.Service.Birth;
using Demo_web_MVC.Service.Cart;
using Demo_web_MVC.Service.Category;
using Demo_web_MVC.Service.Chat;
using Demo_web_MVC.Service.Dashboard;
using Demo_web_MVC.Service.Notifications;
using Demo_web_MVC.Service.Oder;
using Demo_web_MVC.Service.Payment;
using Demo_web_MVC.Service.Product;
using Demo_web_MVC.Service.Search;
using Demo_web_MVC.Service.Sendemail;
using Demo_web_MVC.Service.Shipping;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NETCore.MailKit.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IShippingService,ShippingService>();
builder.Services.AddScoped<INotificationsService, NotificationsService>();
builder.Services.AddScoped<INotificationsRepository, NotificationsRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IChatRepository,ChatRepository>();
builder.Services.AddHostedService<BirthBackgroundService>();
builder.Services.AddScoped<IBirthRopository, BirthRopository>();
builder.Services.AddScoped<IBirthService, BirthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<OrderRiskAnalysisService>();
builder.Services.AddScoped<OrderRiskPredictor>();
builder.Services.AddScoped<OrderRiskModelTrainer>();
builder.Services.AddScoped<IOrderRiskRepository, OrderRiskRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboarService, DashboarService>();
builder.Services.AddScoped<IPagingReponsitory, PagingReponsitory>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ISearchReponsitory, SearchReponsitory>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IOderService, OderService>();
builder.Services.AddScoped<IOderRepository, OderRepository>();
builder.Services.AddScoped< ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped< IProductService,ProductService>();
builder.Services.AddScoped<IEmailServices, Sendemail>();

builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddSignalR();
builder.Services.AddDbContext<AppDatabase>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString =  
        builder.Configuration.GetConnectionString("DefaultConnection");

    options.SchemaName = "dbo";
    options.TableName = "CacheTable";
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
        options.AccessDeniedPath = "/User/Denied";
    });

builder.Services.AddAuthorization();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});
app.UseSession();
app.UseRouting();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");
    //pattern: "{controller=Chat}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");
app.Run();
