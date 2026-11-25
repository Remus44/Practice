using Interview.Models;
using Interview.Repositories;
using Interview.Services;
using Quartz;
using Quartz.Impl;

using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Quartz.Spi;
using Quartz.Simpl;
using Microsoft.OpenApi.Models;
using Interview.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<PermissionRepository>();
builder.Services.AddDbContext<AppDbContext>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers();
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
