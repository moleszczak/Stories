using Stories.Model;
using Flurl.Http;
using Stories.Configuration;
using Flurl;
using System.Runtime.CompilerServices;
using Polly;
using Polly.Retry;
using Microsoft.Extensions.Caching.Memory;

namespace Stories.Services
{
    public class StoryDetailsApiClient : IStoryDetailsApiClient
    {
        private static SemaphoreSlim semaphor = new SemaphoreSlim(5, 5);
        private readonly TimeSpan cacheExpiry = TimeSpan.FromMinutes(2);
        private readonly int maxRetries = 2;
        private readonly ILogger<StoryDetailsApiClient> logger;
        private readonly IMemoryCache cache;
        private readonly IStoriesApiConfiguration storiesApiConfiguration;
        private readonly AsyncRetryPolicy policy;

        public StoryDetailsApiClient(ILogger<StoryDetailsApiClient> logger, IMemoryCache cache, IStoriesApiConfiguration storiesApiConfiguration, WaitDurationProvider delayProvider)
        {
            this.logger = logger;
            this.cache = cache;
            this.storiesApiConfiguration = storiesApiConfiguration;
            this.policy = Policy
                .Handle<FlurlHttpException>()
                .WaitAndRetryAsync(maxRetries, i => delayProvider(i), (e, s, p, x) =>
                {
                    this.logger.LogWarning("Failed to fetch details of story.");
                    if (p < maxRetries)
                    {
                        this.logger.LogInformation("Next retry {0} in {1}", p, s.TotalSeconds);
                    }
                });
        }

        public async Task<Story> GetDetails(int itemId, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Getting details of story {itemId}: START.", itemId);

            try
            {
                await semaphor.WaitAsync(cancellationToken);

                this.logger.LogInformation("Getting details of story {itemId}, sending request.", itemId);

                IFlurlClient client = new FlurlClient();

                var request = this.storiesApiConfiguration.Url.AppendPathSegment($"item/{itemId}.json").WithClient(client);

                return await this.policy.ExecuteAsync(() => request.GetJsonAsync<Story>(cancellationToken));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Getting details of story {itemId}: FAILURE.", itemId);

                throw;
            }
            finally
            {
                this.logger.LogInformation("Getting details of story {itemId}: COMPLETE.", itemId);

                semaphor.Release();
            }
        }

        public async IAsyncEnumerable<Story> GetDetails(IEnumerable<int> itemIds, [EnumeratorCancellation]CancellationToken cancellationToken)
        {
            var storiesFromCache = this.GetStoriesFromCache(itemIds).ToList();

            foreach (var story in storiesFromCache)
            {
                yield return story;
            }

            var storiesToFetch = itemIds.Except(storiesFromCache.Select(item => item.Id)).ToList();

            var tasks = storiesToFetch.Select(item => this.GetDetails(item, cancellationToken)).ToList();

            int complete = 0, totalCount = storiesToFetch.Count;

            while (complete < totalCount)
            {
                var completeTask = await Task.WhenAny(tasks).ConfigureAwait(false);

                ++complete;

                var story = await completeTask;

                this.cache.Set(this.GetCacheKey(story.Id), story, cacheExpiry);
                
                yield return story;

                tasks.Remove(completeTask);
            }
        }

        private IEnumerable<Story> GetStoriesFromCache(IEnumerable<int> itemIds)
        {
            foreach (var item in itemIds)
            {
                if (this.cache.TryGetValue<Story>(this.GetCacheKey(item), out var story))
                {
                    yield return story;
                }
            }
        }

        private string GetCacheKey(int itemId)
        {
            return $"story_{itemId}";
        }
    }
}
