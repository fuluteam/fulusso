using Fulu.AutoDI;
using Microsoft.EntityFrameworkCore;
using FuLu.Passport.Domain.Entities;

namespace FuLu.Passport.Domain.Interface.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUserRepository : IRepository<UserEntity>, IScopedAutoDIable
    {
    }
}
