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
        [Route("/stories", Name = "GetStores")]
        public Task<IEnumerable<int>> Get(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Handling request GetStores");

            return this.storyClient.Fetch(1, cancellationToken);
        }

        [HttpGet()]
        [Route("/details", Name = "GetStoresDetails")]
        public async IAsyncEnumerable<Story> GetDetails([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Handling request GetStoresDetails");

            var ids = await this.storyClient.Fetch(1, cancellationToken);

            await foreach (var item in this.storyDetailsClient.GetDetails(ids.Take(10), cancellationToken))
            {
                yield return item;
            }
        }
    }
}