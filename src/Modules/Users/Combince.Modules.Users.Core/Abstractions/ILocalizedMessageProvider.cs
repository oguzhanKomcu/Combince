namespace Combince.Modules.Users.Core.Abstractions;

public interface ILocalizedMessageProvider
{
    /// <summary>
    /// Verilen anahtara ait hata mesajını errors.json içerisinden dinamik olarak çeker.
    /// </summary>
    string GetUserMessage(string key);

    /// <summary>
    /// Belirtilen hiyerarşik gruptan (Örn: ValidationMessages) anahtara ait mesajı çeker.
    /// </summary>
    string GetMessage(string group, string key);
}