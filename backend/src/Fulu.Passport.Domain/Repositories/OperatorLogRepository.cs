using System;
using System.Collections.Generic;
using System.Text;
using Fulu.EntityFrameworkCore;
using Fulu.Passport.Domain.Interface.Repositories;
using Microsoft.EntityFrameworkCore;
using Fulu.Passport.Domain.Entities;

namespace Fulu.Passport.Domain.Repositories
{
    public class OperatorLogRepository : EfCoreRepository<OperatorLogEntity>, IOperatorLogRepository
    {
        public OperatorLogRepository(DbContext context) : base(context)
        {
        }
    }
}
