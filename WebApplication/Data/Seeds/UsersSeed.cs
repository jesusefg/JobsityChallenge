using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace WebApplication.Data.Seeds
{
    public class UsersSeed
    {
        private readonly ApplicationDbContext _context;

        public UsersSeed(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Start()
        {
            AddKiwiUser();
        }

        private void AddKiwiUser()
        {
            if(!_context.Users.Any(x => x.UserName == "Kiwi"))
            {
                IdentityUser kiwiUser = new IdentityUser()
                {
                    Email = "Kiwi",
                    NormalizedEmail = "KIWI",
                    UserName = "Kiwi",
                    NormalizedUserName = "KIWI",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    AccessFailedCount = 0
                };

                _context.Users.Add(kiwiUser);
                _context.SaveChanges();
            }
        }
    }
}
