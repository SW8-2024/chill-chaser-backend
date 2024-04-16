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
using Microsoft.AspNetCore.Mvc;

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
        Host = "localhost",
        Database = "chillchaser_db",
        Username = "chillchaser_user",
        Password = "password",
        Port = 5555
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
builder.Services.AddTransient<IHeartRateService, HeartRateService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options => {
	options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
	options.RoutePrefix = "swagger";
});

app.MapIdentityApi<CCUser>();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/robots.txt", () => {
    return "User-agent: Googlebot\nDisallow: /admin/flag.txt\n\nUser-agent: *\nAllow: /\n";
}).ExcludeFromDescription();


app.MapGet("/admin/flag.txt", () => {
	return "cWatch{Ch1ll-Ch453r}";
}).ExcludeFromDescription();

string imageVersion = Environment.GetEnvironmentVariable("CC_IMAGE_VERSION") ?? "dev";

app.MapGet("/", () => {
	return $"ChillChaser API...\nVersion: {imageVersion}";
}).ExcludeFromDescription();

app.Run();

public partial class Program { }
