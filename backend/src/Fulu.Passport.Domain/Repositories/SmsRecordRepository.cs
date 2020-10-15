using Fulu.EntityFrameworkCore;
using Fulu.Passport.Domain.Entities;
using Fulu.Passport.Domain.Interface.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Fulu.Passport.Domain.Repositories
{
    public class SmsRecordRepository : EfCoreRepository<SmsRecordEntity>, ISmsRecordRepository
    {
        public SmsRecordRepository(DbContext context) : base(context)
        {
        }
    }
}
