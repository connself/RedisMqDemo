using CSRedis;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using RedisMq.Web.Hubs;
using RedisMq.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace RedisMq.Web
{
    public class RedisConfig
    {
        private static readonly string RedisConnectionString = ConfigurationManager.ConnectionStrings["RedisHosts"].ConnectionString;

        public static void RegisterRedis()
        {
            var csredis = new CSRedisClient(RedisConnectionString);
            //初始化 RedisHelper
            RedisHelper.Initialization(csredis);

            //注册订阅服务
            RegisterRedisSubcribe();
        }

        static readonly IHubContext _myHubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

        private static void RegisterRedisSubcribe()
        {
            //Task.Factory.StartNew(() =>
            //{
            //});


            var s = RedisHelper.SubscribeList("mq", (message) =>
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    _myHubContext.Clients.All.addNewMessageToPage("mq 订阅收到消息", "重试 没拿到消息");
                    return;
                }
                var model = JsonConvert.DeserializeObject<MessageModel>(message);
                //System.Console.WriteLine(" mq 订阅收到消息：" + message);
                //System.Console.WriteLine("___________________________________________________________________");
                if (model.Index < 20)
                    Thread.Sleep(1000);
                _myHubContext.Clients.Client(model.ConnectionId).addNewMessageToPage("订阅收到消息", message);
            }, 0);

            var subcribeActionMatches = FindConsumersFromControllerTypes();

            foreach (var subcribeAction in subcribeActionMatches)
            {
                SubscribeAttribute attribute = subcribeAction.Item1;
                MethodInfo methodInfo = subcribeAction.Item2;
                TypeInfo typeInfo = subcribeAction.Item3;
                object controllerClass = Activator.CreateInstance(typeInfo);

                RedisHelper.SubscribeList(attribute.Name, (message) =>
                {
                    methodInfo.Invoke(controllerClass, new object[] { message });
                }, 0);
            }
        }

        private static IEnumerable<Tuple<SubscribeAttribute, MethodInfo, TypeInfo>> FindConsumersFromControllerTypes()
        {
            var executorDescriptorList = new List<Tuple<SubscribeAttribute, MethodInfo, TypeInfo>>();

            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.ExportedTypes;
            foreach (var type in types)
            {
                var typeInfo = type.GetTypeInfo();
                if (IsController(typeInfo))
                {
                    executorDescriptorList.AddRange(GetTopicAttributesDescription(typeInfo));
                }
            }

            return executorDescriptorList;
        }

        private static IEnumerable<Tuple<SubscribeAttribute, MethodInfo, TypeInfo>> GetTopicAttributesDescription(TypeInfo typeInfo)
        {
            foreach (var method in typeInfo.DeclaredMethods)
            {
                var topicAttr = method.GetCustomAttributes<SubscribeAttribute>(true);
                var topicAttributes = topicAttr as IList<SubscribeAttribute> ?? topicAttr.ToList();

                if (!topicAttributes.Any())
                {
                    continue;
                }

                foreach (var attr in topicAttributes)
                {
                    yield return Tuple.Create(attr, method, typeInfo);
                }
            }
        }

        private static bool IsController(TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass)
            {
                return false;
            }

            if (typeInfo.IsAbstract)
            {
                return false;
            }

            if (!typeInfo.IsPublic)
            {
                return false;
            }

            return !typeInfo.ContainsGenericParameters
                   && typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase);
        }
    }
}