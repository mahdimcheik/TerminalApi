using System.Collections.Concurrent;
using System.Threading.Channels;

namespace TerminalApi.Services
{
    public class SseService
    {
        private readonly ConcurrentDictionary<string, Channel<string>> _userChannels = new();

        public ChannelReader<string> ConnectUser(string email)
        {
            _userChannels.TryRemove(email, out _);
            var channel = Channel.CreateUnbounded<string>();
            var channelList = _userChannels.TryAdd(email,channel);
            return channel.Reader;
        }

        public void DisconnectUser(string email)
        {      
            _userChannels.TryRemove(email, out _);
        }

        public async Task SendMessageToUserAsync(string email, string message)
        {
            if (_userChannels.TryGetValue(email, out var channel))
            {
                    await channel.Writer.WriteAsync(message);                
            }
        }

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var channel in _userChannels)
            {
                await channel.Value.Writer.WriteAsync(message);
            }
        }
    }
}
