using Combince.Host.Api.Extensions;
using Combince.Host.Api.Middlewares;
using Combince.Modules.Users.Infrastructure;
using Combince.Shared.Infrastructure;
using Microsoft.OpenApi;
using Serilog;
using System.Text;

// ==========================================
// 1. LOGLAMA VE BUILDER YAPILANDIRMASI
// ==========================================

var builder = WebApplication.CreateBuilder(args);

// Uygulama genelinde kurumsal loglama altyapısı olarak Serilog yapılandırılıyor
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ==========================================
// 2. .NET 10 / SWASHBUCKLE SWAGGER AYARLARI
// ==========================================

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