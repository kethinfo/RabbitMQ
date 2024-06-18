using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ExploreCalifornia.BackOffice
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare("backOfficeQueue", true, false, false);
            channel.QueueBind("backOfficeQueue", "webappExchange", "tour.*");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, eventArgs) =>
            {
                var msg = Encoding.UTF8.GetString(eventArgs.Body);
                Console.WriteLine($"{eventArgs.RoutingKey}: {msg}");
            };

            channel.BasicConsume("backOfficeQueue", true, consumer);

            Console.ReadLine();

            channel.Close();
            connection.Close();
        }
    }
}
