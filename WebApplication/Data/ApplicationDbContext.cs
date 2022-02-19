using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WebApplication.Data.Entities;

namespace WebApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        DbSet<ChatHistory> ChatHistories { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatHistory>()
                .HasOne(x => x.Sender);

            base.OnModelCreating(modelBuilder);
        }
    }
}
