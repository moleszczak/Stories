using Flurl;
using Flurl.Http;
using Stories.Configuration;

namespace Stories.Services
{
    public class StoryApiClient : IStoryApiClient
    {
        private const string bestStoriesUrl = "beststories.json";
        private readonly ILogger<StoryApiClient> logger;
        private readonly IStoriesApiConfiguration configuratgion;
        private readonly IApiClient<IEnumerable<int>> storiesClient;

        public StoryApiClient(ILogger<StoryApiClient> logger, IStoriesApiConfiguration configuration, IApiClient<IEnumerable<int>> storiesClient)
        {
            this.logger = logger;
            this.configuratgion = configuration;
            this.storiesClient = storiesClient;
        }

        public async ValueTask<IEnumerable<int>> FetchStories(int numberOfBestStories, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Getting list of best stories.");

            var result = await this.storiesClient.FetchBestStoriesIds(this.ConstructUrl(), cancellationToken);

            this.logger.LogInformation("Getting list of best stories [SUCCESS].");

            return result.Take(numberOfBestStories);
        }

        private string ConstructUrl()
        {
            return this.configuratgion.Url.AppendPathSegment(bestStoriesUrl);
        }
    }
}
