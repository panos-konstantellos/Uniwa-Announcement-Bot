using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Uniwa.Announcement.Bot
{
    class Program
    {
        private const string _feedUrl = "http://msc-ngnda.ice.uniwa.gr/?feed=rss";

        private const string _filePath = "lastupdate.log";

        static async Task Main(string[] args)
        {
            var accessToken = Environment.GetEnvironmentVariable("DiscordToken") 
                ?? throw new ArgumentNullException("DiscordToken");

            var channelId = Environment.GetEnvironmentVariable("DiscordChannelId") 
                ?? throw new ArgumentNullException("DiscordChannelId");
        
            if(string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException("DiscordToken");
            }

            if(string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentNullException("DiscordChannelId");
            }

            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;
            var exitCode = -1;

            var lastDate = await GetDateTime(cancellationToken);

            var articles = GetArticles(_feedUrl)
                .Where(x => x.LastUpdatedTime > lastDate)
                .OrderBy(x => x.LastUpdatedTime)
                .ToList();

            var announcementHandler = new DiscordHandler(accessToken) as IDiscordHandler;

            await announcementHandler.StartAsync(async (client, cancellationToken) =>
            {
                foreach(var article in articles)
                {
                    try
                    {
                        await client.AnnounceAsync(Convert.ToUInt64(channelId), article, cancellationToken);

                        lastDate = article.LastUpdatedTime;
                    }
                    catch(Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        await SetDateTime(lastDate, cancellationToken);
                    }
                }

                exitCode = 0;
                tokenSource.Cancel();

            }, cancellationToken);
            
            try
            {
                await Task.Delay(-1, cancellationToken);
            }
            catch(Exception)
            {
                if(exitCode == 0)
                {
                    return;
                }

                throw;
            }
        }

        private static IEnumerable<Article> GetArticles(string url)
        {
            using (var reader = XmlReader.Create(url))
            {
                return SyndicationFeed.Load(reader)
                    .Items
                    .Select(x => new Article
                    {
                        Title = x.Title.Text,
                        Summary = x.Summary.Text,
                        Link = x.Links?.FirstOrDefault()?.Uri?.ToString() ?? string.Empty,
                        PublishedTime = x.PublishDate.DateTime.ToUniversalTime(),
                        LastUpdatedTime = DateTime.MinValue != x.LastUpdatedTime.DateTime.ToUniversalTime() && DateTime.MaxValue != x.LastUpdatedTime.DateTime.ToUniversalTime()
                            ? x.LastUpdatedTime.DateTime.ToUniversalTime()
                            : x.PublishDate.DateTime.ToUniversalTime(),
                    })
                    .ToList();
            }
        }

        private static async Task<DateTime> GetDateTime(CancellationToken cancellationToken)
        {
            if(!File.Exists(_filePath))
            {
                return DateTime.MinValue;
            }

            var text = await File.ReadAllTextAsync(_filePath, Encoding.UTF8, cancellationToken);

            var fileTimeUtc = Convert.ToInt64(text);

            return DateTime.FromFileTimeUtc(fileTimeUtc).ToUniversalTime();
        }

        private static async Task SetDateTime(DateTime dateTime, CancellationToken cancellationToken)
        {
            await File.WriteAllTextAsync(_filePath, dateTime.ToUniversalTime().ToFileTimeUtc().ToString(), Encoding.UTF8, cancellationToken);
        }
    }
}