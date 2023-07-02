using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProject.TokenGenerator
{
    public interface ITokenGenerator
    {
        Task<string> GenerateToken(string email, string id);
    }
}
