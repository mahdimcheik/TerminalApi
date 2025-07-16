namespace TerminalApi.Interfaces
{
    public interface ISignalConnectionManager
    {
        void AddConnection(string connectionId, string userName);
        void RemoveConnection(string connectionId);
        int GetConnectionCount();
        IReadOnlyDictionary<string, string> GetAllConnections();
    }
} 