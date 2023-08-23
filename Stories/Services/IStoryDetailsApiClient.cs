using Stories.Model;

namespace Stories.Services
{
    public interface IStoryDetailsApiClient
    {
        Task<Story> GetDetails(int itemId, CancellationToken cancellationToken);

        IAsyncEnumerable<Story> GetDetails(IEnumerable<int> itemIds, CancellationToken cancellationToken);
    }
}