using System;
using System.Collections.Generic;
using System.Text;

namespace Combince.Modules.Users.Core.Abstractions
{
    public interface IUserValidationService
    {
        Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default);
        bool IsPasswordStrongEnough(string password, out string? errorMessage);
    }
}
