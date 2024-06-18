using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace ExploreCalifornia.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        [HttpPost]
        [Route("Book")]
        public IActionResult Book()
        {
            var tourname = Request.Form["tourname"];
            var name = Request.Form["name"];
            var email= Request.Form["email"];
            var needsTransport = Request.Form["transport"] == "on";

            var message = $"{tourname};{name};{email}";
            var headers = new Dictionary<string, object>
            {
                {"subject", "tour"},
                {"action", "booked"}
            };

            SendMessage(headers, message);

            if (needsTransport)
            {
                var needsTransportHeaders = new Dictionary<string, object>
                {
                    {"subject", "transport"},
                    {"action", "booked"}
                };

                SendMessage(needsTransportHeaders, message);
            }

            return Redirect($"/BookingConfirmed?tourname={tourname}&name={name}&email={email}");
        }

        [HttpPost]
        [Route("Cancel")]
        public IActionResult Cancel()
        {
            var tourname = Request.Form["tourname"];
            var name = Request.Form["name"];
            var email = Request.Form["email"];
            var cancelReason = Request.Form["reason"];

            var message = $"{tourname};{name};{email};{cancelReason}";
            var headers = new Dictionary<string, object>
            {
                {"subject", "tour"},
                {"action", "canceled"}
            };

            SendMessage(headers, message);

            return Redirect($"/BookingCanceled?tourname={tourname}&name={name}");
        }

        private void SendMessage(IDictionary<string, object> headers, string message)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://webapp:webapp123@localhost:5672");
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var bytes = Encoding.UTF8.GetBytes(message);
            var props = new BasicProperties();
            props.Headers = headers;
            props.UserId = "webapp";
            channel.BasicPublish("webappExchange", "", props, bytes);

            channel.Close();
            connection.Close();
        }
    }
}