using Interview.Controllers;
using Interview.Models;
using Interview.Repositories;
using Interview.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Quartz;
using Quartz.Impl;
using Quartz.Simpl;
using Quartz.Spi;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Missing database connection string.");

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)));

// Add services to the container.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<PermissionRepository>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.NumberHandling =
        JsonNumberHandling.Strict;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
           {
               c.SwaggerDoc("v1", new OpenApiInfo { Title = "Interview API", Version = "v1" });
               c.SchemaFilter<SwaggerSkipPropertyFilter>();
           });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
