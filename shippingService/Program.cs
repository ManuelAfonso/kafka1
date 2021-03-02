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

            KafkaHelper.Consume<Order>("GROUP-SHIPPING", Topics.OrderValidated, Process);
            //KafkaHelper.Consume<Order>("GROUP-FOR-ALL", Topics.OrderValidated, Process);

        }

        private static void Process(Order order)
        {
            try
            {
                order.Ship();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
