using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Stories.Configuration;

namespace Stories.Services
{
    public class StoryApiClient : IStoryApiClient
    {
        private const string bestStoriesUrl = "beststories.json";

        private readonly ILogger<StoryApiClient> logger;
        private readonly IStoriesApiConfiguration configuratgion;
        private readonly AsyncRetryPolicy policy;
        private readonly int maxRetries = 2;

        public StoryApiClient(ILogger<StoryApiClient> logger, IStoriesApiConfiguration configuration, WaitDurationProvider delayProvider)
        {
            this.logger = logger;
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

        public async Task<IEnumerable<int>> Fetch(int numberOfBestStories, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Getting list of best stories.");

            IFlurlClient client = new FlurlClient();

            var request = this.configuratgion.Url.AppendPathSegment(bestStoriesUrl).WithClient(client);

            IFlurlResponse response;
            try
            {
                response = await policy.ExecuteAsync((c) => request.GetAsync(c), cancellationToken).ConfigureAwait(false);

                return JsonConvert.DeserializeObject<IEnumerable<int>>(await response.GetStringAsync());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get list of best stories.");
                throw;
            }
        }
    }
}
