using System.Net;
using Combince.Host.Api.Models;
using FluentValidation;

namespace Combince.Host.Api.Middlewares;

/// <summary>
/// Uygulama genelinde fırlatılan tüm istisnaları (exception) merkezi olarak yakalayan,
/// geleneksel hataları <see cref="ErrorDetails"/> formatında, validasyon hatalarını ise 
/// <see cref="ValidationErrorDetails"/> formatında istemciye standart JSON olarak dönen middleware sınıfı.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// <see cref="ExceptionHandlingMiddleware"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    /// <param name="next">HTTP boru hattındaki (pipeline) bir sonraki istek delegesi.</param>
    /// <param name="logger">Hataları loglama mekanizmasına yazmak için kullanılan loglayıcı servis.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// HTTP isteğini boru hattı üzerinden yürütür ve olası istisnaları yakalamak üzere dinler.
    /// </summary>
    /// <param name="httpContext">Mevcut HTTP istek ve yanıt bağlamı.</param>
    /// <returns>Asenkron işlemi temsil eden görev nesnesi.</returns>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            // İsteğin normal akışına devam etmesini sağlıyoruz
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            // Sistemde herhangi bir yerde hata fırlarsa buraya düşer
            _logger.LogError(ex, "Uygulamada bir hata oluştu: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    /// <summary>
    /// Yakalanan istisnanın tipine göre (Validasyon, İş Mantığı veya Sistemsel Hata) HTTP yanıtını hazırlar, 
    /// uygun durum kodunu (Status Code) belirler ve istemciye JSON formatında hata detaylarını yazar.
    /// </summary>
    /// <param name="context">Mevcut HTTP bağlamı.</param>
    /// <param name="exception">Fırlatılan ve yakalanan temel istisna nesnesi.</param>
    /// <returns>Yanıta hata detaylarının yazılması işlemini tamamlayan görev nesnesi.</returns>
    /// <remarks>
    /// <para>1. Senaryo: Veri doğrulama (<see cref="ValidationException"/>) hatalarında HTTP 400 döner ve hatalı alanlar listelenir.</para>
    /// <para>2. Senaryo: İş mantığı (<see cref="InvalidOperationException"/>) hatalarında HTTP 400 ve doğrudan hata mesajı döner.</para>
    /// <para>3. Senaryo: Beklenmeyen sistemsel hatalarda HTTP 500 ve genel bir hata mesajı güvenli biçimde döner.</para>
    /// </remarks>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // 1. SENARYO: FluentValidation Hatalarını Yakalıyoruz (HTTP 400)
        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            // Hataları PropertyName -> Hata Mesajları şeklinde grupluyoruz
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var validationErrorDetails = new ValidationErrorDetails(
                context.Response.StatusCode,
                "Validasyon hatası oluştu.",
                errors
            );

            return context.Response.WriteAsync(validationErrorDetails.ToString());
        }

        // 2. SENARYO: Business (İş mantığı) hataları (HTTP 400)
        if (exception is InvalidOperationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return context.Response.WriteAsync(new ErrorDetails(context.Response.StatusCode, exception.Message).ToString());
        }

        // 3. SENARYO: Beklenmeyen sistemsel hatalar (HTTP 500)
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        return context.Response.WriteAsync(new ErrorDetails(
            context.Response.StatusCode,
            "Sistemsel bir hata oluştu. Lütfen teknik ekiple iletişime geçin."
        ).ToString());
    }
}