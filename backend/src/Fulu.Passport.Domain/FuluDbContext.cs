using Microsoft.EntityFrameworkCore;

namespace Fulu.Passport.Domain
{
    public class FuluDbContext : BaseDbContext
    {
        public FuluDbContext()
        {

        }

        public FuluDbContext(DbContextOptions<FuluDbContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
