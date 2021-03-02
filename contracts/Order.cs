using System;
using System.Collections.Generic;

namespace contracts
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public OrderStatus Status { get; set; }
        public string FinanceId { get; set; }
    }
}
