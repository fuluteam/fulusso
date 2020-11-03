using Fulu.Passport.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Fulu.AutoDI;

namespace Fulu.Passport.Domain.Interface.Repositories
{
    public interface IOperatorLogRepository : IRepository<OperatorLogEntity>, IScopedAutoDIable
    {
    }
}
