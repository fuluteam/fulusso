using System;
using System.Collections.Generic;
using System.Text;
using Fulu.EntityFrameworkCore;
using Fulu.Passport.Domain.Entities;
using Fulu.Passport.Domain.Interface.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fulu.Passport.Domain.Repositories
{
    public class ExternalUserRepository: EfCoreRepository<ExternalUserEntity>, IExternalUserRepository
    {
        public ExternalUserRepository(DbContext context) : base(context)
        {
        }
    }
}
