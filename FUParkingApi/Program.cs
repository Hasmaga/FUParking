using FUParkingModel.DatabaseContext;
using FUParkingRepository;
using FUParkingRepository.Interface;
using FUParkingService;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Database
builder.Services.AddScoped<FUParkingDatabaseContext>();
#endregion

#region Services
builder.Services.AddScoped<IMinioService, MinioService>();
#endregion

#region Repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
#endregion

#region GoogleAuth
builder.Services.AddAuthentication(opt =>
    {
        opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddGoogle(opt =>
    {
        opt.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new Exception("ErrorGoogleClientId");
        opt.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new Exception("ErrorGoogleClientSecret");
    });
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
