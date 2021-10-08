using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Uniwa.Announcement.Bot
{
    class DiscordHandler : IDiscordHandler, IDiscordClient
    {
        private readonly string _accessToken;
        private readonly DiscordSocketClient _client;

        public DiscordHandler(string accessToken)
        {
            this._client = new DiscordSocketClient();
            this._accessToken = accessToken;
        }

        async Task IDiscordHandler.StartAsync(Func<IDiscordClient, CancellationToken, Task> func, CancellationToken cancellationToken)
        {
            if(func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            this._client.Ready += async () => await func.Invoke(this, cancellationToken);

            await this._client.LoginAsync(TokenType.Bot, this._accessToken);
            await this._client.StartAsync();
        }

        async Task IDiscordClient.AnnounceAsync(ulong channelId, string message, CancellationToken cancellationToken)
        {
            var channel =  this._client.GetChannel(channelId);

            if(channel is not IMessageChannel _channel)
            {
                throw new InvalidCastException($"{nameof(channel)} is not assignable from {nameof(IMessageChannel)}");
            }

            await _channel.SendMessageAsync(message, false, null, new RequestOptions
            {
                RetryMode = RetryMode.AlwaysRetry,
                CancelToken = cancellationToken,
                Timeout = Convert.ToInt32(TimeSpan.FromSeconds(30).TotalMilliseconds)
            });
        }
    }
}
