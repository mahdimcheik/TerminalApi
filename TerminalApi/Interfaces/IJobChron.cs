namespace TerminalApi.Interfaces
{
    public interface IJobChron
    {
        Task CleanOrders();
        Task TrackOrder(string orderId);
        Task ExpireCheckout(string checkoutId);
        void SchedulerSingleOrderCleaning(string orderId);
        void CancelScheuledJob(string orderId);
    }
} 