using Microsoft.AspNetCore.Mvc;
using Stories.Model;
using Stories.Services;
using System.Runtime.CompilerServices;

namespace Stories.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly ILogger<StoriesController> logger;
        private readonly IStoryApiClient storyClient;
        private readonly IStoryDetailsApiClient storyDetailsClient;

        public StoriesController(ILogger<StoriesController> logger, IStoryApiClient storyClient, IStoryDetailsApiClient storyDetailsClient)
        {
            this.logger = logger;
            this.storyClient = storyClient;
            this.storyDetailsClient = storyDetailsClient;
        }

        [HttpGet()]
        [Route("/details/{numberOfStories}", Name = "GetStoresDetails")]
        public async IAsyncEnumerable<Story> GetDetails(int numberOfStories, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Handling request GetStoresDetails");

            var ids = await this.storyClient.Fetch(numberOfStories, cancellationToken);

            await foreach (var item in this.storyDetailsClient.GetDetails(ids, cancellationToken))
            {
                yield return item;
            }
        }
    }
}