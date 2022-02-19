using System;

namespace WebApplication.Models
{
    public class ChatHistoryModel
    {
        public string SenderId { get; set; }

        public string SenderName { get; set;}

        public string Message { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
