using contracts;

using helper;

using System;
using System.Threading;

namespace createService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("createService starting");
            //while (true)
            {
                UsingKafka();

                Thread.Sleep(5000);
            }
            Console.WriteLine("createService stopping");
        }

        static void Normal()
        {
            try
            {
                var order = OrderExtensions.Create();

                order.Validate();

                order.Ship();

                order.SendToERP();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void UsingKafka()
        {
            try
            {
                var order = OrderExtensions.Create();
                KafkaHelper.Produce(order.CreateMessage(Topics.OrderCreated));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
