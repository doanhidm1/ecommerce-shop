using Application.Students;
using Demo.DependencyInjections;
using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IServiceA, ServiceA>();
// builder.Services.AddScoped<IServiceA, ServiceA>();
// builder.Services.AddTransient<IServiceA, ServiceA>();
// Add services to the container.
builder.Services.AddControllersWithViews();
var assembly = typeof(ApplicationDbContext).Assembly.GetName().Name;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly(assembly)));
//builder.Configuration["ConnectionStrings:DefaultConnection"] 
builder.Services.AddScoped<IStudentService, StudentService1>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<IRepository, EfRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
