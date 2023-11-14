using Stories.Model;
using Stories.Configuration;
using System.Runtime.CompilerServices;
using AutoMapper;
using Flurl;

namespace Stories.Services
{
    public class StoryDetailsApiClient : IStoryDetailsApiClient
    {
        private const string itemUrlPattern = "item/__ITEM_ID__.json";
        private readonly ILogger<StoryDetailsApiClient> logger;
        private readonly IApiClient<Story> storyApiClient;
        private readonly IMapper mapper;
        private readonly IStoriesApiConfiguration? configuration;

        public StoryDetailsApiClient(ILogger<StoryDetailsApiClient> logger, IApiClient<Story> storyApiClient, IMapper mapper, IStoriesApiConfiguration configuration)
        {            
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.storyApiClient = storyApiClient ?? throw new ArgumentNullException(nameof(storyApiClient));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async IAsyncEnumerable<StoryDto> GetStoriesDetails(IEnumerable<int> itemIds, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Start fetching stories details.");

            var tasks = itemIds.Select(item => this.storyApiClient.Get(this.ConstructUrl(item), cancellationToken)).ToList();

            int complete = 0, totalCount = tasks.Count;

            while (complete < totalCount)
            {
                var completeTask = await Task.WhenAny(tasks).ConfigureAwait(false);

                ++complete;

                var story = await completeTask;

                yield return this.mapper.Map<StoryDto>(story);

                tasks.Remove(completeTask);
            }

            this.logger.LogInformation("Fetching stories details done.");
        }

        private Url? ConstructUrl(int itemId)
        {
            return this.configuration?.Url?.AppendPathSegments(new[] { "item", $"{itemId}.json" });
        }
    }
}
;