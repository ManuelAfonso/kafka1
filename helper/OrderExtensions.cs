using AutoFixture;

using contracts;

using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace helper
{
    public static class OrderExtensions
    {
        private static readonly Fixture fixture = new Fixture();

        public static Order Create()
        {
            var order = new Order
            {
                OrderId = (int)(DateTime.Now - new DateTime(2021, 1, 1)).TotalSeconds,
                Status = OrderStatus.Created,
                CustomerId = fixture.Create<int>(),
                ProductId = fixture.Create<int>()
            };

            order.SaveNewOrder();
            order.AddEvent("created");
            Console.WriteLine($"Order {order.OrderId} created");

            return order;
        }

        public static void Validate(this Order order)
        {
            order.Status = OrderStatus.Validated;
            order.UpdateOrder();
            order.AddEvent("validated");
            Console.WriteLine($"Order {order.OrderId} validated");
        }

        public static void Ship(this Order order)
        {
            var trackingCode = fixture.Create<string>();

            order.Status = OrderStatus.Shipped;
            order.UpdateOrder();
            order.AddEvent($"shipped with tracking code {trackingCode}");
            Console.WriteLine($"Order {order.OrderId} was shipped with tracking code {trackingCode}");
        }

        public static void SendToERP(this Order order)
        {
            order.FinanceId = fixture.Create<string>();
            order.UpdateOrder();
            order.AddEvent($"sentToERP with id {order.FinanceId}");
            Console.WriteLine($"Order {order.OrderId} sent to ERP with id {order.FinanceId}");
        }

        private static void AddEvent(this Order order, string description)
        {
            File.AppendAllText
                (Path.Combine(@"C:\Temp\kafka1", $"events-{order.OrderId}.txt"),
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {description} {Environment.NewLine}");
        }

        private static void SaveNewOrder(this Order order)
        {
            File.WriteAllText(
                Path.Combine(@"C:\Temp\kafka1", $"order-{order.OrderId}.json"),
                JsonSerializer.Serialize(order));
        }

        private static void UpdateOrder(this Order order)
        {
            int tries = 5;
            while (tries-- > 0)
            {
                try
                {
                    using (var fs = File.Open(Path.Combine(@"C:\Temp\kafka1", $"order-{order.OrderId}.json"),
                        FileMode.Open,
                        FileAccess.ReadWrite,
                        FileShare.None))
                    {
                        byte[] data = new byte[fs.Length];
                        fs.Read(data);
                        var orderS = JsonSerializer.Deserialize<Order>(Encoding.UTF8.GetString(data));

                        orderS.Status = order.Status;
                        orderS.FinanceId = order.FinanceId ?? orderS.FinanceId;
                        
                        fs.SetLength(0);
                        fs.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(orderS)));
                        break;
                    }
                }
                catch (IOException ex) when (ex.HResult == -2147024864)
                {
                    System.Threading.Thread.Sleep(500);
                }
            }

            if (tries < 0)
            {
                Console.WriteLine($"Unable to update status on Order {order.OrderId}");
            }
        }
    }
}
