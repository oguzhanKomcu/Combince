using Combince.Modules.Users.Core.Abstractions;
using Combince.Modules.Users.Core.Features.Users.Commands.LoginUser;
using Combince.Modules.Users.Core.Features.Users.Commands.LogoutUser;
using Combince.Modules.Users.Core.Features.Users.Commands.RegisterUser;
using Combince.Modules.Users.Core.Features.Users.Commands.UpdatePassword;
using Combince.Modules.Users.Core.Features.Users.Commands.UpdateProfile;
using Combince.Modules.Users.Core.Features.Users.Queries.GetProfileByUserId;
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
    private readonly ILocalizedMessageProvider _messageProvider; 

    public UsersController(IMediator mediator, ILocalizedMessageProvider messageProvider)
    {
        _mediator = mediator;
        _messageProvider = messageProvider;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode((int)result.StatusCode, result.Error);
        }

        return Ok(result.Value);
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

    [Authorize]
    [HttpPut("update-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordCommand command, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Users:InvalidUserClaim");
        }

        var commandWithUser = command with { UserId = userId };
        var result = await _mediator.Send(commandWithUser, cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode((int)result.StatusCode, result.Error);
        }

        return Ok(result.Value);
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Logout([FromBody] LogoutUserCommand command, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Users:InvalidUserClaim");
        }

        string accessToken = string.Empty;
        var authHeader = Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            accessToken = authHeader.Substring("Bearer ".Length).Trim();
        }

        var commandWithToken = command with
        {
            AccessToken = accessToken,
            UserId = userId
        };

        var result = await _mediator.Send(commandWithToken, cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode((int)result.StatusCode, result.Error);
        }

        return Ok();
    }


    /// <summary>
    /// Aktif oturum sahibi kullanıcının profil bilgilerini (Ad Soyad, Bio, Profil Resmi) günceller.
    /// </summary>
    /// <summary>
    /// Aktif oturum sahibi kullanıcının profil bilgilerini (Ad Soyad, Bio, Profil Resmi) günceller.
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Users:InvalidUserClaim");
        }

        var commandWithUser = command with { UserId = userId };
        var result = await _mediator.Send(commandWithUser, cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode((int)result.StatusCode, result.Error);
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpGet("profile/{userId:guid}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfileById([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var query = new GetProfileByUserIdQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new { StatusCode = (int)result.StatusCode, Message = result.Error });
        }

        return Ok(result.Value);
    }


}