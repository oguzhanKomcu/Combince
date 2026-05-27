using System;
using System.Collections.Generic;
using System.Text;

namespace Combince.Modules.Users.Core.Abstractions
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
