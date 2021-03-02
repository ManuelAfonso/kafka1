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

            KafkaHelper.Consume<Order>("GROUP-VALIDATION", Topics.OrderCreated, Process);
        }

        private static void Process(Order order)
        {
            try
            {
                order.Validate();
                KafkaHelper.Produce(Topics.OrderValidated, order);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
