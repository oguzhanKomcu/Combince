using Combince.Modules.Users.Core.Features.Users.Commands.LoginUser;
using Combince.Modules.Users.Core.Features.Users.Commands.LogoutUser;
using Combince.Modules.Users.Core.Features.Users.Commands.RefreshTokenUser; // Yeni komutun isim uzayı
using Combince.Modules.Users.Core.Features.Users.Commands.RegisterUser;
using Combince.Modules.Users.Core.Features.Users.Commands.UpdatePassword;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Combince.Host.Api.Controllers;

/// <summary>
/// Kullanıcı kayıt, giriş, profil ve oturum yenileme işlemlerini yöneten merkezi API kontrolör sınıfı.
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Sisteme yeni bir kullanıcı kaydeder.
    /// </summary>
    /// <param name="command">Kullanıcı kayıt bilgilerini içeren komut nesnesi.</param>
    /// <param name="cancellationToken">İşlem iptal token'ı.</param>
    /// <returns>Yeni oluşturulan kullanıcının benzersiz sistem kimliğini (<see cref="Guid"/>) döner.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var userId = await _mediator.Send(command, cancellationToken);
        return Ok(userId);
    }

    /// <summary>
    /// Kullanıcı kimlik bilgilerini doğrulayarak sisteme giriş yapar ve JWT token paketi üretir.
    /// </summary>
    /// <param name="command">E-posta/Kullanıcı adı ve şifre bilgilerini içeren giriş komut nesnesi.</param>
    /// <param name="cancellationToken">İşlem iptal token'ı.</param>
    /// <returns>Erişim token'ı ve yenileme token'ı içeren <see cref="LoginResponse"/> nesnesini döner.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Süresi dolmuş Access Token ve mevcut Refresh Token'ı doğrulayarak kullanıcıya yepyeni bir token paketi sunar.
    /// </summary>
    /// <param name="command">Eski Access Token ve veritabanında eşleşecek Refresh Token bilgilerini içeren komut nesnesi.</param>
    /// <param name="cancellationToken">İşlem iptal token'ı.</param>
    /// <returns>Yeni üretilen <see cref="LoginResponse"/> (Access ve Refresh Token) paketini döner.</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenUserCommand command, CancellationToken cancellationToken)
    {
        // İstek doğrudan MediatR üzerinden Core/Application katmanındaki Handler'a uçuyor
        var response = await _mediator.Send(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Sadece geçerli bir JWT Access Token'a sahip olan kullanıcının kendi profil bilgilerini getirmesini sağlar.
    /// </summary>
    /// <returns>İstekte bulunan aktif kullanıcının sistemdeki profil detaylarını döner.</returns>
    /// <remarks>
    /// Bu endpoint <see cref="AuthorizeAttribute"/> ile korunmaktadır. İstek başlığında (Header) Bearer token zorunludur.
    /// </remarks>
    [Authorize]
    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("Token içinde geçerli bir kullanıcı kimliği bulunamadı.");
        }

        return Ok(new { Message = "Token geçerli! Profil kapısı açıldı.", AuthenticatedUserId = Guid.Parse(userIdClaim) });
    }

    /// <summary>
    /// Aktif oturum sahibi kullanıcının mevcut şifresini doğrulatıp yeni bir şifre belirlemesini sağlar.
    /// </summary>
    /// <param name="command">Mevcut ve yeni şifre bilgilerini içeren nesne.</param>
    /// <param name="cancellationToken">İşlem iptal token'ı.</param>
    /// <response code="200">Şifre başarıyla güncellendi.</response>
    /// <response code="400">Doğrulama hatası veya geçersiz şifre durumu.</response>
    /// <response code="401">Yetkisiz erişim (Token eksik veya geçersiz).</response>
    [Authorize]
    [HttpPut("update-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordCommand command, CancellationToken cancellationToken)
    {
        // Token içinden NameIdentifier (User ID) değerini güvenle söküyoruz
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("Geçerli bir kullanıcı kimliği bulunamadı.");
        }

        // Güvenlik gereği UserId atamasını dışarıya bırakmıyor, burada biz yapıyoruz
        command.UserId = Guid.Parse(userIdClaim);

        await _mediator.Send(command, cancellationToken);
        return Ok(new { Message = "Şifreniz başarıyla güncellendi. Güvenliğiniz için tüm diğer oturumlarınız sonlandırıldı." });
    }

    /// <summary>
    /// Güvenli çıkış mekanizması. İsteğe göre sadece mevcut cihazdaki veya tüm cihazlardaki oturumları sonlandırır.
    /// </summary>
    /// <param name="command">Mevcut Refresh Token ve tüm cihazlardan çıkış tercihini içeren nesne.</param>
    /// <param name="cancellationToken">İşlem iptal token'ı.</param>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutUserCommand command, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("Geçerli bir kullanıcı kimliği bulunamadı.");
        }

        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var commandWithToken = command with { AccessToken = accessToken, UserId = Guid.Parse(userIdClaim) };

        await _mediator.Send(commandWithToken, cancellationToken);

        return Ok(new
        {
            Message = command.LogoutAllDevices
            ? "Tüm cihazlardan başarıyla çıkış yapıldı ve oturumlar sıfırlandı."
            : "Bu cihazdan güvenle çıkış yapıldı ve erişim anahtarı kara listeye alındı."
        });
    }
}