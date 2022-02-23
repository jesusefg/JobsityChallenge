using Microsoft.EntityFrameworkCore;

namespace WebApplication.Data.Seeds
{
    public class SeedHelper
    {
        private readonly ApplicationDbContext _context;
        public SeedHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Start()
        {
            ApplyMigrations();
            new UsersSeed(_context).Start();
        }

        private void ApplyMigrations()
        {
            _context.Database.Migrate();
        }
    }
}
