using Stories.Model;

namespace Stories.Services
{
    public interface IStoryDetailsApiClient
    {
        IAsyncEnumerable<StoryDto> GetStoriesDetails(IEnumerable<int> itemIds, CancellationToken cancellationToken);
    }
}