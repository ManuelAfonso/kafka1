using contracts;

using helper;

using System;
using System.Threading;

namespace validationService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Starting order validation service in 5 seconds...");
            Thread.Sleep(5000);

            KafkaHelper.Consume("GROUP-VALIDATION", Topics.OrderCreated, Process);
        }

        private static void Process(OrderMessage orderMessage)
        {
            try
            {
                Console.WriteLine($"Processing order {orderMessage.Order.OrderId}");
                orderMessage.Order.Validate();
                KafkaHelper.Produce(orderMessage.Order.CreateMessage(Topics.OrderValidated));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
