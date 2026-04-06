using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// add MVC
builder.Services.AddControllersWithViews();

// add session support for login
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// initialize the database connection string from appsettings.json
SchedulingApp.Access.DatabaseConnection.Initialize(
    app.Services.GetRequiredService<IConfiguration>());

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();