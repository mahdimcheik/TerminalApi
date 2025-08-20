namespace TerminalApi.Interfaces
{
    public interface IJobChron
    {
        Task CleanOrders();
        void SchedulerSingleOrderCleaning(string orderId);
        void CancelScheuledJob(string orderId);
        void RemoveFinishedJobs();
        
        // RESTORED: Missing methods
        Task TrackOrder(string orderId);
        Task ExpireCheckout(string checkoutId);
        
        // NEW: SignalR cleanup method
        Task CleanupDeadSignalRConnections();
    }
}