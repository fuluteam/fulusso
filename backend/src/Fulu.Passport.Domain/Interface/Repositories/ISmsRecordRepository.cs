using Fulu.AutoDI;
using Fulu.Passport.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.Passport.Domain.Interface.Repositories
{
    public  interface ISmsRecordRepository: IRepository<SmsRecordEntity>, IScopedAutoDIable
    {
    }
}
