using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Stories.Configuration;

namespace Stories.Services
{
    public class StoryApiClient : IStoryApiClient
    {
        private const string bestStoriesUrl = "beststories.json";
        private const string cacheKey = "stories_best";

        private readonly TimeSpan cacheExpiry = TimeSpan.FromMinutes(2);
        private readonly ILogger<StoryApiClient> logger;
        private readonly IMemoryCache cache;
        private readonly IStoriesApiConfiguration configuratgion;
        private readonly AsyncRetryPolicy policy;
        private readonly int maxRetries = 2;

        public StoryApiClient(ILogger<StoryApiClient> logger, IMemoryCache cache, IStoriesApiConfiguration configuration, WaitDurationProvider delayProvider)
        {
            this.logger = logger;
            this.cache = cache;
            this.configuratgion = configuration;
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

        public async ValueTask<IEnumerable<int>> Fetch(int numberOfBestStories, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Getting list of best stories.");

            var cacheValue = this.TryGetFromCache();
            
            if (cacheValue is not null)
            {
                this.logger.LogInformation("List of best stories found in cache.");
                return cacheValue.Take(numberOfBestStories);
            }

            IFlurlClient client = new FlurlClient();

            var request = this.configuratgion.Url.AppendPathSegment(bestStoriesUrl).WithClient(client);

            IFlurlResponse response;
            try
            {
                this.logger.LogInformation("Sending request to {0}", request.Url);

                response = await policy.ExecuteAsync((c) => request.GetAsync(c), cancellationToken).ConfigureAwait(false);

                var bestStories = JsonConvert.DeserializeObject<IEnumerable<int>>(await response.GetStringAsync());

                this.cache.Set(cacheKey, bestStories, this.cacheExpiry);

                return bestStories.Take(numberOfBestStories);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get list of best stories.");
                throw;
            }
        }

        private IEnumerable<int>? TryGetFromCache()
        {
            if (this.cache.TryGetValue<IEnumerable<int>>(cacheKey, out var items))
            { 
                return items;
            }
            else
            {
                return null;
            }
        }
    }
}
