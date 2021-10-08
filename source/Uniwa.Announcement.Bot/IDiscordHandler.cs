using System;
using System.Threading;
using System.Threading.Tasks;

namespace Uniwa.Announcement.Bot
{
    public interface IDiscordHandler
    {
        Task StartAsync(Func<IDiscordClient, CancellationToken, Task> func, CancellationToken cancellationToken);
    }
}
