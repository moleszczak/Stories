namespace Stories.Services
{
    public interface IApiClient<T> where T : class
    {
        Task<T> FetchBestStoriesIds(string url, CancellationToken cancellationToken);
    }
}