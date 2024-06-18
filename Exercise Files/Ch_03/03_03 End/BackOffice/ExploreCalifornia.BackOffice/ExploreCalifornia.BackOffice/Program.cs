using System;
using System.Collections.Generic;
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
            factory.Uri = new Uri("amqp://backoffice:backoffice123@localhost:5672");
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare("backOfficeQueue", true, false, false);

            var headers = new Dictionary<string, object>
            {
                {"subject", "tour"},
                {"action", "booked"},
                {"x-match", "any"}
            };
            channel.QueueBind("backOfficeQueue", "webappExchange", "", headers);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, eventArgs) =>
            {
                var msg = Encoding.UTF8.GetString(eventArgs.Body);
                var subject = Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["subject"] as byte[]);
                var action = Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["action"] as byte[]);
                var userId = eventArgs.BasicProperties.UserId;

                Console.WriteLine($"{userId} -> {subject}.{action}: {msg}");
            };

            channel.BasicConsume("backOfficeQueue", true, consumer);

            Console.ReadLine();

            channel.Close();
            connection.Close();
        }
    }
}
