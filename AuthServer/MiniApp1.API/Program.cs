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

//BirthDayRequirementHandler Sýnýfný authhandler sýnýfýnda ayaða kaldýrabilmesi için ekliyoruz
//'AddSingleton' olarak çaðýrýyosun Çünkü: uygulama ayaða kalkarken bir kere nesne oluþturulsun ve hep aynýsý kullanýlsýn
builder.Services.AddSingleton<IAuthorizationHandler, BirthDayRequirementHandler>();
//Policy-Þartname tanýmlama : city alanýný özel olarak kontrol etmemizi saðlar...
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AnkaraPolicy", policy =>
    {
        policy.RequireClaim("city", "ankara");
    });
    opts.AddPolicy("AgePolicy", policy =>
    {
        //Dinamik bir þekilde istediðin yaþý yollanýlabilir.
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
