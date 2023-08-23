namespace Stories.Services
{
    public interface IStoryApiClient
    {
        ValueTask<IEnumerable<int>> Fetch(int numberOfBestStories, CancellationToken cancellationToken);
    }
}