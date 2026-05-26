using Combince.Shared.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Loglama Altyapısının Kurulması
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Controller Desteğinin Eklenmesi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Yazdığımız Ortak Altyapının Tek Satırda Kaydedilmesi (SQL, Mongo, Redis, MassTransit)
builder.Services.AddSharedInfrastructure(builder.Configuration);

/* İlerleyen fazlarda modülleri yazdıkça buraya şu metotları ekleyeceğiz:
   builder.Services.AddUsersModule();
   builder.Services.AddPostsModule();
*/

var app = builder.Build();

// Geliştirme ortamında Swagger'ı aktif ediyoruz
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();