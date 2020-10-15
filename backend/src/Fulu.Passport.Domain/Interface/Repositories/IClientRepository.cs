using Fulu.AutoDI;
using Microsoft.EntityFrameworkCore;
using FuLu.Passport.Domain.Entities;

namespace FuLu.Passport.Domain.Interface.Repositories
{
    public interface IClientRepository : IRepository<ClientEntity>, IScopedAutoDIable
    {
    }
}
