using Microsoft.AspNetCore.Identity;
using System;
using WebApplication.Data.Interfaces;

namespace WebApplication.Data.Entities
{
    public class ChatHistory
    {
        public int Id { get; set; }

        public string SenderId { get; set; }

        public int RoomId { get; set; }

        public string Message { get; set; }

        public DateTime TimeStamp { get; set; }

        public virtual IdentityUser Sender { get; set; }

        public virtual ChatRoom Room { get; set; }
    }
}
