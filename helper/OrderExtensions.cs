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
            order.TrackingCode = fixture.Create<string>();
            order.UpdateOrder();
            order.AddEvent($"shipped with tracking code {order.TrackingCode}");
            Console.WriteLine($"Order {order.OrderId} was shipped with tracking code {order.TrackingCode}");
        }

        public static void SendToERP(this Order order)
        {
            order.FinanceId = fixture.Create<string>();
            order.UpdateOrder();
            order.AddEvent($"sentToERP with id {order.FinanceId}");
            Console.WriteLine($"Order {order.OrderId} sent to ERP with id {order.FinanceId}");
        }

        public static OrderMessage CreateMessage(this Order order, string topic)
        {
            return new OrderMessage
            {
                Id = fixture.Create<string>(),
                Order = order,
                Topic = topic
            };
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
            UpdateFileWithLock(
                Path.Combine(@"C:\Temp\kafka1", $"order-{order.OrderId}.json"),
                text =>
                {
                    var orderFromStorage = JsonSerializer.Deserialize<Order>(text);
                    orderFromStorage.TrackingCode = order.TrackingCode ?? orderFromStorage.TrackingCode;
                    orderFromStorage.FinanceId = order.FinanceId ?? orderFromStorage.FinanceId;
                    return JsonSerializer.Serialize(orderFromStorage);
                });
        }

        private static void UpdateFileWithLock(string path, Func<string, string> contentModifier)
        {
            int tries = 5;
            while (tries-- > 0)
            {
                try
                {
                    using (var fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        byte[] data = new byte[fs.Length];
                        fs.Read(data);

                        var newText = contentModifier(Encoding.UTF8.GetString(data));

                        fs.SetLength(0);
                        fs.Write(Encoding.UTF8.GetBytes(newText));
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
                Console.WriteLine($"Unable to update file {path}");
            }
        }
    }
}
