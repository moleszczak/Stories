using Stories.Model;

namespace Stories.Services
{
    public interface IStoryDetailsApiClient
    {
        IAsyncEnumerable<StoryDto> GetDetails(IEnumerable<int> itemIds, CancellationToken cancellationToken);
    }
}