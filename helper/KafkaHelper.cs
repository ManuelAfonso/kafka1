using Confluent.Kafka;

using contracts;

using System;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace helper
{
    public static class KafkaHelper
    {
        private static readonly string Server = "localhost:9092";

        public static void Produce(OrderMessage message)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = Server
            };

            Action<DeliveryReport<string, OrderMessage>> handler = r =>
                Console.WriteLine(!r.Error.IsError
                    ? $"Delivered message to {r.TopicPartitionOffset} on topic {r.TopicPartition}"
                    : $"Delivery Error: {r.Error.Reason}");

            using var p = new ProducerBuilder<string, OrderMessage>(config).SetValueSerializer(new Serializer<OrderMessage>()).Build();
            p.Produce(
                message.Topic,
                new Message<string, OrderMessage> { Key = message.Id, Value = message },
                handler);
            p.Flush(TimeSpan.FromSeconds(10));
        }

        public static void Consume(string groupName, string topicName, Action<OrderMessage> processor)
        {
            var conf = new ConsumerConfig
            {
                GroupId = groupName,
                BootstrapServers = Server,
                // Note: The AutoOffsetReset property determines the start offset in the event
                // there are not yet any committed offsets for the consumer group for the
                // topic/partitions of interest. By default, offsets are committed
                // automatically, so in this example, consumption will only start from the
                // earliest message in the topic 'my-topic' the first time you run the program.
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var c = new ConsumerBuilder<Ignore, OrderMessage>(conf).SetValueDeserializer(new Deserializer<OrderMessage>()).Build();

            c.Subscribe(topicName);

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = c.Consume(cts.Token);
                        Console.WriteLine($"Consumed message at: '{cr.TopicPartitionOffset}'.");
                        processor(cr.Message.Value);
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error occured: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                c.Close();
            }
        }
    }

    public class Serializer<T> : ISerializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        }
    }

    public class Deserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(data));
        }
    }
}
