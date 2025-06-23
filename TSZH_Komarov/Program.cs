using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using TSZH_Komarov.Data;
using TSZH_Komarov.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddHttpContextAccessor();

//cache
builder.Services.AddMemoryCache();

//services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AnnouncementService>();
builder.Services.AddScoped<MeterReadingsService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<VotingService>();
builder.Services.AddScoped<ServicesService>();
builder.Services.AddScoped<ForumService>();
builder.Services.AddScoped<NotificationService>();


builder.Services.AddSingleton<TelegramService>();

builder.Services.AddHostedService<TelegramPollingService>();
builder.Services.AddHostedService<ReminderBackgroundService>();


//auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "tszh_session2";
        options.LoginPath = "/User/Login";
        options.AccessDeniedPath = "/Home/Error";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

//db
builder.Services.AddDbContext<TszhKomarovContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
