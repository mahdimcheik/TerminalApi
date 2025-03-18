using System.Collections.Concurrent;
using System.Threading.Channels;

namespace TerminalApi.Services
{
    public class SseService
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary< string,Channel<string>>> _userChannels = new();

        public ChannelReader<string> ConnectUser(string email, string token)
        {

            var channel = Channel.CreateUnbounded<string>();
            var channelList = _userChannels.GetOrAdd(email, _ => new ());
            channelList.TryRemove(token, out _);
            channelList.TryAdd(token, channel);
            return channel.Reader;
        }

        public void DisconnectUser(string email,string token)
        {
            var channelList = _userChannels.GetOrAdd(email, _ => new());
            if(channelList is not null)
            {
                channelList.TryRemove(token, out _);
            }
            
        } 

        public async Task SendMessageToUserAsync(string userId, string message)
        {
            if (_userChannels.TryGetValue(userId, out var channel))
            {
                foreach (var userChannel in channel)
                {
                    //await channel.TryGetValue(userChannel, out channel).Writer.WriteAsync(message);
                    await userChannel.Value.Writer.WriteAsync(message);
                }
            }
        }
    }
}
