using System;

namespace TerminalApi.Models.Payments
{
    public static class OrderExtension
    {
        public static void GenerateOrderNumber(this Order order)
        {
            order.OrderNumber = DateTime.Now.Ticks;
        }
    }

}
