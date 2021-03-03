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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Starting order finance service in 9 seconds...");
            Thread.Sleep(9000);

            KafkaHelper.Consume("GROUP-FINANCE", Topics.OrderValidated, Process);
            //KafkaHelper.Consume<Order>("GROUP-FOR-ALL", Topics.OrderValidated, Process);
        }

        private static void Process(OrderMessage orderMessage)
        {
            try
            {
                Console.WriteLine($"Processing order {orderMessage.Order.OrderId}");
                orderMessage.Order.SendToERP();
                KafkaHelper.Produce(orderMessage.Order.CreateMessage(Topics.OrderSentToERP));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
