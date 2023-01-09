using KWM.Application.Data;
using KWM.Application.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#if !DEBUG
builder.Configuration.AddJsonFile("appsettings.Production.json", true);
#endif

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddMvc(o =>
{
    o.EnableEndpointRouting = false;
});


builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")))
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Forbidden";
    }).AddGoogle(googleOptions =>
    {
        // To enable secret storage on in development environment, run these commands...
        // dotnet user-secrets init
        // dotnet user-secrets set "Authentication:Google:ClientId" "<ClientId>"
        // dotnet user-secrets set "Authentication:Google:ClientSecret" "<ClientSecret>"
        // Get the credentials from Google Developer Console 

        // *In production environment, add these credentials to appsettings section

        //Secrets load from secret storage
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        googleOptions.SignInScheme = IdentityConstants.ExternalScheme;
    });

builder.Services.AddIdentity<AppUserModel, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddTransient<ITaskRepository, TaskRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() == false)
{
    //app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseMvcWithDefaultRoute();

app.Run();
