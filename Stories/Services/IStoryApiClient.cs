namespace Stories.Services
{
    public interface IStoryApiClient
    {
        ValueTask<IEnumerable<int>> FetchStories(int numberOfBestStories, CancellationToken cancellationToken);
    }
}