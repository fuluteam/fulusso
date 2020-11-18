using System;
using System.Collections.Generic;
using FuLu.Passport.Domain.Entities;
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
            modelBuilder.Entity<ClientEntity>().HasData(new List<ClientEntity>()
            {
                new ClientEntity
                {
                    ClientId =10000001,
                    Id = Guid.NewGuid().ToString("N"),
                    ClientSecret = "14p9ao1gxu4q3sp8ogk8bq4gkct59t9w",
                    FullName = "葫芦藤",
                    HostUrl = "http://localhost:8080",
                    RedirectUri = "http://localhost:8080",
                    Enabled = true
                }
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
