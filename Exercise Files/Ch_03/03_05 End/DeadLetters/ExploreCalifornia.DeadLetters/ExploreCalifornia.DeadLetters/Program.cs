using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ExploreCalifornia.DeadLetters
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://backoffice:backoffice123@localhost:5672");
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare("DLX", ExchangeType.Direct, true, false);
            channel.QueueDeclare("deadLetters", true, false, false);
            channel.QueueBind("deadLetters", "DLX", "");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, eventArgs) =>
            {
                var message = Encoding.UTF8.GetString(eventArgs.Body);
                var deathReasonBytes = eventArgs.BasicProperties.Headers["x-first-death-reason"] as byte[];
                var deathReason = Encoding.UTF8.GetString(deathReasonBytes);
                Console.WriteLine($"Deadletter: {message}. Reason: {deathReason}");
            };

            channel.BasicConsume("deadLetters", true, consumer);

            Console.ReadLine();

            channel.Close();
            connection.Close();
        }
    }
}
