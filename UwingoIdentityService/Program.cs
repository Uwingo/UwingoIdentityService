using Entity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RapositoryAppClient;
using Repositories;
using Repositories.AutoMapper;
using Serilog;
using Serilog.Events;
using UwingoIdentityService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.ConfigureServices();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureAuthorizationPolicies();
builder.Services.ConfigureEmailSenderOptions(builder.Configuration);
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.ConfigureDbContextAppClient(builder.Configuration);
//builder.Services.ConfigureMyJsonSerializer();
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var projectDirectory = Directory.GetCurrentDirectory();
var logDirectory = Path.Combine(projectDirectory, "LogFile");

// LogFile klasörünün var olup olmadığını kontrol et, yoksa oluştur
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}


// Serilog yapılandırması
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.File(Path.Combine(logDirectory, "log-.txt"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddLogging(log =>
{
    log.ClearProviders();
    log.AddSerilog(Log.Logger);
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//app.Urls.Add("http://0.0.0.0:5137");

app.Run();