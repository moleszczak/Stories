using Stories.Model;
using Flurl.Http;
using Stories.Configuration;
using Flurl;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace Stories.Services
{
    public class StoryDetailsApiClient : IStoryDetailsApiClient
    {
        private static SemaphoreSlim semaphor = new SemaphoreSlim(5, 5);
        private readonly ILogger<StoryDetailsApiClient> logger;
        private readonly IStoriesApiConfiguration storiesApiConfiguration;

        public StoryDetailsApiClient(ILogger<StoryDetailsApiClient> logger, IStoriesApiConfiguration storiesApiConfiguration)
        {
            this.logger = logger;
            this.storiesApiConfiguration = storiesApiConfiguration;
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

                return await request.GetJsonAsync<Story>(cancellationToken);
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
            var tasks = itemIds.Select(item => this.GetDetails(item, cancellationToken)).ToList();

            int complete = 0, totalCount = tasks.Count();

            while (complete < totalCount)
            {
                var completeTask = await Task.WhenAny(tasks).ConfigureAwait(false);

                ++complete;

                yield return await completeTask;

                tasks.Remove(completeTask);
            }
        }
    }
}
