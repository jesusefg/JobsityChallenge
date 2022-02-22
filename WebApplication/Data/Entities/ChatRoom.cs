using Microsoft.AspNetCore.Identity;

namespace WebApplication.Data.Entities
{
    public class ChatRoom
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CreatedById { get; set; }

        public virtual IdentityUser CreatedBy { get; set; }
    }
}
