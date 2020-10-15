using Fulu.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FuLu.Passport.Domain.Entities;
using FuLu.Passport.Domain.Interface.Repositories;

namespace FuLu.Passport.Domain.Repositories
{
    public class UserRepository : EfCoreRepository<UserEntity>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context)
        {
        }
    }
}
