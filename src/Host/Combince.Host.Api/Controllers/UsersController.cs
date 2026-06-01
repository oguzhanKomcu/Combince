using System.Security.Claims;
using Combince.Modules.Users.Core.Features.Users.Commands.LoginUser;
using Combince.Modules.Users.Core.Features.Users.Commands.RegisterUser;
using Combince.Modules.Users.Core.Features.Users.Commands.RefreshTokenUser; // Yeni komutun isim uzayı
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
}