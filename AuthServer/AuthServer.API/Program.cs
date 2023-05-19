using AuthServer.Core.Configuration;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using AuthServer.Data;
using AuthServer.Data.Repositories;
using AuthServer.Service.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SharedLibrary.Configurations;
using SharedLibrary.Exceptions;
using SharedLibrary.Extensions;
using SharedLibrary.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

//DI Register
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
//�ki tane generic enitity ald�g� i�in <,> virg�l koyuluyor.Ka� tane generic entity al�yorsa bir eksigi kadar virg�l koyulur..
builder.Services.AddScoped(typeof(IServiceGeneric<,>), typeof(ServiceGeneric<,>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"), sqlOptions =>
    {
        sqlOptions.MigrationsAssembly("AuthServer.Data");
    });
});

builder.Services.AddIdentity<UserApp, IdentityRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.Password.RequireNonAlphanumeric = false;


}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

// Add services to the container.

builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOption"));//AppSettings'den gelen verileri Constructora g�nderir.
builder.Services.Configure<List<Client>>(builder.Configuration.GetSection("Clients"));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
{
    var tokenOptions = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
    opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        //Datalar
        ValidIssuer = tokenOptions.Issuer,
        ValidAudience = tokenOptions.Audience[0],//Listenin 0 de�eri yeterli "www.authserver.com"
        IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOptions.SecurityKey),

        //Do�rulama
        ValidateIssuerSigningKey = true,//�mzas� Do�rulicak m� ?
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,

        //Tokan standart�nda ne kadar s�re verirsen fazladan 5 dakka �st�ne koya.Burdan s�f�rlan�r..
        ClockSkew = TimeSpan.Zero,
    };
});

builder.Services.AddControllers().AddFluentValidation(options =>
{
    //options.RegisterValidatorsFromAssemblyContaining<Program>();
    options.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
});

builder.Services.UseCustomValidationsResponse();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCustomException();
app.UseHttpsRedirection();

app.UseAuthentication();//Kimlik Doprulama
app.UseAuthorization();// Yetki Kontrol�

app.MapControllers();

app.Run();
