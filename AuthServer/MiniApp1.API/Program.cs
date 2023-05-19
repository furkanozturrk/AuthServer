using Microsoft.AspNetCore.Authorization;
using MiniApp1.API.Requirements;
using SharedLibrary.Configurations;
using SharedLibrary.Extensions;
using static MiniApp1.API.Requirements.BirthDayRequirement;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOption"));
var tokenOptions = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
builder.Services.AddCustomTokenAuth(tokenOptions);

//BirthDayRequirementHandler S�n�fn� authhandler s�n�f�nda aya�a kald�rabilmesi i�in ekliyoruz
//'AddSingleton' olarak �a��r�yosun ��nk�: uygulama aya�a kalkarken bir kere nesne olu�turulsun ve hep ayn�s� kullan�ls�n
builder.Services.AddSingleton<IAuthorizationHandler, BirthDayRequirementHandler>();
//Policy-�artname tan�mlama : city alan�n� �zel olarak kontrol etmemizi sa�lar...
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AnkaraPolicy", policy =>
    {
        policy.RequireClaim("city", "ankara");
    });
    opts.AddPolicy("AgePolicy", policy =>
    {
        //Dinamik bir �ekilde istedi�in ya�� yollan�labilir.
        policy.Requirements.Add(new BirthDayRequirement(18));
    });
});

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
