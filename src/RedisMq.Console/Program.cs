using System.Configuration;

namespace RedisMq.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var csredis = new CSRedis.CSRedisClient(RedisConnectionString);
            //初始化 RedisHelper
            RedisHelper.Initialization(csredis);

            var s = RedisHelper.SubscribeList("mq", (string message) =>
             {
                 System.Console.WriteLine(" mq 订阅收到消息：" + message);
                 System.Console.WriteLine("___________________________________________________________________");
             }, 0);


            System.Console.WriteLine("频道【mq】订阅客户端开始接收消息 ~~~");
            System.Console.WriteLine("___________________________________________________________________");
            System.Console.ReadLine();
        }

        private static readonly string RedisConnectionString = ConfigurationManager.ConnectionStrings["RedisHosts"].ConnectionString;
    }
}
