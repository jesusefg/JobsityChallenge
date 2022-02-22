using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data.Entities;

namespace WebApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        DbSet<ChatHistory> ChatHistories { get; set; }

        DbSet<ChatRoom> ChatRooms { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatHistory>()
                .HasOne(x => x.Sender);

            modelBuilder.Entity<ChatHistory>()
                .HasOne(x => x.Room);

            modelBuilder.Entity<ChatRoom>()
                .HasOne(x => x.CreatedBy);

            base.OnModelCreating(modelBuilder);
        }
    }
}
