using Combince.Host.Api.Extensions;
using Combince.Host.Api.Middlewares;
using Combince.Modules.Users.Core.Abstractions;
using Combince.Modules.Users.Infrastructure;
using Combince.Modules.Users.Infrastructure.Services;
using Combince.Shared.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddSingleton<ILocalizedMessageProvider, JsonMessageProvider>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Configuration.AddJsonFile("Configs/errors.json", optional: false, reloadOnChange: true);

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Combince Host API",
        Version = "v1"
    });


    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});



builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddSecurityInfrastructure(builder.Configuration);

var app = builder.Build();


app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TokenBlacklistMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Combince API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization(); 

app.MapControllers();

app.Run();