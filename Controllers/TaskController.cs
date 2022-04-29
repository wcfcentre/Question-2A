using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using producer.Models;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using System.Net.Http;



//namespace producer.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class TaskController : ControllerBase
//    {
//    }
//}

namespace producer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();

        [HttpPost]
        public async Task PostAsync([FromBody] Tasks taskinfo)
        {
            var values = new Dictionary<string, string> {
                { "email", taskinfo.Email },
                { "password", taskinfo.Password }
                };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://reqres.in/api/login", content);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString = responseString.Split("\"")[1];

            if (responseString.Equals("token"))
            {
                var factory = new ConnectionFactory()
                {
                    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                    Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
                };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "TaskQueue",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);
                    string task = taskinfo.Task;
                    var taskBody = Encoding.UTF8.GetBytes(task);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "TaskQueue",
                                         basicProperties: null,
                                         body: taskBody);
                }
            }
            else
            {
                Console.WriteLine("ERROR 401");
            }
        }
    }
}
