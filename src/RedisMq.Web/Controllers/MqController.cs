using Newtonsoft.Json;
using RedisMq.Web.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RedisMq.Web.Controllers
{
    public class MqController : Controller
    {
        // GET: Mq
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> PublicMessage(MqRequestModel requestModel)
        {
            if (string.IsNullOrWhiteSpace(requestModel.Message))
                return RedirectToAction("Index");

            var r = await RedisHelper.RPushAsync("mq", Create());

            MessageModel Create()
            {
                return new MessageModel() { Index = requestModel.Index, AddDateTime = DateTime.Now, Text = requestModel.Message, ConnectionId = requestModel.ConnectionId };
            }

            return Json(r);
        }

        public string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        [Subscribe("测试")]
        public void TestRedisSubcribe(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<Hubs.NotificationHub>().Clients.All.addNewMessageToPage("测试 订阅收到消息", "重试 没拿到消息");
                return;
            }
            var model = JsonConvert.DeserializeObject<MessageModel>(message);
            if (model.Index < 20)
                Thread.Sleep(1000);
            Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<Hubs.NotificationHub>().Clients.Client(model.ConnectionId).addNewMessageToPage("订阅收到消息", message);
        }
    }
}