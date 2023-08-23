namespace Stories.Services
{
    public interface IStoryApiClient
    {
        Task<IEnumerable<int>> Fetch(int numberOfBestStories, CancellationToken cancellationToken);
    }
}