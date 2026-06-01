using FluentValidation;
using MediatR;

namespace Combince.Modules.Users.Infrastructure.PipelineBehaviors;

/// <summary>
/// MediatR üzerinden gönderilen her bir istek (Command/Query) için, ilişkili FluentValidation 
/// doğrulayıcılarını (Validator) otomatik olarak tetikleyen boru hattı davranış (Pipeline Behavior) sınıfı.
/// </summary>
/// <typeparam name="TRequest">Yürütülen ve <see cref="IRequest{TResponse}"/> arayüzünü uygulayan istek tipi.</typeparam>
/// <typeparam name="TResponse">İstek sonucunda dönmesi beklenen yanıt tipi.</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// <see cref="ValidationBehavior{TRequest, TResponse}"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    /// <param name="validators">Bağımlılık konteynerinden (DI) enjekte edilen, ilgili istek tipine ait doğrulayıcıların listesi.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// İsteği yakalar, tanımlı tüm kuralları asenkron olarak doğrular ve hata yoksa akışı bir sonraki adıma geçirir.
    /// </summary>
    /// <param name="request">MediatR boru hattına gönderilen mevcut istek nesnesi.</param>
    /// <param name="next">Boru hattındaki bir sonraki davranışı veya asıl işleyiciyi (Handler) tetikleyen delege.</param>
    /// <param name="cancellationToken">İşlemin iptal edilmesini dinleyen iptal token'ı.</param>
    /// <returns>Eğer doğrulama başarılı ise asıl işleyiciden (Handler) dönen yanıt nesnesini döner.</returns>
    /// <exception cref="ValidationException">
    /// Herhangi bir iş kuralı veya veri doğrulaması ihlal edildiğinde, yakalanan tüm hatalarla birlikte fırlatılır.
    /// </exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // İstek için tanımlanmış herhangi bir FluentValidation sınıfı var mı kontrol ediliyor
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            // Tanımlı tüm validator'lar eşzamanlı (paralel) olarak çalıştırılıyor
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            // Başarısız olan tüm validasyon hataları tek bir listede toplanıyor
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            // Eğer en az bir kural ihlali varsa, akış kesilerek istisna fırlatılıyor
            if (failures.Count != 0)
            {
                // Hata fırlatıyoruz, ExceptionHandlingMiddleware bunu havada kapacak!
                throw new ValidationException(failures);
            }
        }

        // Doğrulama kurallarından başarıyla geçildiyse akış bir sonraki adıma (veya asıl handler'a) aktarılıyor
        return await next();
    }
}