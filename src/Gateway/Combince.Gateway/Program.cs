var builder = WebApplication.CreateBuilder(args);

// YARP Gateway servislerini konfigürasyondan okuyarak sisteme ekliyoruz
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseHttpsRedirection();

// Gateway'i aktif hale getiriyoruz
app.MapReverseProxy();

app.Run();