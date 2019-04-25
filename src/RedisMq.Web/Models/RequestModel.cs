namespace RedisMq.Web.Models
{
    public class MqRequestModel
    {
        public int Index { get; set; }
        public string Message { get; set; }
        public string ConnectionId { get; set; }
    }
}