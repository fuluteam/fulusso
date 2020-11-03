using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fulu.Passport.Domain.Interface
{
    public interface IExternalService
    {
        Task<string> GetOpenId(string code, string appid, string secret);
    }
}
