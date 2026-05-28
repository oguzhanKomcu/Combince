using Combince.Modules.Users.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Combince.Modules.Users.Core.Abstractions
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
