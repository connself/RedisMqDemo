using System;

namespace RedisMq.Web.Models
{
    public class MessageModel
    {
        public int Index { get; set; }
        public string ConnectionId { get; set; }
        public string Text { get; set; }
        public DateTime AddDateTime { get; set; }
    }
}