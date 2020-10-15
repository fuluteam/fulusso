using Fulu.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FuLu.Passport.Domain.Entities;
using FuLu.Passport.Domain.Interface.Repositories;

namespace FuLu.Passport.Domain.Repositories
{
    public class ClientRepository : EfCoreRepository<ClientEntity>, IClientRepository
    {
        public ClientRepository(DbContext context) : base(context)
        {
        }
    }
}
