using ChillChaser;
using ChillChaser.Models.DB;
using ChillChaser.Services;
using ChillChaser.Services.impl;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<CCDbContext>(
    options => options.UseSqlite((new SqliteConnectionStringBuilder()
    {
        Mode = SqliteOpenMode.ReadWriteCreate,
        DataSource = "chillchaser.sqlite3"
    }).ToString()));
} 
else
{
    builder.Services.AddDbContext<CCDbContext>(
    options => options.UseSqlite((new NpgsqlConnectionStringBuilder()
    {
        Username = "chillchaser_user",
        Database = "chillchaser_db",
        Passfile = "/run/secrets/db_password"
    }).ToString()));
}

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

app.UseHttpsRedirection();

app.MapIdentityApi<CCUser>();

app.UseAuthorization();

app.MapControllers();

app.Run();
