using contracts;

using helper;

using System;
using System.Threading;

namespace shippingService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Starting order shipping service in 11 seconds...");
            Thread.Sleep(11000);

            KafkaHelper.Consume("GROUP-SHIPPING", Topics.OrderValidated, Process);
            //KafkaHelper.Consume<Order>("GROUP-FOR-ALL", Topics.OrderValidated, Process);
        }

        private static void Process(OrderMessage orderMessage)
        {
            try
            {
                Console.WriteLine($"Processing order {orderMessage.Order.OrderId}");
                orderMessage.Order.Ship();
                KafkaHelper.Produce(orderMessage.Order.CreateMessage(Topics.OrderShipped));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
