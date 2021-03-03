namespace contracts
{
    public class OrderMessage
    {
        public string Id { get; set; }
        public string Topic { get; set; }
        public Order Order { get; set; }
    }
}
