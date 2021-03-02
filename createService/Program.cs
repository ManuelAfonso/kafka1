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
            //while (true)
            {
                Metodo2();

                Thread.Sleep(5000);
            }
        }

        static void Metodo1()
        {
            var order = OrderExtensions.Create();

            order.Validate();

            order.Ship();

            order.SendToERP();
        }

        static void Metodo2()
        {
            var order = OrderExtensions.Create();
            KafkaHelper.Produce(Topics.OrderCreated, order);
        }
    }
}
