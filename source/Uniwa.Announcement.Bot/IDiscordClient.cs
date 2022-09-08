using System.Threading;
using System.Threading.Tasks;

namespace Uniwa.Announcement.Bot
{
    public interface IDiscordClient
    {
        Task AnnounceAsync(ulong channelId, Article article, CancellationToken cancellationToken);
    }
}
