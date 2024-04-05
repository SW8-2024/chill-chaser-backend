using ChillChaser;
using ChillChaser.Models.DB;
using ChillChaser.Services;
using ChillChaser.Services.impl;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = "";
if (builder.Environment.IsDevelopment())
{
    connectionString = (new NpgsqlConnectionStringBuilder()
    {
        Host = "db",
        Database = "chillchaser_db",
        Username = "chillchaser_user",
        Password = "password",
        Port = 5432
    }).ToString();
} 
else
{
    connectionString = (new NpgsqlConnectionStringBuilder()
    {
        Host = "db",
        Database = "chillchaser_db",
        Username = "chillchaser_user",
        Passfile = "/run/secrets/db_passfile",
        Port = 5432
    }).ToString();
}

builder.Services.AddDbContext<CCDbContext>(
    options => options.UseNpgsql(connectionString.ToString()));

builder.Services.AddIdentityApiEndpoints<CCUser>()
    .AddEntityFrameworkStores<CCDbContext>();

builder.Services.AddAuthorization();


builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;


    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

builder.Services.AddTransient<IAppService, AppService>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddTransient<IAppUsageService, AppUsageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapIdentityApi<CCUser>();

app.UseAuthorization();

app.MapControllers();

app.Run();
