using Stories.Model;
using Flurl.Http;
using Stories.Configuration;
using Flurl;
using System.Runtime.CompilerServices;
using Polly;
using Polly.Retry;
using Microsoft.Extensions.Caching.Memory;
using AutoMapper;

namespace Stories.Services
{
    public class StoryDetailsApiClient : IStoryDetailsApiClient
    {
        private static SemaphoreSlim semaphor = new SemaphoreSlim(5, 5);
        private readonly TimeSpan cacheExpiry = TimeSpan.FromMinutes(2);
        private readonly int maxRetries = 2;
        private readonly ILogger<StoryDetailsApiClient> logger;
        private readonly IMemoryCache cache;
        private readonly IMapper mapper;
        private readonly IStoriesApiConfiguration storiesApiConfiguration;
        private readonly AsyncRetryPolicy policy;

        public StoryDetailsApiClient(ILogger<StoryDetailsApiClient> logger, IMemoryCache cache, IMapper mapper, IStoriesApiConfiguration storiesApiConfiguration, WaitDurationProvider delayProvider)
        {
            this.logger = logger;
            this.cache = cache;
            this.mapper = mapper;
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

                using IFlurlClient client = new FlurlClient();

                var request = this.storiesApiConfiguration.Url.AppendPathSegment($"item/{itemId}.json").WithClient(client);

                var story = await this.policy.ExecuteAsync(() => request.GetJsonAsync<Story>(cancellationToken));

                this.logger.LogInformation("Getting details of story {itemId}: COMPLETE.", itemId);

                return story;
            }
            catch (FlurlHttpException ex)
            {
                this.logger.LogError("Stories api responded with {0}. {1}", ex.StatusCode, ex.Message);

                throw;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Getting details of story {itemId}: FAILURE.", itemId);

                throw;
            }
            finally
            {
                semaphor.Release();
            }
        }

        public async IAsyncEnumerable<StoryDto> GetDetails(IEnumerable<int> itemIds, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var storiesFromCache = this.GetStoriesFromCache(itemIds).ToList();

            foreach (var story in storiesFromCache)
            {
                yield return this.mapper.Map<StoryDto>(story);
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

                yield return this.mapper.Map<StoryDto>(story);

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
