using Microsoft.AspNetCore.Mvc;
using Stories.Model;
using Stories.Services;
using System.Net;
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
        [ProducesResponseType(typeof(IAsyncEnumerable<StoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDetails(int numberOfStories, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Handling request GetStoresDetails");

            if (numberOfStories < 1 || numberOfStories > 200)
            {
                this.logger.LogInformation("Invalid number of requested stories.");
                return BadRequest("Number of stories must be in range 1 .. 200.");
            }

            var ids = await this.storyClient.Fetch(numberOfStories, cancellationToken);

            return Ok(this.storyDetailsClient.GetDetails(ids, cancellationToken));
        }
    }
}